﻿using System;
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

namespace Gherkin.ViewModel
{
    public class GherkinSettings : NotifyPropertyChangedBase
    {
        private readonly Color FolderTextColorDefault = Colors.Blue;
        private readonly Color KeywordColorDefault = Colors.Blue;
        private readonly Color StepWordColorDefault = Colors.DarkCyan;
        private readonly Color TableColorDefault = Colors.Peru;
        private readonly Color TagColorDefault = Colors.Purple;
        private readonly Color DocStringColorDefault = Colors.Peru;
        private readonly Color ConstantsColorDefault = Colors.Peru;
        private readonly Color MockAttributeColorDefault = Colors.Green;

        private IAppSettings m_AppSettings;
        private string m_CurrentGherkinLanguageKey = "en";
        private string m_GherkinKeywords = "";

        public DelegateCommandNoArg ResetFolderTextColorCmd => new DelegateCommandNoArg(OnResetFolderTextColor, CanExecuteResetFolderTextColor);
        public DelegateCommandNoArg ResetKeywordColorCmd => new DelegateCommandNoArg(OnResetKeywordColor, CanExecuteResetKeywordColor);
        public ICommand ResetStepWordColorCmd => new DelegateCommandNoArg(OnResetStepWordColor, CanExecuteResetStepWordColor);
        public ICommand ResetTagColorCmd => new DelegateCommandNoArg(OnResetTagColor, CanExecuteResetTagColor);
        public ICommand ResetTableColorCmd => new DelegateCommandNoArg(OnResetTableColor, CanExecuteResetTableColor);
        public ICommand ResetDocStringColorCmd => new DelegateCommandNoArg(OnResetDocStringColor, CanExecuteResetDocStringColor);
        public ICommand ResetConstantsColorCmd => new DelegateCommandNoArg(OnResetConstantsColor, CanExecuteResetConstantsColor);
        public ICommand ResetMockAttributeColorCmd => new DelegateCommandNoArg(OnResetMockAttributeColor, CanExecuteResetMockAttributeColor);
        public ICommand ResetHighlightingColorsCmd => new DelegateCommandNoArg(OnResetHighlightingColors, CanExecuteResetHighlightingColors);
        public ICommand ChangeLanguageCmd => new DelegateCommand<string>(OnChangeLanguage, CanExecuteChangeLanguage);

        public ICommand CreateHighlightingFilesCmd => new DelegateCommandNoArg(OnCreateHighlightingFiles);
        public ICommand CreateKeywordsFileCmd => new DelegateCommandNoArg(OnCreateKeywordsFile);

        public ObservableCollection<EditorTab> TabPanels { get; set; }

        public GherkinSettings(IAppSettings appSettings)
        {
            m_AppSettings = appSettings;
            GherkinUtil.RegisterGherkinHighlighting();

            EventAggregator<CurrentGherkinLanguageArg>.Instance.Event += OnGherkinLanguage;
        }

        public bool SupportUnicode
        {
            get { return m_AppSettings.SupportUnicode; }
            set
            {
                m_AppSettings.SupportUnicode = value;
                base.OnPropertyChanged();
            }
        }

        public bool ShowScenarioIndexByDefault
        {
            get { return m_AppSettings.ShowScenarioIndexByDefault; }
            set
            {
                m_AppSettings.ShowScenarioIndexByDefault = value;
                base.OnPropertyChanged();
            }
        }

