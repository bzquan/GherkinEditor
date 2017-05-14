using System;
using System.Windows;
using ICSharpCode.AvalonEdit;
using Gherkin.Util;
using System.Windows.Input;
using Gherkin.ViewModel;

namespace Gherkin.View
{
    public class EditorTabContent : System.Windows.Controls.ContentControl
    {
        private TextEditor MainEditor { get; set; }
        private bool IsEditorViewLoaded => MainEditor != null;

        private EditorTabContentViewModel EditorTabContentViewModel { get; set; }

        public EditorTabContent(System.Windows.FrameworkElement parent, EditorTabContentViewModel viewModel)
        {
            EditorTabContentViewModel = viewModel;

            this.ContentTemplate = parent.FindResource("editorDataTemplate") as DataTemplate;
            this.DataContext = viewModel;

            base.Loaded += OnEditorViewLoaded;
        }

        private void OnEditorViewLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsEditorViewLoaded)
            {
                try
                {
                    MainEditor = VisualChildrenFinder.FindControl<EditorTabContent, TextEditor>(this, "mainEditor");
                    TextEditor subEditor = VisualChildrenFinder.FindControl<EditorTabContent, TextEditor>(this, "subEditor");
                    TextEditor viewerEditor = VisualChildrenFinder.FindControl<EditorTabContent, TextEditor>(this, "viewerEditor");
                    EditorTabContentViewModel.InitializeEditorView(MainEditor, subEditor, viewerEditor);
                }
                catch (Exception ex)
                {
                    EventAggregator<FailedToFindEditorControlArg>.Instance.Publish(this, new FailedToFindEditorControlArg(EditorTabContentViewModel));
                    EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg(ex.Message));
                }
            }

            EventAggregator<EditorLoadedArg>.Instance.Publish(this, new EditorLoadedArg(EditorTabContentViewModel));

            // Important Note: the main editor is focused whenever it is loaded.
            // Reason: FindControl of InitializeEditorView will fail
            // when creating loading next new editor if MainEditor is not focused.
            // 
            // Root cause: Microsoft bug?
            Keyboard.Focus(MainEditor);
        }
    }
}
