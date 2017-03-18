using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Gherkin.ViewModel;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System.Windows;
using Gherkin.Util;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Gherkin.ViewModel
{
    public class EditorTab : TabItem
    {
        private TextBlock TabTextBlock { get; set; }

        public EditorView EditorView { get; set; }
        public TextEditor MainEditor => EditorView.MainEditor;

        public EditorTab()
        {
            Header = Properties.Resources.Message_UnknownFileName;
            ToolTip = Properties.Resources.Message_UnknownFileName;

            base.Loaded += OnEditorTabLoaded;
        }

        private void OnEditorTabLoaded(object sender, RoutedEventArgs e)
        {
            TabTextBlock = VisualChildrenFinder.FindControl<EditorTab, TextBlock>(this, "tabTextBlock");
            EventAggregator<AdjustMaxWidthOfEditorTabArg>.Instance.Publish(this, new AdjustMaxWidthOfEditorTabArg());
        }

        public DelegateCommandNoArg DeleteTabCmd => new DelegateCommandNoArg(OnDelete);

        public void SetContent(EditorView editorView)
        {
            EditorView = editorView;
            base.Content = editorView;

            editorView.FileNameChangedEvent += OnFileNameChangedEvent;
        }

        public void SetMaxWidth(double width)
        {
            // TabTextBlock may be null when EditorTab has not been loaded yet
            if (TabTextBlock != null)
            {
                TabTextBlock.MaxWidth = Math.Max(width - 40, 1);
            }
        }

        private void OnFileNameChangedEvent(string filePath)
        {
            if (filePath != null)
            {
                Header = System.IO.Path.GetFileName(filePath);
                ToolTip = filePath;
            }
            else
            {
                Header = Properties.Resources.Message_UnknownFileName;
                ToolTip = Properties.Resources.Message_UnknownFileName;
            }
        }

        private void OnDelete()
        {
            DeleteEditorTabRequested arg = new DeleteEditorTabRequested(this);
            Util.EventAggregator<DeleteEditorTabRequested>.Instance.Publish(this, arg);
        }
    }
}