        public bool ShowSplitViewByDefault
        {
            get { return m_AppSettings.ShowSplitViewByDefault; }
            set
            {
                m_AppSettings.ShowSplitViewByDefault = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsCloseTablesFoldingByDefault
        {
            get { return m_AppSettings.IsCloseTablesFoldingByDefault; }
            set
            {
                m_AppSettings.IsCloseTablesFoldingByDefault = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsCloseScenarioFoldingByDefault
        {
            get { return m_AppSettings.IsCloseScenarioFoldingByDefault; }
            set
            {
                m_AppSettings.IsCloseScenarioFoldingByDefault = value;
                base.OnPropertyChanged();
            }
        }

        public bool HighlightCurrentLine
        {
            get { return m_AppSettings.HighlightCurrentLine; }
            set
            {
                m_AppSettings.HighlightCurrentLine = value;
                foreach (var tab in TabPanels)
                {
                    tab.EditorView.HighlightCurrentLine = value;
                }
                base.OnPropertyChanged();
            }
        }

        public bool ShowCurrentLineBorder
        {
            get { return m_AppSettings.ShowCurrentLineBorder; }
            set
            {
                m_AppSettings.ShowCurrentLineBorder = value;
                foreach (var tab in TabPanels)
                {
                    tab.EditorView.ShowCurrentLineBorder = value;
                }
                base.OnPropertyChanged();
            }
        }

        public Brush FoldingTextBrush
        {
            get { return new SolidColorBrush(SelectedFoldingTextColor); }
            set
            {
                ChangeFoldingTextColorOfAvalonEdit(value);
                base.OnPropertyChanged();
                RefreshFolding4AllEditors();
            }
        }

        public void ChangeFoldingTextColorOfAvalonEdit()
        {
            ChangeFoldingTextColorOfAvalonEdit(FoldingTextBrush);
        }

        private void ChangeFoldingTextColorOfAvalonEdit(Brush brush)
        {
            ICSharpCode.AvalonEdit.Folding.FoldingElementGenerator.TextBrush = brush;
        }

        private void RefreshFolding4AllEditors()
        {
            foreach (var tab in TabPanels)
            {
                tab.EditorView.UpdateFoldings(refresh: true);
            }
        }

        public Color SelectedFoldingTextColor
        {
            get { return m_AppSettings.ColorOfFoldingText.ToColor(); }
            set
            {
                string colorName = value.ToName();
                if (m_AppSettings.ColorOfFoldingText != colorName)
                {
                    m_AppSettings.ColorOfFoldingText = colorName;
                    FoldingTextBrush = new SolidColorBrush(colorName.ToColor());
                    base.OnPropertyChanged();
                }
            }
        }

        private void OnResetFolderTextColor()
        {
            SelectedFoldingTextColor = FolderTextColorDefault;
        }
        private bool CanExecuteResetFolderTextColor()
        {
            return SelectedFoldingTextColor != FolderTextColorDefault;
        }

        public Brush HighlightingKeywordBrush
        {
            get { return new SolidColorBrush(SelectedHighlightingKeywordColor); }
        }
        public Color SelectedHighlightingKeywordColor
        {
            get { return m_AppSettings.ColorOfHighlightingKeyword.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingKeyword),
                                     value,
                                     nameof(HighlightingKeywordBrush));
            }
        }
        private void OnResetKeywordColor()
        {
            SelectedHighlightingKeywordColor = KeywordColorDefault;
        }
        private bool CanExecuteResetKeywordColor()
        {
            return SelectedHighlightingKeywordColor != KeywordColorDefault;
        }

        /// <summary>
        /// Set highlighting color.
        /// </summary>
        /// <param name="colorPropertyNameOfAppSettings"></param>
        /// <param name="newColor"></param>
        /// <param name="brushProperty"></param>
        /// <param name="selectedColorName"></param>
        private void SetHighlightingColor(string colorPropertyNameOfAppSettings, Color newColor, string brushProperty, [CallerMemberName] string selectedColorName = null)
        {
            var appSettingProperty = typeof(IAppSettings).GetProperty(colorPropertyNameOfAppSettings);

            string currentColorName = appSettingProperty.GetValue(m_AppSettings, null) as string;
            string newColorName = newColor.ToName();
            if (currentColorName != newColorName)
            {
                appSettingProperty.SetValue(m_AppSettings, newColorName);
                RefreshHighlithingColor();
                base.OnPropertyChanged(selectedColorName);
                base.OnPropertyChanged(brushProperty);
            }
        }

        private void RefreshHighlithingColor()
        {
            foreach (var tab in TabPanels)
            {
                tab.EditorView.UpdateGherkinHighlighing(installFolding: false);
            }
        }

        public Brush HighlightingStepWordBrush
        {
            get { return new SolidColorBrush(SelectedHighlightingStepWordColor); }
        }
        public Color SelectedHighlightingStepWordColor
        {
            get { return m_AppSettings.ColorOfHighlightingStepWord.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingStepWord),
                                     value,
                                     nameof(HighlightingStepWordBrush));
            }
        }
        private void OnResetStepWordColor()
        {
            SelectedHighlightingStepWordColor = StepWordColorDefault;
        }
        private bool CanExecuteResetStepWordColor()
        {
            return SelectedHighlightingStepWordColor != StepWordColorDefault;
        }

