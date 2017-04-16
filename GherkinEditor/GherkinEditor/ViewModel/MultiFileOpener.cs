using Gherkin.Util;
using Gherkin.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gherkin.ViewModel
{
    public class MultiFileOpener
    {
        public delegate void OpeningTabHandler(EditorTabItem tab);
        public delegate void LoadFilesCompletedHandler();

        public event OpeningTabHandler OpeningTabEvent;
        public event LoadFilesCompletedHandler LoadFilesCompletedEvent;

        private List<string> FilesToLoad = new List<string>();  // files to be loaded
        private bool IsLoadingEmptyTab { get; set; }

        private IAppSettings m_AppSettings;
        private ObservableCollection<EditorTabItem> TabPanels { get; set; }
        private TabControl EditorTabControl { get; set; }

        public MultiFileOpener(IAppSettings appSettings)
        {
            m_AppSettings = appSettings;
            EventAggregator<EditorViewInitializationCompleted>.Instance.Event += OnEditorViewInitializationCompleted;
        }

        public void Initialize(ObservableCollection<EditorTabItem> tabPanels, TabControl editorTabControl)
        {
            TabPanels = tabPanels;
            EditorTabControl = editorTabControl;
        }

        /// <summary>
        /// We load files one by one to avoid initialization error of EditorView
        /// </summary>
        /// <param name="files"></param>
        public void OpenFiles(string[] files)
        {
            if ((files == null) || files.Count() == 0) return;

            FilesToLoad.Clear();
            SortFiles(files);
            FilesToLoad.AddRange(files);

            LoadNextFile();
        }

         private static void SortFiles(string[] files)
        {
            Array.Sort(files, delegate (string path1, string path2)
            {
                string fileName1 = Path.GetFileName(path1);
                string fileName2 = Path.GetFileName(path2);
                return string.Compare(fileName1, fileName2, StringComparison.OrdinalIgnoreCase);
            });
        }

        public bool HaveMoreFilesToLoad => FilesToLoad.Count > 0;

        private void OnEditorViewInitializationCompleted(object sender, EditorViewInitializationCompleted arg)
        {
            if (!IsLoadingEmptyTab)
            {
                LoadNextFile();
            }

            IsLoadingEmptyTab = false;
        }

        /// <summary>
        /// Load next one file.
        /// It will be called recursively again from CurrentEditor setter after loading the file
        /// </summary>
        private void LoadNextFile()
        {
            bool isNewTab = false;
            while (HaveMoreFilesToLoad && !isNewTab)
            {
                string filePath = FilesToLoad[0];
                FilesToLoad.Remove(filePath);
                isNewTab = OpenEditorTab(filePath).Item1;
            }

            if (!HaveMoreFilesToLoad && !isNewTab)
            {
                // Important note: LoadFilesCompletedEvent should ONLY be raised
                // after all editor tabs have been loaded.
                // Otherwise the last editor could not be loaded in EditorTabContent.
                LoadFilesCompletedEvent?.Invoke();
            }
        }

        /// <summary>
        /// Open an editor tab.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>true: new tab created</returns>
        public Tuple<bool, EditorTabItem>  OpenEditorTab(string filePath)
        {
            bool isNewTab = false;
            EditorTabItem tab = TabPanels.FirstOrDefault(x => x.EditorTabContentViewModel.CurrentFilePath == filePath);
            if (tab == null)
            {
                tab = TabPanels.FirstOrDefault(x => x.EditorTabContentViewModel.IsEmptyFile());
                if (tab != null)
                {
                    tab.EditorTabContentViewModel.Load(filePath);
                    AdjustTabOrder(tab);
                }
                else
                {
                    isNewTab = true;
                    tab = CreateNewTab(filePath);
                }
            }

            OpeningTabEvent?.Invoke(tab);
            m_AppSettings.LastUsedFile = filePath;
            return Tuple.Create(isNewTab, tab);
        }

        public EditorTabItem CreateEmtyTab()
        {
            IsLoadingEmptyTab = true;
            EditorTabItem tab = NewTab(null);
            TabPanels.Add(tab);
            return tab;
        }

        private EditorTabItem CreateNewTab(string filePath)
        {
            EditorTabItem tab = NewTab(filePath);
            InsertTab(tab);
            return tab;
        }

        private EditorTabItem NewTab(string filePath)
        {
            return new EditorTabItem(EditorTabControl,
                                     new CanCloseAllDocumentsChecker(TabPanels),
                                     filePath,
                                     m_AppSettings);
        }

        private void AdjustTabOrder(EditorTabItem tab)
        {
            if (IsSorted()) return;

            TabPanels.Remove(tab);
            InsertTab(tab);
        }

        private bool IsSorted()
        {
            int i = 1;
            while (i < TabPanels.Count && Compare(TabPanels[i - 1].FileName, TabPanels[i].FileName) <= 0)
                i++;

            return (i == TabPanels.Count);
        }

        private void InsertTab(EditorTabItem tab)
        {
            string fileName = tab.FileName;
            int i = 0;
            while (i < TabPanels.Count && Compare(fileName, TabPanels[i].FileName) > 0)
                i++;
            TabPanels.Insert(i, tab);
        }

        private int Compare(string file1, string file2) =>
            string.Compare(file1, file2, StringComparison.OrdinalIgnoreCase);
    }
}
