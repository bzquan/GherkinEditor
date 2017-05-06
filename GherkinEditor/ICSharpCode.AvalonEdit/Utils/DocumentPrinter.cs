// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

using ICSharpCode.NRefactory.Editor;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace ICSharpCode.AvalonEdit.Utils
{
	/// <summary>
	/// Helps printing documents.
	/// </summary>
	public static class DocumentPrinter
	{
		#if NREFACTORY
		/// <summary>
		/// Converts a readonly TextDocument to a Block and applies the provided highlighting definition.
		/// </summary>
		public static Block ConvertTextDocumentToBlock(ReadOnlyDocument document, IHighlightingDefinition highlightingDefinition)
		{
			IHighlighter highlighter;
			if (highlightingDefinition != null)
				highlighter = new DocumentHighlighter(document, highlightingDefinition);
			else
				highlighter = null;
			return ConvertTextDocumentToBlock(document, highlighter);
		}
		#endif
		
		/// <summary>
		/// Converts an IDocument to a Block and applies the provided highlighter.
		/// </summary>
		public static Block ConvertTextDocumentToBlock(IDocument document, IHighlighter highlighter, List<VisualLineElementGenerator> visualLineElementGenerators)
		{
			if (document == null)
				throw new ArgumentNullException("document");
			Paragraph p = new Paragraph();
			p.TextAlignment = TextAlignment.Left;
			for (int lineNumber = 1; lineNumber <= document.LineCount; lineNumber++)
            {
                if (lineNumber > 1)
                    p.Inlines.Add(new LineBreak());
                var line = document.GetLineByNumber(lineNumber);
                if (!AddVisualLineElements(visualLineElementGenerators, p, line))
                {
                    if (highlighter != null)
                    {
                        HighlightedLine highlightedLine = highlighter.HighlightLine(lineNumber);
                        p.Inlines.AddRange(highlightedLine.ToRichText().CreateRuns());
                    }
                    else
                    {
                        p.Inlines.Add(document.GetText(line));
                    }
                }
            }
            return p;
		}

        /// <summary>
        /// Add Run elements, if available, to paragrapsh
        /// Added by: bzquan@gmail.com
        /// </summary>
        /// <param name="visualLineElementGenerators"></param>
        /// <param name="p"></param>
        /// <param name="line"></param>
        /// <returns>true if Run elements added</returns>
        private static bool AddVisualLineElements(List<VisualLineElementGenerator> visualLineElementGenerators, Paragraph p, IDocumentLine line)
        {
            InlineObjectElementWithText inlineObject = GetVisualLineElement(visualLineElementGenerators, line);
            if (inlineObject == null) return false;

            // Leading text
            if (inlineObject.LeadingText != null)
            {
                p.Inlines.Add(inlineObject.LeadingText);
            }
            // UIElement
            p.Inlines.Add(inlineObject.Element);
            // Trailing text
            if (inlineObject.TrailingText != null)
            {
                p.Inlines.Add(inlineObject.TrailingText);
            }

            return true;
        }

        /// <summary>
        /// Get a InlineObjectElementWithText from generators if availabe.
        /// Added by: bzquan@gmail.com
        /// </summary>
        /// <param name="visualLineElementGenerators">generators</param>
        /// <param name="line"></param>
        /// <returns>an in line object if available or null </returns>
        private static InlineObjectElementWithText GetVisualLineElement(List<VisualLineElementGenerator> visualLineElementGenerators, IDocumentLine line)
        {
            foreach (var generator in visualLineElementGenerators)
            {
                int offset = generator.GetFirstInterestedOffset(line.Offset);
                if (offset >= 0)
                {
                    var elem = generator.ConstructElement(offset) as InlineObjectElementWithText;
                    if (elem != null)
                    {
                        return elem;
                    }
                }
            }

            return null;
        }

#if NREFACTORY
		/// <summary>
		/// Converts a readonly TextDocument to a RichText and applies the provided highlighting definition.
		/// </summary>
		public static RichText ConvertTextDocumentToRichText(ReadOnlyDocument document, IHighlightingDefinition highlightingDefinition)
		{
			IHighlighter highlighter;
			if (highlightingDefinition != null)
				highlighter = new DocumentHighlighter(document, highlightingDefinition);
			else
				highlighter = null;
			return ConvertTextDocumentToRichText(document, highlighter);
		}
#endif

        /// <summary>
        /// Converts an IDocument to a RichText and applies the provided highlighter.
        /// </summary>
        public static RichText ConvertTextDocumentToRichText(IDocument document, IHighlighter highlighter)
		{
			if (document == null)
				throw new ArgumentNullException("document");
			var texts = new List<RichText>();
			for (int lineNumber = 1; lineNumber <= document.LineCount; lineNumber++) {
				var line = document.GetLineByNumber(lineNumber);
				if (lineNumber > 1)
					texts.Add(line.PreviousLine.DelimiterLength == 2 ? "\r\n" : "\n");
				if (highlighter != null) {
					HighlightedLine highlightedLine = highlighter.HighlightLine(lineNumber);
					texts.Add(highlightedLine.ToRichText());
				} else {
					texts.Add(document.GetText(line));
				}
			}
			return RichText.Concat(texts.ToArray());
		}
		
		/// <summary>
		/// Creates a flow document from the editor's contents.
		/// </summary>
		public static FlowDocument CreateFlowDocumentForEditor(TextEditor editor, List<VisualLineElementGenerator> visualLineElementGenerators)
		{
            IHighlighter highlighter = editor.TextArea.GetService(typeof(IHighlighter)) as IHighlighter;
			FlowDocument doc = new FlowDocument(ConvertTextDocumentToBlock(editor.Document, highlighter, visualLineElementGenerators));
			doc.FontFamily = editor.FontFamily;
			doc.FontSize = editor.FontSize;
			return doc;
		}
	}
}
