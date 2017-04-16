using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ICSharpCode.AvalonEdit.Rendering
{
    /// <summary>
    /// Searched text highlighting transformer
    /// Note: added by bzquan@gmail.com
    /// </summary>
    public class SearchHighlightingTransformer : DocumentColorizingTransformer
    {
        /// <summary>
        /// Searched Text
        /// </summary>
        public Regex SearchRegex { get; set; }

        /// <summary>
        /// Find colorize text by considering case sensitive.
        /// </summary>
        /// <param name="line"></param>
        protected override void ColorizeLine(DocumentLine line)
        {
            if (SearchRegex == null) return;

            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);
            int line_len = text.Length;
            int start = 0;
            Match match = SearchRegex.Match(text, start);
            while (match.Success && (start < line_len))
            {
                base.ChangeLinePart(
                    lineStartOffset + match.Index,                // startOffset
                    lineStartOffset + match.Index + match.Length, // endOffset
                    (VisualLineElement element) =>
                    {
                        ColorizeElement(element);
                    });
                start = match.Index + match.Length; // search for next occurrence
                if (start < line_len)
                {
                    match = SearchRegex.Match(text, start);
                }
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
