using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using System.Windows;
using System.ComponentModel;
using Gherkin.Util;
using static Gherkin.Util.Util;
using System.Windows.Input;

namespace Gherkin.ViewModel
{
    public class EditorTab : TabItem, INotifyPropertyChanged
    {
        private TextBlock TabTextBlock { get; set; }

        public EditorView EditorView { get; set; }
        public TextEditor MainEditor => EditorView.MainEditor;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SaveCmd => new DelegateCommandNoArg(OnSave);

        public EditorTab()
        {
            Header = Properties.Resources.Message_UnknownFileName;
            ToolTip = Properties.Resources.Message_UnknownFileName;

            base.Loaded += OnEditorTabLoaded;
        }

        private void OnSave()
        {
            if (MainEditor.IsModified)
            {
                EditorView.SaveFile(saveAs: false);
            }
        }

        private void OnTextEditorLoadedEvent()
        {
            MainEditor.TextChanged += OnTextEditorTextChanged;
        }

        public string DocumentModificationStatusIcon
        {
            get
            {
                string image_name = (MainEditor?.IsModified == true) ? "Modified.png" : "Unchanged.png";
                return PackImageURI(image_name);
            }
        }

        private void OnTextEditorTextChanged(object sender, EventArgs e)
        {
            NotifyModificationStatus();
        }

        private void OnDocumentSavedEvent()
        {
            NotifyModificationStatus();
        }

        /// <summary>
        /// Notify updating DocumentModificationStatusIcon.
        /// Note: Do this work in background because we may have received TextChanged
        /// before TextEditor's IsModified property updated
        /// property stay unchanged.
        /// </summary>
        private void NotifyModificationStatus()
        {
            Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DocumentModificationStatusIcon)));
                }));
            });
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

            EditorView.TextEditorLoadedEvent += OnTextEditorLoadedEvent;
            EditorView.FileNameChangedEvent += OnFileNameChangedEvent;
            EditorView.DocumentSavedEvent += OnDocumentSavedEvent;
        }

        public void SetMaxWidth(double width)
        {
            // TabTextBlock may be null when EditorTab has not been loaded yet
            TabTextBlock.IfNotNull( x => x.MaxWidth = Math.Max(width - 40, 1));
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
