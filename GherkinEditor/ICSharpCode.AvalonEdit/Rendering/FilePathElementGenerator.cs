using System;
using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Rendering
{
    /// <summary>
    /// Detects file path and makes them clickable.
    /// </summary>
    public class FilePathElementGenerator : VisualLineElementGenerator, IBuiltinElementGenerator
    {
        private TextEditorOptions.ContainOpenableFilePathHandler ContainOpenableFilePath { get; set; }
        private Action<string> FilePathClickedHandler { get; set; }

        void IBuiltinElementGenerator.FetchOptions(TextEditorOptions options)
        {
            ContainOpenableFilePath = options.ContainOpenableFilePath;
            FilePathClickedHandler = options.FilePathClickedHandler;
            this.RequireControlModifierForClick = options.RequireControlModifierForHyperlinkClick;
        }

        /// <summary>
        /// Gets/Sets whether the user needs to press Control to click the link.
        /// The default value is false.
        /// </summary>
        public bool RequireControlModifierForClick { get; set; }
        /// <inheritdoc/>
        public override int GetFirstInterestedOffset(int startOffset)
        {
            int matched_length;
            int matchOffset = GetMatchedOffset(startOffset, out matched_length);

            return matchOffset;
        }

        private int GetMatchedOffset(int startOffset, out int matched_length)
        {
            matched_length = -1;
            if (ContainOpenableFilePath == null) return -1;

            string text = GetText(startOffset);
            int matched_startOffset = -1;
            bool matched = ContainOpenableFilePath(text, out matched_startOffset, out matched_length);
            int matchOffset = matched ? startOffset + matched_startOffset : -1;

            return matchOffset;
        }

        private string GetText(int startOffset)
        {
            int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
            StringSegment relevantText = CurrentContext.GetText(startOffset, endOffset - startOffset);
            string text = relevantText.Text.Substring(relevantText.Offset, relevantText.Count);

            return text;
        }

        /// <inheritdoc/>
        public override VisualLineElement ConstructElement(int offset)
        {
            int matched_length;
            int matchOffset = GetMatchedOffset(offset, out matched_length);
            if (matchOffset == offset)
            {
                VisualLineFilePathText filePathText = new VisualLineFilePathText(CurrentContext.VisualLine, matched_length);
                filePathText.FilePath = GetText(matchOffset);
                filePathText.FilePathClickedHandler = this.FilePathClickedHandler;
                filePathText.RequireControlModifierForClick = this.RequireControlModifierForClick;

                return filePathText;
            }
            else
            {
                return null;
            }
        }
    }
}
