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

namespace Gherkin.ViewModel
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

        public GherkinCodeCompletion(TextEditor textEditor, IAppSettings appSettings)
        {
            TextEditor = textEditor;
            m_AppSettings = appSettings;

            TextEditor.TextArea.TextEntering += OnTextAreaTextEntering;
            TextEditor.TextArea.TextEntered += OnTextAreaTextEntered;
        }

        private void OnTextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            var caret = TextEditor.TextArea.Caret;
            EnteredText enteredText = new EnteredText(e.Text, caret.Line, caret.Column);
            GherkinCodeCompletionWordsProvider completionWordsProvider = new GherkinCodeCompletionWordsProvider(Document, enteredText, m_AppSettings);
            List<GherkinCodeCompletionWord> completionWords = completionWordsProvider.CompletionWords;
            if (completionWords.Count > 0)
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
                if (!char.IsLetterOrDigit(e.Text[0]))
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
