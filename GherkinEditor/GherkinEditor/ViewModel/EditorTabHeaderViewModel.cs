using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using System.Windows.Input;
using System.Windows;
using static Gherkin.Util.Util;
using System.Collections.ObjectModel;
using Gherkin.View;
using System.IO;
using System.Diagnostics;
using Gherkin.Model;
using ICSharpCode.AvalonEdit.Rendering;

namespace Gherkin.ViewModel
{
    public class EditorTabHeaderViewModel : NotifyPropertyChangedBase
    {
        private string m_FilePath;
        private EditorTabContentViewModel EditorTabContentViewModel { get; set; }
        private ICanCloseAllDocumentsChecker CanCloseAllDocumentsChecker { get; set; }

        public EditorTabHeaderViewModel(EditorTabContentViewModel editorTabContentViewModel, ICanCloseAllDocumentsChecker canCloseAllDocumentsChecker)
        {
            EditorTabContentViewModel = editorTabContentViewModel;
            m_FilePath = editorTabContentViewModel.CurrentFilePath;
            CanCloseAllDocumentsChecker = canCloseAllDocumentsChecker;

            EditorTabContentViewModel.TextEditorLoadedEvent += OnTextEditorLoaded;
            EditorTabContentViewModel.FileNameChangedEvent += OnFileNameChanged;
            EditorTabContentViewModel.DocumentSavedEvent += OnDocumentSaved;
        }

        public ICommand ClearSearchHighlightingCmd => new DelegateCommandNoArg(OnClearSearchHighlighting, CanClearSearchHighlighting);
        public ICommand CloseCmd => new DelegateCommandNoArg(OnClose);
        public ICommand CloseAllDocumentsCmd => new DelegateCommandNoArg(OnCloseAllDocuments, CanCloseAllDocumentsChecker.CanCloseAllDocuments);
        public ICommand CloseAllButThisCmd => new DelegateCommandNoArg(OnCloseAllButThis, CanCloseAllDocumentsChecker.CanCloseAllButThis);
        public ICommand SaveCmd => new DelegateCommandNoArg(OnSave);
        public ICommand SaveAsCmd => new DelegateCommandNoArg(OnSaveAs);
        public ICommand RenameCmd => new DelegateCommandNoArg(OnRename, FileExist);
        public ICommand OpenCurrentFolderCmd => new DelegateCommandNoArg(OnOpenFolder, FileExist);
        public ICommand OpenInNewEditorCmd => new DelegateCommandNoArg(OnOpenInNewEditor, CanOpenInNewEditor);

        private void OnRename()
        {
            RenameDocumentRequestedArg arg = new RenameDocumentRequestedArg(EditorTabContentViewModel);
            Util.EventAggregator<RenameDocumentRequestedArg>.Instance.Publish(this, arg);
        }
        private void OnSaveAs()
        {
            SaveAsDocumentRequestedArg arg = new SaveAsDocumentRequestedArg(EditorTabContentViewModel);
            Util.EventAggregator<SaveAsDocumentRequestedArg>.Instance.Publish(this, arg);
        }

        private bool FileExist()
        {
            return (!string.IsNullOrEmpty(m_FilePath) &&
                    File.Exists(m_FilePath));
        }

        private void OnClearSearchHighlighting()
        {
            var itemsToRemove = LineTransformers.Where(x => x is ColorizeAvalonEdit).ToList();
            foreach (var item in itemsToRemove)
            {
                LineTransformers.Remove(item);
            }
        }
        private bool CanClearSearchHighlighting()
        {
            return LineTransformers.FirstOrDefault(x => x is ColorizeAvalonEdit) != null;
        }

        private IList<IVisualLineTransformer> LineTransformers =>
            EditorTabContentViewModel.MainEditor.TextArea.TextView.LineTransformers;

        private void OnOpenFolder()
        {
            string path = Path.GetDirectoryName(m_FilePath);
            Process.Start(path);
        }

        private void OnOpenInNewEditor()
        {
            OnClose();

            OpenNewGherkinEditorRequestedArg arg = new OpenNewGherkinEditorRequestedArg(m_FilePath);
            Util.EventAggregator<OpenNewGherkinEditorRequestedArg>.Instance.Publish(this, arg);
        }
        private bool CanOpenInNewEditor()
        {
            return (!string.IsNullOrEmpty(m_FilePath) &&
                    File.Exists(m_FilePath) &&
                    !EditorTabContentViewModel.MainEditor.IsModified);
        }
        
        private void OnClose()
        {
            DeleteEditorTabRequestedArg arg = new DeleteEditorTabRequestedArg(EditorTabContentViewModel);
            Util.EventAggregator<DeleteEditorTabRequestedArg>.Instance.Publish(this, arg);
        }

        private void OnSave()
        {
            if (EditorTabContentViewModel.IsModified)
            {
                EditorTabContentViewModel.SaveFile(saveAs: false);
            }
        }

        private void OnCloseAllDocuments()
        {
            DeleteAllEditorTabsRequestedArg arg = new DeleteAllEditorTabsRequestedArg(null);
            Util.EventAggregator<DeleteAllEditorTabsRequestedArg>.Instance.Publish(this, arg);
        }

        private void OnCloseAllButThis()
        {
            DeleteAllEditorTabsRequestedArg arg = new DeleteAllEditorTabsRequestedArg(EditorTabContentViewModel);
            Util.EventAggregator<DeleteAllEditorTabsRequestedArg>.Instance.Publish(this, arg);
        }

        public string DocumentModificationStatusIcon
        {
            get
            {
                string image_name = (EditorTabContentViewModel.IsModified == true) ? "Modified.png" : "Unchanged.png";
                return PackImageURI(image_name);
            }
        }

        private double m_MaxWidthOfFileNameText = 300;
        public double MaxWidthOfFileNameText
        {
            get { return m_MaxWidthOfFileNameText; }
            set
            {
                const double WIDTH_FOR_ICONS = 40;
                m_MaxWidthOfFileNameText = Math.Max(value - WIDTH_FOR_ICONS, 1);
                base.OnPropertyChanged();
            }
        }

        private void OnTextEditorLoaded()
        {
            EditorTabContentViewModel.MainEditor.TextChanged += OnTextEditorTextChanged;
        }

        private void OnTextEditorTextChanged(object sender, EventArgs e)
        {
            NotifyModificationStatus();
        }

        public string FileName
        {
            get
            {
                if (m_FilePath != null)
                {
                    try
                    {
                        return Path.GetFileName(m_FilePath);
                    }
                    catch
                    {
                        // The the "FilePath" would not not be real file path
                        //  when grepping text as file path
                        return m_FilePath;
                    }
                }
                else
                    return Properties.Resources.Message_UnknownFileName;
            }
        }

        public string FilePath
        {
            get
            {
                return m_FilePath ?? Properties.Resources.Message_UnknownFileName;
            }
        }

        private void OnFileNameChanged(string filePath)
        {
            m_FilePath = filePath;
            base.OnPropertyChanged(nameof(FileName));
            base.OnPropertyChanged(nameof(FilePath));
        }

        private void OnDocumentSaved()
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
                Application.Current.Dispatcher.Invoke(
                     new Action(() => base.OnPropertyChanged(nameof(DocumentModificationStatusIcon))
                     ));
            });
        }
    }
}
