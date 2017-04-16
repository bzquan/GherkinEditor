using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using System.IO;
using Gherkin.Util;
using ICSharpCode.AvalonEdit;
using System.Windows;
using Gherkin.Model;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Rendering;

namespace Gherkin.ViewModel
{
    public class GrepViewModel : AbstractFindViewModel
    {
        private WorkAreaEditorViewModel m_WorkArea;
        private MultiFileOpener m_MultiFilesOpener;

        private string m_Folder;
        private string m_FileExtension;
        private bool m_ShowStatus;
        private string m_Status = "";
        private int m_GrepCompletedPercent;

        private bool IsGrepping { get; set; }

        public GrepViewModel(WorkAreaEditorViewModel workArea, MultiFileOpener multiFilesOpener, IAppSettings appSettings, string default_grep_text) :
            base(appSettings)
        {
            m_WorkArea = workArea;
            m_MultiFilesOpener = multiFilesOpener;
            SetDefaultGrepConditions(default_grep_text);
        }

        private void SetDefaultGrepConditions(string default_grep_text)
        {
            m_SearchCondition.IsCaseSensitive = m_AppSettings.IsCaseSensitiveInFind;
            m_SearchCondition.IsMatchWholeWord = m_AppSettings.IsMatchWholeWordInFind;
            m_SearchCondition.IsUseRegex = m_AppSettings.IsUseRegexInFind;
            m_SearchCondition.IsUseWildcards = m_AppSettings.IsUseWildcardsInFind;

            if (!string.IsNullOrEmpty(default_grep_text))
            {
                TextToFind = default_grep_text;
            }
            else
            {
                TextToFind = RecentGreppedTexts.Count > 0 ? RecentGreppedTexts[0] : "";
            }
            FileExtension = RecentFileExtensions.Count > 0 ? RecentFileExtensions[0] : "";
            Folder = RecentFolders.Count > 0 ? RecentFolders[0] : "";
        }

        public Window GrepDiaglog { get; set; }
        public ICommand GrepCmd => new DelegateCommandNoArg(OnGrep, CanGrep);
        public ICommand FindFolderCmd => new DelegateCommandNoArg(OnFindFolder);

        public RegexDocument RegexDocument { get; private set; } = new RegexDocument();

        public string TextToGrep
        {
            get { return m_SearchCondition.TextToFind; }
            set
            {
                m_SearchCondition.TextToFind = value;
                base.OnPropertyChanged();
            }
        }
        public List<string> RecentGreppedTexts
        {
            get { return m_AppSettings.LastGreppedTexts; }
        }

        public string FileExtension
        {
            get { return m_FileExtension; }
            set
            {
                m_FileExtension = value;
                base.OnPropertyChanged();
            }
        }
        public List<string> RecentFileExtensions
        {
            get { return m_AppSettings.LastFileExtensions; }
        }

        public string Folder
        {
            get { return m_Folder; }
            set
            {
                m_Folder = value;
                base.OnPropertyChanged();
            }
        }

        public List<string> RecentFolders
        {
            get { return m_AppSettings.LastGreppedFolders; }
        }

        public bool ShowStatus
        {
            get { return m_ShowStatus; }
            set
            {
                m_ShowStatus = value;
                base.OnPropertyChanged();
            }
        }

        public string Status
        {
            get { return m_Status; }
            set
            {
                m_Status = value.TrimEnd();
                base.OnPropertyChanged();
            }
        }

        public int GrepCompletedPercent
        {
            get { return m_GrepCompletedPercent; }
            set
            {
                m_GrepCompletedPercent = value;
                base.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Start grep text in files
        /// </summary>
        private async void OnGrep()
        {
            IsGrepping = true;

            BackupLastUsedText();
            GrepCompletedPercent = 0;
            ShowStatus = true;
            m_WorkArea.OnGrepStarted();

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            SetSearchHighlightingTransformer();

            await Task.Run(() => Grep(Folder)); // Do grep in background

            Mouse.OverrideCursor = Mouse.OverrideCursor = null;
            GrepDiaglog?.Close();
            m_WorkArea.OnGrepFinished();
        }

        private void BackupLastUsedText()
        {
            m_AppSettings.LastGreppedText = TextToFind;
            m_AppSettings.LastUsedFileExtension = FileExtension;
            m_AppSettings.LastGreppedFolder = Folder;
        }

        private bool CanGrep()
        {
            return m_SearchCondition.IsValidTextToFind() &&
                   !string.IsNullOrWhiteSpace(FileExtension) &&
                   Directory.Exists(Folder) &&
                   !IsGrepping;
        }

        private void SetSearchHighlightingTransformer()
        {
            m_WorkArea.SetSearchHighlightingTransformer(m_SearchCondition.GetRegEx(isForHighlighting: true));
        }

        private void OnFindFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    Folder = dialog.SelectedPath;
                }
            }
        }

        private void Grep(string targetDirectory)
        {
            List<string> files = new List<string>();
            CollectFiles(files, targetDirectory);
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => ShowStatus = false));
            Regex regex = m_SearchCondition.GetRegEx(isForHighlighting:true);
            Grep(files, regex);
        }

        private void CollectFiles(List<string> files, string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                string ext = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(ext) && FileExtension.Contains(ext))
                {
                    files.Add(fileName);
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => Status = fileName));
                }
            }
            // Recurse into subdirectories of this directory
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                CollectFiles(files, subdirectory);
            }
        }

        private void Grep(List<string> files, Regex regex)
        {
            int total = files.Count();
            int count = 0;
            foreach (var file in files)
            {
                GrepText(file, regex);

                count++;
                int percent = (count * 100) / total;
                SetCompletedPercent(percent);
            }
            SetCompletedPercent(100);
        }

        private void SetCompletedPercent(int percent)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => GrepCompletedPercent = percent));
        }

        private void GrepText(string file, Regex regex)
        {
            var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            //            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            using (var streamReader = new StreamReader(fileStream))
            {
                string line;
                int line_no = 1;
                while ((line = streamReader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(
                             new Action(() =>
                             {
                                 var line_text = string.Format("{0}({1}){2}{3}\n", file, line_no, GreppedFileOpener.SPACES_AFTER_FILENAME, line);
                                 m_WorkArea.AppendText(line_text);
                             }
                             ));
                    }
                    line_no++;
                }
            }
        }
    }
}