        public Brush HighlightingTableBrush
        {
            get { return new SolidColorBrush(SelectedHighlightingTableColor); }
        }
        public Color SelectedHighlightingTableColor
        {
            get { return m_AppSettings.ColorOfHighlightingTable.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingTable),
                                     value,
                                     nameof(HighlightingTableBrush));
            }
        }
        private void OnResetTableColor()
        {
            SelectedHighlightingTableColor = TableColorDefault;
        }
        private bool CanExecuteResetTableColor()
        {
            return SelectedHighlightingTableColor != TableColorDefault;
        }

        public Brush HighlightingTagBrush
        {
            get { return new SolidColorBrush(SelectedHighlightingTagColor); }
        }
        public Color SelectedHighlightingTagColor
        {
            get { return m_AppSettings.ColorOfHighlightingTag.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingTag),
                                     value,
                                     nameof(HighlightingTagBrush));
            }
        }
        private void OnResetTagColor()
        {
            SelectedHighlightingTagColor = TagColorDefault;
        }
        private bool CanExecuteResetTagColor()
        {
            return SelectedHighlightingTagColor != TagColorDefault;
        }

        public Brush HighlightingDocStringBrush
        {
            get { return new SolidColorBrush(SelectedHighlightingDocStringColor); }
        }
        public Color SelectedHighlightingDocStringColor
        {
            get { return m_AppSettings.ColorOfHighlightingDocString.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingDocString),
                                     value,
                                     nameof(HighlightingDocStringBrush));
            }
        }
        private void OnResetDocStringColor()
        {
            SelectedHighlightingDocStringColor = DocStringColorDefault;
        }
        private bool CanExecuteResetDocStringColor()
        {
            return SelectedHighlightingDocStringColor != DocStringColorDefault;
        }

        public Brush HighlightingConstantsBrush
        {
            get { return new SolidColorBrush(SelectedHighlightingConstantsColor); }
        }
        public Color SelectedHighlightingConstantsColor
        {
            get { return m_AppSettings.ColorOfHighlightingConstants.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingConstants),
                                     value,
                                     nameof(HighlightingConstantsBrush));
            }
        }
        private void OnResetConstantsColor()
        {
            SelectedHighlightingConstantsColor = ConstantsColorDefault;
        }
        private bool CanExecuteResetConstantsColor()
        {
            return SelectedHighlightingConstantsColor != ConstantsColorDefault;
        }

        public Brush HighlightingMockAttributeBrush
        {
            get { return new SolidColorBrush(SelectedHighlightingMockAttributeColor); }
        }
        public Color SelectedHighlightingMockAttributeColor
        {
            get { return m_AppSettings.ColorOfHighlightingMockAttribute.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingMockAttribute),
                                     value,
                                     nameof(HighlightingMockAttributeBrush));
            }
        }
        private void OnResetMockAttributeColor()
        {
            SelectedHighlightingMockAttributeColor = MockAttributeColorDefault;
        }
        private bool CanExecuteResetMockAttributeColor()
        {
            return SelectedHighlightingMockAttributeColor != MockAttributeColorDefault;
        }

        private void OnResetHighlightingColors()
        {
            OnResetKeywordColor();
            OnResetStepWordColor();
            OnResetTagColor();
            OnResetTableColor();
            OnResetDocStringColor();
            OnResetConstantsColor();
            OnResetMockAttributeColor();
        }
        private bool CanExecuteResetHighlightingColors()
        {
            return CanExecuteResetKeywordColor() ||
                   CanExecuteResetStepWordColor() ||
                   CanExecuteResetTagColor() ||
                   CanExecuteResetTableColor() ||
                   CanExecuteResetDocStringColor() ||
                   CanExecuteResetConstantsColor() ||
                   CanExecuteResetMockAttributeColor();
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
                }
            }
        }

        private void OnGherkinLanguage(object sender, CurrentGherkinLanguageArg arg)
        {
            if (m_CurrentGherkinLanguageKey == arg.LanguageKey) return;

            m_CurrentGherkinLanguageKey = arg.LanguageKey;
            GherkinKeywords = GherkinKeywordGenerator.GenerateKeywordsToolTip(m_CurrentGherkinLanguageKey);
        }

        /// <summary>
        /// We do not need to confirm saving again in OnClosing of main window
        /// when changing current language
        /// </summary>
        private bool IsChangingLanguage { get; set; } = false;

        public MessageBoxResult SaveAllFilesWithRequesting()
        {
            if (IsChangingLanguage) return MessageBoxResult.Yes;

            foreach (EditorTab tab in TabPanels)
            {
                MessageBoxResult result = SaveCurrentFileWithRequesting(tab.EditorView);
                if (result == MessageBoxResult.Cancel)
                {
                    return result;
                }
            }

            return MessageBoxResult.Yes;
        }

        public MessageBoxResult SaveCurrentFileWithRequesting(EditorView editor)
        {
            return editor.SaveCurrentFileWithRequesting();
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

            result = SaveAllFilesWithRequesting();
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

        public Visibility EnableCreateHighlightingFiles
        {
            get
            {
                if (ConfigReader.GetValue<bool>("showCreateHighlightingFilesButton", false))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
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
    }
}
