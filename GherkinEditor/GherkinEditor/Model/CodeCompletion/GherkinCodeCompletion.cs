using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Gherkin.Util;

namespace Gherkin.Model
{
    /// <summary>
    /// Implementation of Gherkin code completion
    /// </summary>
    public class GherkinCodeCompletion
    {
        private IAppSettings m_AppSettings;

        private TextEditor TextEditor { get; set; }
        private TextDocument Document => TextEditor.Document;
        private CompletionWindow m_CompletionWindow;

        private ICommand ShowCodeCompletionCmd => new DelegateCommandNoArg(OnShowCodeCompletion, CanShowCodeCompletion);

        public GherkinCodeCompletion(TextEditor textEditor, IAppSettings appSettings)
        {
            TextEditor = textEditor;
            m_AppSettings = appSettings;

            TextEditor.TextArea.Options.ShowCodeCompletionCmd = ShowCodeCompletionCmd;
            TextEditor.TextArea.TextEntering += OnTextAreaTextEntering;
            TextEditor.TextArea.TextEntered += OnTextAreaTextEntered;
        }

        private void OnShowCodeCompletion()
        {
            DoCodeCompletion(showCompletionWords: true);
        }

        private bool CanShowCodeCompletion()
        {
            if (!GherkinUtil.IsFeatureFile(Document.FileName)) return false;

            var caret = TextEditor.TextArea.Caret;
            var line = Document.GetLineByNumber(caret.Line);
            string text = GherkinFormatUtil.GetText(Document, line);

            return string.IsNullOrWhiteSpace(text);
        }

        private void OnTextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            DoCodeCompletion(showCompletionWords: false);
        }

        private void DoCodeCompletion(bool showCompletionWords)
        {
            if (!CanShowCodeCompletion()) return;

            var caret = TextEditor.TextArea.Caret;
            TextEnteredPosition enteredText = new TextEnteredPosition(caret.Line, caret.Column);
            GherkinCodeCompletionWordsProvider completionWordsProvider = new GherkinCodeCompletionWordsProvider(Document, enteredText, m_AppSettings);
            bool isEditingDescription;
            List<GherkinCodeCompletionWord> completionWords = completionWordsProvider.CompletionWords(out isEditingDescription);
            if ((completionWords.Count > 0) && (!isEditingDescription || showCompletionWords))
            {
                // open code completion after the user has pressed dot:
                m_CompletionWindow = new CompletionWindow(TextEditor.TextArea);
                // provide AvalonEdit with the data:
                IList<ICompletionData> data = m_CompletionWindow.CompletionList.CompletionData;
                foreach (var word in completionWords)
                {
                    data.Add(word);
                }

                m_CompletionWindow.Show();
                m_CompletionWindow.Closed += delegate { m_CompletionWindow = null; };
            }
        }

        private void OnTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && m_CompletionWindow != null)
            {
                if (!char.IsLetter(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    m_CompletionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // do not set e.Handled=true - we still want to insert the character that was typed
        }
    }
}
