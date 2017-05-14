using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Gherkin.Util;
using WpfMath;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Globalization;
using System.Diagnostics;
using ICSharpCode.AvalonEdit;

namespace Gherkin.Model
{
    /// <summary>
    /// Custom Element Generators for AvalonEdit
    /// Remark:
    /// http://danielgrunwald.de/coding/AvalonEdit/rendering.php
    /// </summary>
    public abstract class CustomElementGenerator : VisualLineElementGenerator
    {
        protected TextEditor TextEditor { get; set; }
        protected TextDocument Document => TextEditor.Document;

        public CustomElementGenerator(TextEditor textEditor)
        {
            TextEditor = textEditor;
        }

        /// Gets the first offset >= startOffset where the generator wants to construct
        /// an element.
        /// Return -1 to signal no interest.
        public override int GetFirstInterestedOffset(int startOffset)
        {
            if (CanApplyGenerator())
            {
                Match m = FindMatch(startOffset);
                return m.Success ? (startOffset + m.Index) : -1;
            }
            else
                return -1;
        }

        protected virtual bool CanApplyGenerator()
        {
            return true;
        }

        protected abstract Regex GetRegex();

        protected Match FindMatch(int startOffset)
        {
            string relevantText = GetRelevantText(startOffset);
            return GetRegex().Match(relevantText);
        }

        protected bool IsPrinting => (CurrentContext == null);

        /// <summary>
        /// fetch the end offset of the VisualLine being generated
        /// </summary>
        /// <param name="startOffset"></param>
        /// <returns></returns>
        private string GetRelevantText(int startOffset)
        {
            if (CurrentContext != null)
            {
                // This is original implementation in
                // http://danielgrunwald.de/coding/AvalonEdit/rendering.php
                // It works perfectly.
                int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
                return Document.GetText(startOffset, endOffset - startOffset);
            }
            else
            {
                // This is customized implementation for printing preview & printing
                // in which CurrentContext is not available(would be null).
                // Here we directly use TextDocument to fetch "relevant text".
                DocumentLine line = Document.GetLineByOffset(startOffset);
                int endOffset = startOffset - (startOffset - line.Offset) + line.TotalLength;
                string relevantText = Document.GetText(startOffset, endOffset - startOffset);
                return relevantText;
            }
        }

        /// Constructs an element at the specified offset.
        /// May return null if no element can be constructed.
        public override VisualLineElement ConstructElement(int offset)
        {
            InlineObjectElement mainUIElement = ConstructMainElement(offset);

            if ((mainUIElement == null) || !IsPrinting)
                return mainUIElement;
            else
                return ConstructElements4Printing(offset, mainUIElement);
        }

        protected abstract InlineObjectElement ConstructMainElement(int offset);

        private InlineObjectElementWithText ConstructElements4Printing(int offset, InlineObjectElement mainUIElement)
        {
            Debug.Assert(mainUIElement != null);

            InlineObjectElementWithText inlineObject = new InlineObjectElementWithText(mainUIElement.DocumentLength, mainUIElement.Element);

            var line = Document.GetLineByOffset(offset);
            inlineObject.LeadingText = GetLeadingText(Document, line, offset);
            inlineObject.TrailingText = GetTrailingText(Document, line, offset, mainUIElement.DocumentLength);

            return inlineObject; 
        }

        private string GetLeadingText(IDocument document, IDocumentLine line, int visualLineElementOffset)
        {
            int leadingTextLen = visualLineElementOffset - line.Offset;
            if (leadingTextLen > 0)
            {
                string leadingText = document.GetText(line.Offset, leadingTextLen);
                return leadingText;
            }

            return null;
        }

        private string GetTrailingText(IDocument document, IDocumentLine line, int visualLineElementOffset, int documentLength)
        {
            int leadingTextLen = visualLineElementOffset - line.Offset;
            int trailingTextLen = line.Length - leadingTextLen - documentLength;
            if (trailingTextLen > 0)
            {
                string trailingText = document.GetText(visualLineElementOffset + documentLength, trailingTextLen);
                return trailingText;
            }

            return null;
        }
    }
}
