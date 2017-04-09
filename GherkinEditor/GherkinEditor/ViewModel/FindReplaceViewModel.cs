using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows;

using Gherkin.Util;
using ICSharpCode.AvalonEdit;
using Gherkin.Model;
using ICSharpCode.AvalonEdit.Document;

namespace Gherkin.ViewModel
{
    public class FindReplaceViewModel : NotifyPropertyChangedBase
    {
        private IAppSettings m_AppSettings;

        private static string m_TextToFind = "";
        private static string m_ReplaceText = "";
        private static bool m_IsCaseSensitive;
        private static bool m_MatchWholeWord;
        private static bool m_IsUseRegex;
        private static bool m_IsUseWildcards;
        private static bool m_IsSearchUp;

        private TextEditor Editor { get; set; }
        private ColorizeAvalonEdit DocumentColorizingTransformer4Search { get; set; }

        public FindReplaceViewModel(TextEditor editor, IAppSettings appSettings)
        {
            Editor = editor;
            m_AppSettings = appSettings;
            InitializeProperties();
        }

        public ICommand FindNextCmd => new DelegateCommandNoArg(OnFindNext, CanExecFindReplaceCmd);
        public ICommand ReplaceCmd => new DelegateCommandNoArg(OnReplace, CanExecFindReplaceCmd);
        public ICommand ReplaceAllCmd => new DelegateCommandNoArg(OnReplaceAll, CanExecFindReplaceCmd);

        private void InitializeProperties()
        {
            m_TextToFind = Editor.TextArea.Selection.GetText();
            if (string.IsNullOrEmpty(m_TextToFind))
            {
                m_TextToFind = m_AppSettings.LastSearchedText;
            }
            else
            {
                m_AppSettings.LastSearchedText = m_TextToFind;
            }
            m_IsCaseSensitive = m_AppSettings.IsCaseSensitiveInFind;
            m_MatchWholeWord = m_AppSettings.IsMatchWholeWordInFind;
        }

        public string TextToFind
        {
            get { return m_TextToFind; }
            set
            {
                m_TextToFind = value;
                m_AppSettings.LastSearchedText = value;
                base.OnPropertyChanged();
            }
        }

        public string ReplaceText
        {
            get { return m_ReplaceText; }
            set
            {
                m_ReplaceText = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsCaseSensitive
        {
            get { return m_IsCaseSensitive; }
            set
            {
                m_IsCaseSensitive = value;
                m_AppSettings.IsCaseSensitiveInFind = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsMatchWholeWord
        {
            get { return m_MatchWholeWord; }
            set
            {
                m_MatchWholeWord = value;
                m_AppSettings.IsMatchWholeWordInFind = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsUseRegex
        {
            get { return m_IsUseRegex; }
            set
            {
                m_IsUseRegex = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsUseWildcards
        {
            get { return m_IsUseWildcards; }
            set
            {
                m_IsUseWildcards = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsSearchUp
        {
            get { return m_IsSearchUp; }
            set
            {
                m_IsSearchUp = value;
                base.OnPropertyChanged();
            }
        }

        private void OnFindNext()
        {
            SetColorizingWord(TextToFind);
            FindNext(TextToFind);
        }

        private bool CanExecFindReplaceCmd()
            => string.IsNullOrEmpty(TextToFind) == false;

        private void OnReplace()
        {
            SetColorizingWord(TextToFind);
            Regex regex = GetRegEx(TextToFind);
            string input = Editor.Text.Substring(Editor.SelectionStart, Editor.SelectionLength);
            Match match = regex.Match(input);
            if (match.Success && match.Index == 0 && match.Length == input.Length)
            {
                Editor.Document.Replace(Editor.SelectionStart, Editor.SelectionLength, ReplaceText);
            }

            FindNext(TextToFind);
        }

        private void OnReplaceAll()
        {
            SetColorizingWord(ReplaceText);
            string msg = string.Format(Properties.Resources.Message_ReplaceAllConfirm, TextToFind, ReplaceText);
            if (MessageBox.Show(Application.Current.MainWindow,
                           msg,
                           Properties.Resources.Message_ReplaceAllTitle,
                           MessageBoxButton.OKCancel,
                           MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                Regex regex = GetRegEx(TextToFind, true);
                int offset = 0;
                Editor.BeginChange();
                var matchCollection = regex.Matches(Editor.Text);
                foreach (Match match in matchCollection)
                {
                    Editor.Document.Replace(offset + match.Index, match.Length, ReplaceText);
                    offset += ReplaceText.Length - match.Length;
                }
                Editor.EndChange();

                string replace_msg = string.Format(Properties.Resources.Message_ReplaceAllResult, matchCollection.Count);
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg(replace_msg));
            }
        }

        private void SetColorizingWord(string text)
        {
            if (DocumentColorizingTransformer4Search == null)
            {
                InitializeHighlighting();
            }

            DocumentColorizingTransformer4Search.ColorizingWord = text;
            DocumentColorizingTransformer4Search.IsCaseSensitive = m_IsCaseSensitive;
        }

        private void InitializeHighlighting()
        {
            var transformer = Editor.TextArea.TextView.LineTransformers.FirstOrDefault(x => x is ColorizeAvalonEdit);
            if (transformer == null)
            {
                DocumentColorizingTransformer4Search = new ColorizeAvalonEdit();
                Editor.TextArea.TextView.LineTransformers.Add(DocumentColorizingTransformer4Search);
            }
            else
            {
                DocumentColorizingTransformer4Search = transformer as ColorizeAvalonEdit;
            }
        }

        private bool FindNext(string textToFind)
        {
            Regex regex = GetRegEx(textToFind);
            int start = regex.Options.HasFlag(RegexOptions.RightToLeft) ?
            Editor.SelectionStart : Editor.SelectionStart + Editor.SelectionLength;
            Match match = regex.Match(Editor.Text, start);

            if (!match.Success)  // start again from beginning or end
            {
                MessageBoxResult result = MessageBox.Show(
                                               Application.Current.MainWindow,
                                               Properties.Resources.DlgGrepMsg_NoMoreTextFound,
                                               Properties.Resources.DlgFind_FindNext,
                                               MessageBoxButton.YesNo,
                                               MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (regex.Options.HasFlag(RegexOptions.RightToLeft))
                        match = regex.Match(Editor.Text, Editor.Text.Length);
                    else
                        match = regex.Match(Editor.Text, 0);
                }
            }

            if (match.Success)
            {
                Editor.Select(match.Index, match.Length);
                TextLocation loc = Editor.Document.GetLocation(match.Index);
                Editor.ScrollTo(loc.Line, loc.Column);
            }

            return match.Success;
        }

        private Regex GetRegEx(string textToFind, bool leftToRight = false)
        {
            RegexOptions options = RegexOptions.None;
            if (IsSearchUp && !leftToRight)
                options |= RegexOptions.RightToLeft;
            if (!IsCaseSensitive)
                options |= RegexOptions.IgnoreCase;

            if (IsUseRegex)
            {
                return new Regex(textToFind, options);
            }
            else
            {
                string pattern = Regex.Escape(textToFind);
                if (IsUseWildcards)
                    pattern = pattern.Replace("\\*", ".*").Replace("\\?", ".");
                if (IsMatchWholeWord)
                    pattern = "\\b" + pattern + "\\b";
                return new Regex(pattern, options);
            }
        }
    }
}
