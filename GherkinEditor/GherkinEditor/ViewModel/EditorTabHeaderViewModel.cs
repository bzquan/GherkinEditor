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
            CanCloseAllDocumentsChecker = canCloseAllDocumentsChecker;

            EditorTabContentViewModel.TextEditorLoadedEvent += OnTextEditorLoaded;
            EditorTabContentViewModel.FileNameChangedEvent += OnFileNameChanged;
            EditorTabContentViewModel.DocumentSavedEvent += OnDocumentSaved;
        }

        public ICommand CloseAllDocumentsCmd => new DelegateCommandNoArg(OnCloseAllDocuments, CanCloseAllDocumentsChecker.CanCloseAllDocuments);
        public ICommand CloseAllButThisCmd => new DelegateCommandNoArg(OnCloseAllButThis, CanCloseAllDocumentsChecker.CanCloseAllButThis);
        public DelegateCommandNoArg DeleteTabCmd => new DelegateCommandNoArg(OnDelete);
        public ICommand SaveCmd => new DelegateCommandNoArg(OnSave);

        private void OnDelete()
        {
            DeleteEditorTabRequested arg = new DeleteEditorTabRequested(EditorTabContentViewModel);
            Util.EventAggregator<DeleteEditorTabRequested>.Instance.Publish(this, arg);
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
            DeleteAllEditorTabsRequested arg = new DeleteAllEditorTabsRequested(null);
            Util.EventAggregator<DeleteAllEditorTabsRequested>.Instance.Publish(this, arg);
        }

        private void OnCloseAllButThis()
        {
            DeleteAllEditorTabsRequested arg = new DeleteAllEditorTabsRequested(EditorTabContentViewModel);
            Util.EventAggregator<DeleteAllEditorTabsRequested>.Instance.Publish(this, arg);
        }

        public string DocumentModificationStatusIcon
        {
            get
            {
                string image_name = (EditorTabContentViewModel.IsModified == true) ? "Modified.png" : "Unchanged.png";
                return PackImageURI(image_name);
            }
        }

        private double m_MaxWidthOfFileNameText = 40;
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
                    return System.IO.Path.GetFileName(m_FilePath);
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
