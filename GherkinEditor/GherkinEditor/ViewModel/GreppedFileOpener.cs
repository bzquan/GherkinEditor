using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gherkin.Util;
using System.IO;

namespace Gherkin.ViewModel
{
    class GreppedFileOpener
    {
        public const string SPACES_AFTER_FILENAME = "      ";               // 6 spaces
        // Example: filepath(12)      grepped line text 
        private static Regex s_GrepLineRegex = new Regex(@"(.+)\((\d+)\)\s{6,}.*"); // 6 spaces

        private MultiFileOpener m_MultiFilesOpener;
        private EditorTabContentViewModel m_GreppedFileEditorViewModel;
        private int GreppedLineNo { get; set; }

        public static void TryOpenGreppedFile(MultiFileOpener multiFilesOpener, string grepLine)
        {
            var opener = new GreppedFileOpener();
            opener.OpenGreppedFile(multiFilesOpener, grepLine);
        }

        private GreppedFileOpener()
        {
        }

        private void OpenGreppedFile(MultiFileOpener multiFilesOpener, string grepLine)
        {
            m_MultiFilesOpener = multiFilesOpener;

            Match m = s_GrepLineRegex.Match(grepLine);
            if (!m.Success) return;

            string filePath = m.Groups[1].ToString();
            if (!File.Exists(filePath)) return;

            string line = m.Groups[2].ToString();
            GreppedLineNo = int.Parse(line);
            var tab = m_MultiFilesOpener.OpenEditorTab(filePath);
            m_GreppedFileEditorViewModel = tab.Item2.EditorTabContentViewModel;
            if (tab.Item1)
            {
                // new editor created so that we need to wait EditorLoadeded event to finish work
                EventAggregator<EditorLoadedArg>.Instance.Event += OnGrepEditorLoaded;
            }
            else
            {
                // directly move cursor because the text editor has been loaded
                ScrollCursorToLine(GreppedLineNo);
            }
        }

        private void OnGrepEditorLoaded(object sender, EditorLoadedArg arg)
        {
            if (arg.EditorTabContentViewModel != m_GreppedFileEditorViewModel) return;

            EventAggregator<EditorLoadedArg>.Instance.Event -= OnGrepEditorLoaded;
            ScrollCursorToLine(GreppedLineNo);
        }

        private void ScrollCursorToLine(int line_no) =>
                            m_GreppedFileEditorViewModel.ScrollCursorTo(line_no, 1);
    }
}
