using Gherkin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gherkin.ViewModel
{
    public class CodePageListPopupViewModel : NotifyPropertyChangedBase
    {
        private bool m_ShowCodePageList;
        private int m_SelectedCodePageIndex;
        private EditorTabContentViewModel m_CurrentEditor;
        private EncodingInfo[] m_CodePageList;

        public CodePageListPopupViewModel()
        {
            EventAggregator<WindowDeactivatedArg>.Instance.Event += delegate { ShowCodePageList = false; };
        }

        public event Action CodePageChangedEvent;

        public ICommand CloseCodePageListCmd => new DelegateCommandNoArg(OnCloseCodePageList);
        public ICommand ReloadFileWithCodePageCmd => new DelegateCommandNoArg(OnReloadFileWithCodePage);
        public ICommand SaveAsWithEncodingCmd => new DelegateCommandNoArg(OnSaveAsWithEncoding);

        public EditorTabContentViewModel CurrentEditor
        {
            get { return m_CurrentEditor; }
            set
            {
                m_CurrentEditor = value;
                SelectDefaultCodePage();
            }
        }

        private void SelectDefaultCodePage()
        {
            var encoding = CurrentEditor?.Encoding;
            if (encoding == null) return;

            for (int i = 0; i < CodePageList.Length; i++)
            {
                if (CodePageList[i].CodePage == encoding.CodePage)
                {
                    SelectedCodePageIndex = i;
                    return;
                }
            }
        }

        public EncodingInfo[] CodePageList
        {
            get
            {
                if (m_CodePageList == null)
                {
                    m_CodePageList = Encoding.GetEncodings();
                    Array.Sort(m_CodePageList, new Comparison<EncodingInfo>(
                            (i1, i2) => i1.DisplayName.CompareTo(i2.DisplayName)));
                }
                return m_CodePageList;
            }
        }

        public bool ShowCodePageList
        {
            get { return m_ShowCodePageList; }
            set
            {
                if (m_ShowCodePageList != value)
                {
                    m_ShowCodePageList = value;
                    base.OnPropertyChanged();
                }
            }
        }

        private void OnCloseCodePageList()
        {
            ShowCodePageList = false;
        }

        public int SelectedCodePageIndex
        {
            get { return m_SelectedCodePageIndex; }
            set
            {
                m_SelectedCodePageIndex = value;
                base.OnPropertyChanged();
            }
        }

        private void OnReloadFileWithCodePage()
        {
            if (CurrentEditor == null) return;
            if (CurrentEditor.IsModified == true)
            {
                var result = MessageBox.Show(Application.Current.MainWindow,
                               Properties.Resources.Message_ConfirmReloadFileMessage,
                               Properties.Resources.Message_ConfirmReloadFileTitle,
                               MessageBoxButton.OKCancel,
                               MessageBoxImage.Question);
                if (result != MessageBoxResult.OK) return;
            }

            var newEncoding = CodePageList[SelectedCodePageIndex];
            CurrentEditor.Load(CurrentEditor.CurrentFilePath, newEncoding.GetEncoding());
            CodePageChangedEvent?.Invoke();
        }

        private void OnSaveAsWithEncoding()
        {
            var newEncoding = CodePageList[SelectedCodePageIndex];
            ShowCodePageList = false;
            bool isSaved = CurrentEditor?.SaveFile(saveAs:true, newEncoding: newEncoding.GetEncoding()) == true;
            if (isSaved)
            {
                CodePageChangedEvent?.Invoke();
                CurrentEditor.Load(CurrentEditor.CurrentFilePath);  // reload with new encoding
            }
        }
    }
}
