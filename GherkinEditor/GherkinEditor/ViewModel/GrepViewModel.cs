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

namespace Gherkin.ViewModel
{
    class GreppedFileOpener
    {
        // Example: filepath(12)      grepped line text 
        private Regex m_GrepLineRegex = new Regex(@"(.+)\((\d+)\)\s{5,}.*");

        private IGrepEditorProvider m_GrepEditorProvider;
        private EditorTabContentViewModel m_GreppedFileEditorViewModel;
        private int GreppedLineNo { get; set; }

        public GreppedFileOpener(IGrepEditorProvider grepEditorProvider, string grepLine)
        {
            m_GrepEditorProvider = grepEditorProvider;

            Match m = m_GrepLineRegex.Match(grepLine);
            if (m.Success)
            {
                string filePath = m.Groups[1].ToString();
                string line = m.Groups[2].ToString();
                GreppedLineNo = int.Parse(line);
                m_GreppedFileEditorViewModel = m_GrepEditorProvider.OpenEditor(filePath);
                EventAggregator<EditorLoadedArg>.Instance.Event += OnGrepEditorLoaded;
            }
        }

        private void OnGrepEditorLoaded(object sender, EditorLoadedArg arg)
        {
            if (arg.EditorTabContentViewModel != m_GreppedFileEditorViewModel) return;

            EventAggregator<EditorLoadedArg>.Instance.Event -= OnGrepEditorLoaded;
            m_GreppedFileEditorViewModel.ScrollCursorTo(GreppedLineNo, 1);
        }
    }

    public class GrepViewModel : NotifyPropertyChangedBase
    {
        private IGrepEditorProvider m_GrepEditorProvider;
        private IAppSettings m_AppSettings;
        private EditorTabContentViewModel m_GrepEditorViewModel;
        private GreppedFileOpener m_GreppedFileOpener;

        private string m_Folder;
        private string m_TextToGrep;
        private string m_FileExtension;
        private bool m_ShowStatus;
        private string m_Status = "";
        private int m_GrepCompletedPercent;
        private StringComparison CompareType { get; set; }

        private bool IsGrepping { get; set; }

        public GrepViewModel(IGrepEditorProvider grepEditorProvider, IAppSettings appSettings)
        {
            m_GrepEditorProvider = grepEditorProvider;
            m_AppSettings = appSettings;

            TextToGrep = RecentGreppedTexts.Count > 0 ? RecentGreppedTexts[0] : "";
            FileExtension = RecentFileExtensions.Count > 0 ? RecentFileExtensions[0] : "";
            Folder = RecentFolders.Count > 0 ? RecentFolders[0] : "";
            CompareType = IsCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
        }

        public Window GrepDiaglog { get; set; }
        public ICommand GrepCmd => new DelegateCommandNoArg(OnGrep, CanGrep);
        public ICommand FindFolderCmd => new DelegateCommandNoArg(OnFindFolder);

        public string TextToGrep
        {
            get { return m_TextToGrep; }
            set
            {
                m_TextToGrep = value;
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

        public bool IsCaseSensitive
        {
            get { return m_AppSettings.IsCaseSensitiveInFind; }
            set
            {
                m_AppSettings.IsCaseSensitiveInFind = value;
                if (value)
                    CompareType = StringComparison.CurrentCulture;
                else
                    CompareType = StringComparison.CurrentCultureIgnoreCase;
            }
        }

        public bool IsMatchWholeWord
        {
            get { return m_AppSettings.IsMatchWholeWordInFind; }
            set { m_AppSettings.IsMatchWholeWordInFind = value; }
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

        private void OnGrep()
        {
            IsGrepping = true;

            m_AppSettings.LastGreppedText = TextToGrep;
            m_AppSettings.LastUsedFileExtension = FileExtension;
            m_AppSettings.LastGreppedFolder = Folder;
            GrepCompletedPercent = 0;
            ShowStatus = true;

            EventAggregator<EditorLoadedArg>.Instance.Event += OnGrepEditorLoaded;
            m_GrepEditorViewModel = m_GrepEditorProvider.NewGrepEditor();
            m_GrepEditorViewModel.GrepViewModel = this;
        }

        private bool CanGrep()
        {
            return !string.IsNullOrWhiteSpace(TextToGrep) &&
                   !string.IsNullOrWhiteSpace(FileExtension) &&
                   Directory.Exists(Folder) &&
                   !IsGrepping;
        }

        /// <summary>
        /// Start to grep text if the grep editor has been loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private async void OnGrepEditorLoaded(object sender, EditorLoadedArg arg)
        {
            if (arg.EditorTabContentViewModel != m_GrepEditorViewModel) return;

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            EventAggregator<EditorLoadedArg>.Instance.Event -= OnGrepEditorLoaded;
            SetColorizedHighlighting();

            await Task.Run(() => Grep(Folder)); // Do grep in background

            MainEditor.TextArea.MouseDoubleClick += OnGrepLineDoubleClick;
            m_GrepEditorViewModel.SetUnmodified(GrepTextInfo());
            Mouse.OverrideCursor = Mouse.OverrideCursor = null;
            GrepDiaglog?.Close();
        }

        private string GrepTextInfo() =>
                        string.Format("Grep\"{0}\" - ({1})", TextToGrep, FileExtension);

        private void SetColorizedHighlighting()
        {
            ColorizeAvalonEdit colorizedHighlighting = new ColorizeAvalonEdit();
            colorizedHighlighting.ColorizingWord = TextToGrep;
            MainEditor.TextArea.TextView.LineTransformers.Add(colorizedHighlighting);
            colorizedHighlighting.IsCaseSensitive = IsCaseSensitive;
        }

        private TextEditor MainEditor => m_GrepEditorViewModel.MainEditor;

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
            Regex regex = GetRegEx(TextToGrep);
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

        private Regex GetRegEx(string textToFind)
        {
            RegexOptions options = RegexOptions.None;
            if (!IsCaseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }

            string pattern = Regex.Escape(textToFind);
            if (IsMatchWholeWord)
            {
                pattern = "\\b" + pattern + "\\b";
            }
            return new Regex(pattern, options);
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
                                 var line_text = string.Format("{0}({1})      {2}\n", file, line_no, line);
                                 MainEditor.AppendText(line_text);
                             }
                             ));
                    }
                    line_no++;
                }
            }
        }

        /// <summary>
        /// Open an editor to display double clicked file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGrepLineDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string grep_line = m_GrepEditorViewModel.CurrentLineText.Trim();
            m_GreppedFileOpener = new GreppedFileOpener(m_GrepEditorProvider, grep_line);
        }
    }
}
