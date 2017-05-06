using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Gherkin.Model;
using Gherkin.Util;
using System.Runtime.CompilerServices;
using Gherkin.View;

namespace Gherkin.ViewModel
{
    public class GherkinSettingViewModel : NotifyPropertyChangedBase
    {
        private IAppSettings m_AppSettings;
        private string m_CurrentGherkinLanguageKey = "en";
        private string m_GherkinKeywords = "";
        private ObservableCollection<EditorTabItem> m_TabPanels;

        public GherkinSettingViewModel(IAppSettings appSettings, SettingPropertyGridViewModel editorSettingViewModel)
        {
            m_AppSettings = appSettings;
            PropertyGridViewModel = editorSettingViewModel;
            GherkinUtil.RegisterGherkinHighlighting();

            EventAggregator<CurrentGherkinLanguageArg>.Instance.Event += OnGherkinLanguage;
        }

        public ICommand ChangeLanguageCmd => new DelegateCommand<string>(OnChangeLanguage, CanExecuteChangeLanguage);
        public ICommand CreateHighlightingFilesCmd => new DelegateCommandNoArg(OnCreateHighlightingFiles);
        public ICommand CreateKeywordsFileCmd => new DelegateCommandNoArg(OnCreateKeywordsFile);
        public ICommand ShowEditorSettingCmd => new DelegateCommandNoArg(OnShowEditorSetting);

        public ObservableCollection<EditorTabItem> TabPanels
        {
            get { return m_TabPanels; }
            set
            {
                m_TabPanels = value;
                PropertyGridViewModel.SetTabPanels(value);
            }
        }

        public SettingPropertyGridViewModel PropertyGridViewModel { get; set; }

        public bool HasGherkinKeywords
        {
            get { return m_GherkinKeywords.Length > 0; }
        }

        public string GherkinKeywords
        {
            get
            {
                return m_GherkinKeywords;
            }
            set
            {
                if (m_GherkinKeywords != value)
                {
                    m_GherkinKeywords = value;
                    base.OnPropertyChanged();
                    base.OnPropertyChanged(nameof(HasGherkinKeywords));
                }
            }
        }

        private void OnGherkinLanguage(object sender, CurrentGherkinLanguageArg arg)
        {
            if ((m_CurrentGherkinLanguageKey != arg.LanguageKey) || string.IsNullOrEmpty(m_GherkinKeywords))
            {
                m_CurrentGherkinLanguageKey = arg.LanguageKey;
                GherkinKeywords = GherkinKeywordGenerator.GenerateKeywordsToolTip(m_CurrentGherkinLanguageKey);
            }
        }

        /// <summary>
        /// We do not need to confirm saving again in OnClosing of main window
        /// when changing current language
        /// </summary>
        private bool IsChangingLanguage { get; set; } = false;

        public MessageBoxResult SaveAllFilesWithRequesting(EditorTabContentViewModel excluded)
        {
            if (IsChangingLanguage) return MessageBoxResult.Yes;

            foreach (EditorTabItem tab in TabPanels)
            {
                if (tab.EditorTabContentViewModel != excluded)
                {
                    MessageBoxResult result = SaveCurrentFileWithRequesting(tab.EditorTabContentViewModel);
                    if (result == MessageBoxResult.Cancel)
                    {
                        return result;
                    }
                }
            }

            return MessageBoxResult.Yes;
        }

        public MessageBoxResult SaveCurrentFileWithRequesting(EditorTabContentViewModel editor)
        {
            return editor.SaveCurrentFileWithRequesting();
        }

        public void ChangeFoldingTextColorOfAvalonEdit()
        {
            PropertyGridViewModel.ChangeFoldingTextColorOfAvalonEdit();
        }

        private void OnChangeLanguage(string newLanguage)
        {
            Languages language = EnumUtil.ToEnumValue<Languages>(newLanguage);
            if (m_AppSettings.Language == language) return;

            MessageBoxResult result = MessageBox.Show(
                                           Gherkin.Properties.Resources.Message_ConfirmRestartApp,
                                           Gherkin.Properties.Resources.Message_ConfirmRestartAppTitle,
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Information);

            if (result == MessageBoxResult.No) return;

            result = SaveAllFilesWithRequesting(excluded: null);
            if (result != MessageBoxResult.Cancel)
            {
                m_AppSettings.Language = language;
                IsChangingLanguage = true;
                Util.Util.RestartApplication();
            }
        }
        private bool CanExecuteChangeLanguage(string newLanguag)
        {
            Languages language = EnumUtil.ToEnumValue<Languages>(newLanguag);
            return m_AppSettings.Language != language;
        }

        public bool showCreateHighlightingFilesButton
        {
            get
            {
                return ConfigReader.GetValue<bool>("showCreateHighlightingFilesButton", false);
            }
        }

        private void OnCreateHighlightingFiles()
        {
            GherkinHighlightingXSHDBuilder.CreateGherkinHighlightingFiles();
        }

        private void OnCreateKeywordsFile()
        {
            GherkinKeywordGenerator.GenerateKeywords();
        }

        private void OnShowEditorSetting()
        {
            Gherkin.View.EditorSetting window = new EditorSetting(PropertyGridViewModel);
            window.Show();
        }
    }
}
