using Gherkin.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.ViewModel
{
    public class MultiFileOpener
    {
        private List<string> FilesToLoad = new List<string>();  // files to be loaded

        private ObservableCollection<EditorTab> m_TabPanels;
        private IOpenNewEditor m_OpenNewEditor;

        public MultiFileOpener()
        {
            EventAggregator<EditorViewInitializationCompleted>.Instance.Event += OnEditorViewInitializationCompleted;
        }

        public void Config(ObservableCollection<EditorTab> tabPanels, IOpenNewEditor openNewEditor)
        {
            m_TabPanels = tabPanels;
            m_OpenNewEditor = openNewEditor;
        }

        /// <summary>
        /// We load files one by one to avoid initialization error of EditorView
        /// </summary>
        /// <param name="files"></param>
        public void OpenFiles(string[] files)
        {
            if (files == null) return;
            FilesToLoad.Clear();

            var existing_files = m_TabPanels.Select(x => x.EditorView.CurrentFilePath);
            string[] new_files = files.Where(x => !existing_files.Contains(x)).ToArray();
            FilesToLoad.AddRange(new_files);

            if  (new_files.Length > 0)
            {
                LoadNextFile();
            }
            else if (files.Length > 0)
            {
                // Make the editor, which has the last file, as current editor
                m_OpenNewEditor.OpenNewEditorTab(files.Last());
            }

        }

        public bool HaveMoreFilesToLoad => FilesToLoad.Count > 0;

        private void OnEditorViewInitializationCompleted(object sender, EditorViewInitializationCompleted arg)
        {
            LoadNextFile();
        }

        /// <summary>
        /// Load next one file.
        /// It will be called recursively again from CurrentEditor setter after loading the file
        /// </summary>
        // 
        private void LoadNextFile()
        {
            if (HaveMoreFilesToLoad)
            {
                string filePath = FilesToLoad[0];
                FilesToLoad.Remove(filePath);
                m_OpenNewEditor.OpenNewEditorTab(filePath);
            }
        }
    }
}
