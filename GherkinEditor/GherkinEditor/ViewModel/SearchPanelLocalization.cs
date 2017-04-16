using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util;
using Gherkin.Properties;
using System.IO;

namespace Gherkin.ViewModel
{
    public class SearchPanelLocalization : ICSharpCode.AvalonEdit.Search.Localization
    {
        private static Lazy<string> s_RegexNote = new Lazy<string>(() => LoadRegexNote());

        private SearchPanelLocalization()
        {
        }

        public static Lazy<SearchPanelLocalization> Instance = new Lazy<SearchPanelLocalization>(() => new SearchPanelLocalization());

        public override string SearchTermText
        {
            get { return Resources.DlgFind_SearchTermText; }
        }
        public override string ReplacementTermText
        {
            get { return Resources.DlgFind_ReplacementTermText; }
        }
        public override string MatchCaseText
        {
            get { return Resources.DlgFind_MachCase; }
        }
        public override string MatchWholeWordsText
        {
            get { return Resources.DlgFind_MachWholdWord; }
        }
        public override string UseWildCardsText
        {
            get { return Resources.DlgFind_Wildcards; }
        }
        public override string UseRegexText
        {
            get { return Resources.DlgFind_RegularExpression; }
        }
        public override string FindNextText
        {
            get { return Resources.DlgFind_FindNextToolTip; }
        }
        public override string FindPreviousText
        {
            get { return Resources.DlgFind_FindNextUpToolTip; }
        }
        public override string ToggleFindReplace
        {
            get { return Resources.DlgFind_ToggleFindReplace; }
        }
        public override string ReplaceNextText
        {
            get { return Resources.DlgFind_Replace; }
        }
        public override string ReplaceAllText
        {
            get { return Resources.DlgFind_ReplaceAll; }
        }
        public override string ReplaceAllConfirmMessage
        {
            get { return Resources.Message_ReplaceAllConfirm; }
        }
        public override string ReplaceAllConfirmTitle
        {
            get { return Resources.Message_ReplaceAllTitle; }
        }

        public override string ErrorText
        {
            get { return "Error: "; }
        }
        public override string NoMatchesFoundText
        {
            get { return Resources.DlgFind_NoMatchesFoundText; }
        }
        public override string RegexNoteTitle
        {
            get { return Resources.Message_RegexNoteTitle; }
        }
        public override string RegexNote
        {
            get { return s_RegexNote.Value; }
        }

        private static string LoadRegexNote()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var templateName = "Gherkin.ViewModel.ReleaseNote.RegexNote.html";

            using (Stream stream = assembly.GetManifestResourceStream(templateName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
