using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;

namespace ICSharpCode.AvalonEdit.Rendering
{
    /// <summary>
    /// VisualLineElement that represents a piece of text and is a clickable link.
    /// </summary>
    public class VisualLineFilePathText : VisualLineText
    {
        /// <summary>
        /// File path which can be opened in editor
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
		/// Gets/Sets whether the user needs to press Control to click the link.
		/// The default value is false.
        /// </summary>
        public bool RequireControlModifierForClick { get; set; } = false;

        /// <summary>
        /// The action will be called with FilePath as parameter if file is clicked.
        /// </summary>
        public Action<string> FilePathClickedHandler { get; set; }

        /// <summary>
        /// Creates a visual line text element with the specified length.
        /// It uses the <see cref="ITextRunConstructionContext.VisualLine"/> and its
        /// <see cref="VisualLineElement.RelativeTextOffset"/> to find the actual text string.
        /// </summary>
        public VisualLineFilePathText(VisualLine parentVisualLine, int length) : base(parentVisualLine, length)
        {
        }

        /// <inheritdoc/>
        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            this.TextRunProperties.SetForegroundBrush(context.TextView.LinkTextForegroundBrush); //DarkBlue
            this.TextRunProperties.SetBackgroundBrush(context.TextView.LinkTextBackgroundBrush);
            if (context.TextView.LinkTextUnderline)
                this.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
            return base.CreateTextRun(startVisualColumn, context);
        }

        /// <summary>
        /// Gets whether the link is currently clickable.
        /// </summary>
        protected virtual bool LinkIsClickable()
        {
            if (FilePath == null) return false;

            if (RequireControlModifierForClick)
                return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            else
                return true;
        }

        /// <inheritdoc/>
        protected internal override void OnQueryCursor(QueryCursorEventArgs e)
        {
            if (LinkIsClickable())
            {
                e.Handled = true;
                e.Cursor = Cursors.Hand;
            }
        }

        /// <inheritdoc/>
        protected internal override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !e.Handled && LinkIsClickable())
            {
                FilePathClickedHandler?.Invoke(FilePath);

                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        protected override VisualLineText CreateInstance(int length)
        {
            return new VisualLineFilePathText(ParentVisualLine, length)
            {
                FilePath = this.FilePath,
                FilePathClickedHandler = this.FilePathClickedHandler,
                RequireControlModifierForClick = this.RequireControlModifierForClick
            };
        }
    }
}
