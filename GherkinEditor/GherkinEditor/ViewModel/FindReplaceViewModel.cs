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
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media;

namespace Gherkin.ViewModel
{
    public class FindReplaceViewModel : AbstractFindViewModel
    {
        private string m_ReplaceText = "";

        private GherkinEditor GherkinEditor { get; set; }
        private TextEditor Editor => GherkinEditor.TextEditor;
        private SearchHighlightingTransformer SearchHighlightingTransformer => GherkinEditor.GetSearchHighlightingTransformer();

        public FindReplaceViewModel(GherkinEditor editor, IAppSettings appSettings) :
            base(appSettings)
        {
            GherkinEditor = editor;
            m_SearchCondition.IsCaseSensitive = m_AppSettings.LastStatus.IsCaseSensitiveInFind;
            m_SearchCondition.IsMatchWholeWord = m_AppSettings.LastStatus.IsMatchWholeWordInFind;
            m_SearchCondition.IsUseRegex = m_AppSettings.LastStatus.IsUseRegexInFind;
            m_SearchCondition.IsUseWildcards = m_AppSettings.LastStatus.IsUseWildcardsInFind;
        }

        public ICommand FindNextCmd => new DelegateCommandNoArg(OnFindNext, CanExecFindReplaceCmd);
        public ICommand FindNextUpCmd => new DelegateCommandNoArg(OnFindNextUp, CanExecFindReplaceCmd);
        public ICommand FindNextCmd2 => new DelegateCommand<bool>(OnFindNext, CanExecFindReplaceCmd);
        public ICommand ReplaceCmd => new DelegateCommandNoArg(OnReplace, CanExecFindReplaceCmd);
        public ICommand ReplaceAllCmd => new DelegateCommandNoArg(OnReplaceAll, CanExecFindReplaceCmd);

        public RegexDocument RegexDocument { get; private set; } = new RegexDocument();

        public void SetTextToFind()
        {
            if (!Editor.TextArea.Selection.IsMultiline)
            {
                TextToFind = Editor.TextArea.Selection.GetText();
            }
            if (string.IsNullOrEmpty(TextToFind))
            {
                TextToFind = RecentTextsToFind.Count > 0 ? RecentTextsToFind[0] : "";
            }
        }

        public string ReplaceText
        {
            get { return m_ReplaceText ?? ""; }
            set
            {
                m_ReplaceText = value ?? "";
                base.OnPropertyChanged();
            }
        }
        public List<string> RecentTextsToFind
        {
            get { return m_AppSettings.LastStatus.LastSearchedTexts; }
        }

        public bool IsSearchUp
        {
            get { return m_SearchCondition.IsSearchUp; }
            set
            {
                m_SearchCondition.IsSearchUp = value;
                base.OnPropertyChanged(nameof(ArrowDownIcon));
                base.OnPropertyChanged(nameof(ArrowUpIcon));
                base.OnPropertyChanged();
            }
        }

        public DrawingImage ArrowDownIcon => ArrowIcon("ArrowDown.png", isSelected: !IsSearchUp);
        public DrawingImage ArrowUpIcon => ArrowIcon("ArrowUp.png", isSelected: IsSearchUp);

        private DrawingImage ArrowIcon(string arrow, bool isSelected) =>
                                Util.Util.DrawingCircleOnImage(arrow, isSelected);

        public void FindNextByDefault()
        {
            OnFindNext(m_SearchCondition.IsSearchUp);
        }

        private void OnFindNext()
        {
            OnFindNext(isSeachUp: false);
        }

        private void OnFindNextUp()
        {
            OnFindNext(isSeachUp: true);
        }

        public void OnFindNext(bool isSeachUp)
        {
            IsSearchUp = isSeachUp;
            SetTransformerAndBackup(TextToFind);
            FindNext();
        }

        private bool CanExecFindReplaceCmd() => m_SearchCondition.IsValidTextToFind();
        private bool CanExecFindReplaceCmd(bool isSeachUp) => CanExecFindReplaceCmd();

        private void OnReplace()
        {
            SetTransformerAndBackup(TextToFind);
            Regex regex = GetRegEx(isForHighlighting:false);
            string input = Editor.Text.Substring(Editor.SelectionStart, Editor.SelectionLength);
            Match match = regex.Match(input);
            if (match.Success && match.Index == 0 && match.Length == input.Length)
            {
                Editor.Document.Replace(Editor.SelectionStart, Editor.SelectionLength, ReplaceText);
            }

            FindNext();
        }

        private void OnReplaceAll()
        {
            string msg = string.Format(Properties.Resources.Message_ReplaceAllConfirm, TextToFind, ReplaceText);
            if (MessageBox.Show(Application.Current.MainWindow,
                           msg,
                           Properties.Resources.Message_ReplaceAllTitle,
                           MessageBoxButton.OKCancel,
                           MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                SetTransformerAndBackup(ReplaceText, true);
                Regex regex = GetRegEx(isForHighlighting: false, leftToRight: true);
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

        private void SetTransformerAndBackup(string text, bool isReplacedText = false)
        {
            BackupLastUsedText();
            Editor.TextArea.TextView.Redraw();
            if (isReplacedText)
                SearchHighlightingTransformer.SearchRegex = m_SearchCondition.GetSimpleRegEx(text);
            else
                SearchHighlightingTransformer.SearchRegex = GetRegEx(isForHighlighting: true);
        }

        private void BackupLastUsedText()
        {
            m_AppSettings.LastStatus.LastSearchedText = TextToFind;
        }

        private bool FindNext()
        {
            Regex regex = GetRegEx(isForHighlighting: false);
            int start = regex.Options.HasFlag(RegexOptions.RightToLeft) ?
                                Editor.SelectionStart : Editor.SelectionStart + Editor.SelectionLength;
            Match match = regex.Match(Editor.Text, start);

            if (!match.Success)  // start again from beginning or end
            {
                MessageBoxResult result = MessageBox.Show(
                                               Application.Current.MainWindow,
                                               Properties.Resources.DlgFind_NoMoreTextFound,
                                               Properties.Resources.DlgFind_Find,
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

        private Regex GetRegEx(bool isForHighlighting, bool leftToRight = false)
        {
            return m_SearchCondition.GetRegEx(isForHighlighting, leftToRight);
        }
    }
}
