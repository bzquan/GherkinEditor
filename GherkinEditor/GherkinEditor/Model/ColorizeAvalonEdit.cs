using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gherkin.Model
{
    public class ColorizeAvalonEdit : DocumentColorizingTransformer
    {
        private StringComparison CompareType { get; set; } = StringComparison.CurrentCultureIgnoreCase;
        public string ColorizingWord { get; set; }
        public bool IsCaseSensitive
        {
            set
            {
                if (value)
                    CompareType = StringComparison.CurrentCulture;
                else
                    CompareType = StringComparison.CurrentCultureIgnoreCase;
            }
        }

        /// <summary>
        /// Find colorize text by considering case sensitive.
        /// Limitation: Currently whole word searching is not supported.
        /// </summary>
        /// <param name="line"></param>
        protected override void ColorizeLine(DocumentLine line)
        {
            if (string.IsNullOrEmpty(ColorizingWord)) return;

            int lenthOfWord = ColorizingWord.Length;
            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);
            int start = 0;
            int index;
            while ((index = text.IndexOf(ColorizingWord, start, CompareType)) >= 0)
            {
                base.ChangeLinePart(
                    lineStartOffset + index, // startOffset
                    lineStartOffset + index + lenthOfWord, // endOffset
                    (VisualLineElement element) =>
                    {
                        ColorizeElement(element);
                    });
                start = index + lenthOfWord; // search for next occurrence
            }
        }

        /// <summary>
        /// This method gets called once for every VisualLineElement between the specified offsets.
        /// </summary>
        /// <param name="element"></param>
        private void ColorizeElement(VisualLineElement element)
        {
            Typeface tf = element.TextRunProperties.Typeface;
            // Replace the typeface with a modified version of the same typeface
            element.TextRunProperties.SetTypeface(new Typeface(
                        tf.FontFamily,
                        FontStyles.Italic,
                        FontWeights.Bold,
                        tf.Stretch
                    ));
            element.TextRunProperties.SetForegroundBrush(Brushes.Blue);
            element.TextRunProperties.SetBackgroundBrush(Brushes.Yellow);
        }
    }
}
