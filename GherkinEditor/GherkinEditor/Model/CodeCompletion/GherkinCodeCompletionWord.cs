using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Gherkin.Model;
using Gherkin.Util;

namespace Gherkin.Model
{
    /// <summary>
    /// Implements AvalonEdit ICompletionData interface to provide the entries in the completion drop down.
    /// </summary>
    public class GherkinCodeCompletionWord : ICompletionData
    {
        private GherkinKeyword Keyword { get; set; }
        private TextBlock ContentTextBlock { get; set; }
        private IAppSettings AppSettings { get; set; }

        public GherkinCodeCompletionWord(GherkinKeyword keyword, IAppSettings appSettings)
        {
            this.Keyword = keyword;
            this.Text = keyword.Text.Trim();
            this.AppSettings = appSettings;
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the text. This property is used to filter the list of visible elements.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// The displayed content. This can be the same as 'Text', or a WPF UIElement if
        /// you want to display rich content.
        /// </summary>
        public object Content
        {
            get
            {
                if (ContentTextBlock == null)
                {
                    ContentTextBlock = MakeContentTextBlock();
                }
                return ContentTextBlock;
            }
        }

        private TextBlock MakeContentTextBlock()
        {
            TextBlock contentTextBlock = new TextBlock();
            contentTextBlock.Text = this.Text;

            var colorName = "Black";
            if (Keyword.IsLanguage)
                colorName = "Green";
            else if (Keyword.IsKeyword)
                colorName = AppSettings.ColorOfHighlightingKeyword;
            else if (Keyword.IsStepKeyword())
                colorName = AppSettings.ColorOfHighlightingStepWord;

            contentTextBlock.Foreground = new SolidColorBrush(colorName.ToColor());
            contentTextBlock.Background = Brushes.White;

            return contentTextBlock;
        }

        public object Description
        {
            get
            {
                if (Keyword.IsLanguage)
                    return Properties.Resources.Message_CodeCompletionLanguage;

                if (Keyword.IsFeature)
                    return Properties.Resources.Message_CodeCompletionFeature;

                if (Keyword.IsBackground)
                    return Properties.Resources.Message_CodeCompletionBackground;

                if (Keyword.IsScenario)
                    return Properties.Resources.Message_CodeCompletionScenario;

                if (Keyword.IsScenarioOutline)
                    return Properties.Resources.Message_CodeCompletionScenarioOutline;

                if (Keyword.IsExample)
                    return Properties.Resources.Message_CodeCompletionExample;

                return Properties.Resources.Message_CodeCompletionStepKeyword;
            }
        }

        /// <summary>
        /// Gets the priority. This property is used in the selection logic. You can use it to prefer selecting those items
        /// which the user is accessing most frequently.
        /// </summary>
        public double Priority
        {
            get { return 0; }
        }

        /// <summary>
        /// Perform the completion.
        /// </summary>
        /// <param name="textArea">The text area on which completion is performed.</param>
        /// <param name="completionSegment">The text segment that was used by the completion window if
        /// the user types (segment between CompletionWindow.StartOffset and CompletionWindow.EndOffset).</param>
        /// <param name="insertionRequestEventArgs">The EventArgs used for the insertion request.
        /// These can be TextCompositionEventArgs, KeyEventArgs, MouseEventArgs, depending on how
        /// the insertion was triggered.</param>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            string formatted_text = this.Text;
            if (Keyword.IsKeyword)
            {
                formatted_text = this.Text + ": ";
            }
            else if (Keyword.IsStepKeyword())
            {
                string leading_spaces = HasNoLeandingText(textArea) ? GherkinSimpleParser.IDENT2 : "";
                formatted_text = leading_spaces + this.Text + " ";
            }
            else if (Keyword.IsExample)
            {
                string leading_spaces = HasNoLeandingText(textArea) ? GherkinSimpleParser.IDENT2 : "";
                formatted_text = leading_spaces + this.Text + ": ";
            }

            textArea.Document.Replace(completionSegment, formatted_text);
        }

        private bool HasNoLeandingText(TextArea textArea) => textArea.Caret.Column == 1;
    }
}
