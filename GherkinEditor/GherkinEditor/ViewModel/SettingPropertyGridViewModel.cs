using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Gherkin.Properties;
using Gherkin.View;
using Gherkin.Util;
using static Gherkin.Util.ForEachUtil;

namespace Gherkin.ViewModel
{
    public class SettingPropertyGridViewModel : NotifyPropertyChangedBase
    {
        private readonly Color KeywordColorDefault = Colors.Blue;
        private readonly Color StepWordColorDefault = Colors.DarkCyan;
        private readonly Color TableColorDefault = Colors.Peru;
        private readonly Color TagColorDefault = Colors.Purple;
        private readonly Color DocStringColorDefault = Colors.Peru;
        private readonly Color ConstantsColorDefault = Colors.Peru;
        private readonly Color MockAttributeColorDefault = Colors.Green;
        private readonly Color FolderTextColorDefault = Colors.Blue;

        private ObservableCollection<EditorTabItem> m_TabPanels;
        private IAppSettings m_AppSettings;

        /// <summary>
        /// Reset Gherkin keywords highlighting.
        /// Note: It is defined as private to prevent to be displayed on EditorSetting window
        /// </summary>
        private ICommand m_ResetHighlightingColorsCmd => new DelegateCommandNoArg(OnResetHighlightingColors, CanExecuteResetHighlightingColors);

        public SettingPropertyGridViewModel(IAppSettings appSettings)
        {
            m_AppSettings = appSettings;

            Model.BitmapImageCache.CacheSizee = appSettings.ImageCacheSize;
            Model.LaTexImageCache.CacheSizee = appSettings.ImageCacheSize;
        }

        public void SetTabPanels(ObservableCollection<EditorTabItem> tabPanels)
        {
            m_TabPanels = tabPanels;
        }

        public ICommand GetResetHighlightingColorsCmd() => m_ResetHighlightingColorsCmd;

        public void ResetProperty(string property)
        {
            if (string.IsNullOrEmpty(property)) return;

            if (property == nameof(FontFamily4Gherkin))
            {
                FontFamily4Gherkin = GetFontFamily("Kaiti");
            }
            else if (property == nameof(FontSize4Gherkin))
            {
                FontSize4Gherkin = 11;
            }
            else if (property == nameof(FontFamily4NonGherkin))
            {
                FontFamily4NonGherkin = GetFontFamily("Meiryo");
            }
            else if (property == nameof(FontSize4NonGherkin))
            {
                FontSize4NonGherkin = 10;
            }
            else if (property == nameof(HighlightingKeywordColor))
            {
                HighlightingKeywordColor = KeywordColorDefault;
            }
            else if (property == nameof(HighlightingStepWordColor))
            {
                HighlightingStepWordColor = StepWordColorDefault;
            }
            else if (property == nameof(HighlightingTableColor))
            {
                HighlightingTableColor = TableColorDefault;
            }
            else if (property == nameof(HighlightingTagColor))
            {
                HighlightingTagColor = TagColorDefault;
            }
            else if (property == nameof(HighlightingDocStringColor))
            {
                HighlightingDocStringColor = DocStringColorDefault;
            }
            else if (property == nameof(HighlightingConstantsColor))
            {
                HighlightingConstantsColor = ConstantsColorDefault;
            }
            else if (property == nameof(HighlightingMockAttributeColor))
            {
                HighlightingMockAttributeColor = MockAttributeColorDefault;
            }
            else if (property == nameof(FoldingTextColor))
            {
                FoldingTextColor = FolderTextColorDefault;
            }
        }

        [LocalizedCategory("MenuCompile_Comple", typeof(Resources))]
        [LocalizedDisplayName("MenuCompile_SupportUnicode", typeof(Resources))]
        public bool SupportUnicode
        {
            get { return m_AppSettings.SupportUnicode; }
            set
            {
                m_AppSettings.SupportUnicode = value;
                base.OnPropertyChanged(nameof(SupportUnicodeIcon));
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuCompile_Comple", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_AutoGenGUID", typeof(Resources))]
        public bool GenerateGUIDforScenario
        {
            get { return m_AppSettings.GenerateGUIDforScenario; }
            set
            {
                m_AppSettings.GenerateGUIDforScenario = value;
                base.OnPropertyChanged();
            }
        }

        public DrawingImage SupportUnicodeIcon
        {
            get
            {
                if (SupportUnicode)
                    return Util.Util.DrawingImageByOverlapping("Unicode.png", "Tick64.png");
                else
                    return Util.Util.DrawingImageFromResource("Unicode.png");
            }
        }

        [Category("Current Line")]
        [LocalizedDisplayName("MenuSetting_HighlightCurrentLine", typeof(Resources))]
        public bool HighlightCurrentLine
        {
            get { return m_AppSettings.HighlightCurrentLine; }
            set
            {
                m_AppSettings.HighlightCurrentLine = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.HighlightCurrentLine = value);
                base.OnPropertyChanged();
            }
        }

        [Category("Current Line")]
        [LocalizedDisplayName("MenuSetting_ShowCurrentLineBorder", typeof(Resources))]
        public bool ShowCurrentLineBorder
        {
            get { return m_AppSettings.ShowCurrentLineBorder; }
            set
            {
                m_AppSettings.ShowCurrentLineBorder = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.ShowCurrentLineBorder = value);
                base.OnPropertyChanged();
            }
        }

        [Category("Open hyper link")]
        [LocalizedDisplayName("MenuSetting_RequireControlModifierForHyperlinkClick", typeof(Resources))]
        [LocalizedDescription("Tooltip_RequireControlModifierForHyperlinkClick", typeof(Resources))]
        public bool RequireControlModifierForHyperlinkClick
        {
            get { return m_AppSettings.RequireControlModifierForHyperlinkClick; }
            set
            {
                m_AppSettings.RequireControlModifierForHyperlinkClick = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateRequireControlModifierForHyperlinkClick());
                base.OnPropertyChanged();
            }
        }

        [Category("Column ruler")]
        [LocalizedDisplayName("MenuSetting_ShowColumnRuler", typeof(Resources))]
        public bool ShowColumnRuler
        {
            get { return m_AppSettings.ShowColumnRuler; }
            set
            {
                m_AppSettings.ShowColumnRuler = value;
                UpdateColumnRuler();
                base.OnPropertyChanged();
            }
        } 

        [Category("Column ruler")]
        [LocalizedDisplayName("MenuSetting_ColumnRulerPosition", typeof(Resources))]
        public int ColumnRulerPositon
        {
            get { return m_AppSettings.ColumnRulerPositon; }
            set
            {
                int v = Math.Max(60, value);
                int pos = Math.Min(300, v);
                m_AppSettings.ColumnRulerPositon = pos;
                UpdateColumnRuler();
                base.OnPropertyChanged();
            }
        }

        private void UpdateColumnRuler()
        {
            foreach (var tab in m_TabPanels)
            {
                tab.EditorTabContentViewModel.UpdateColumnRuler();
            }
        }

        [Category("Font")]
        [ItemsSource(typeof(FontFamilyItemsSource))]
        [LocalizedDisplayName("Tooltip_FontForGherkin", typeof(Resources))]
        public FontFamily FontFamily4Gherkin
        {
            get { return GetFontFamily(m_AppSettings.FontFamilyName); }
            set
            {
                m_AppSettings.FontFamilyName = value.ToString();
                base.OnPropertyChanged();
            }
        }

        [Category("Font")]
        [LocalizedDisplayName("Tooltip_FontSizeForGherkin", typeof(Resources))]
        public int FontSize4Gherkin
        {
            get { return int.Parse(m_AppSettings.FontSize); }
            set
            {
                if ((value >= 10) && (value <= 28))
                {
                    m_AppSettings.FontSize = value.ToString();
                }
                base.OnPropertyChanged();
            }
        }

        [Category("Font")]
        [ItemsSource(typeof(FontFamilyItemsSource))]
        [LocalizedDisplayName("Tooltip_FontForNonGherkin", typeof(Resources))]
        public FontFamily FontFamily4NonGherkin
        {
            get { return GetFontFamily(m_AppSettings.FontFamilyName4NonGherkin); }
            set
            {
                m_AppSettings.FontFamilyName4NonGherkin = value.ToString();
                base.OnPropertyChanged();
            }
        }

        [Category("Font")]
        [LocalizedDisplayName("Tooltip_FontSizeForNonGherkin", typeof(Resources))]
        public int FontSize4NonGherkin
        {
            get { return int.Parse(m_AppSettings.FontSize4NonGherkin); }
            set
            {
                if ((value >= 10) && (value <= 28))
                {
                    m_AppSettings.FontSize4NonGherkin = value.ToString();
                }
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_KeywordColor", typeof(Resources))]
        public Color HighlightingKeywordColor
        {
            get { return m_AppSettings.ColorOfHighlightingKeyword.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingKeyword), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_StepWordColor", typeof(Resources))]
        public Color HighlightingStepWordColor
        {
            get { return m_AppSettings.ColorOfHighlightingStepWord.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingStepWord), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_TableColor", typeof(Resources))]
        public Color HighlightingTableColor
        {
            get { return m_AppSettings.ColorOfHighlightingTable.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingTable), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_TagColor", typeof(Resources))]
        public Color HighlightingTagColor
        {
            get { return m_AppSettings.ColorOfHighlightingTag.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingTag), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_DocStringColor", typeof(Resources))]
        public Color HighlightingDocStringColor
        {
            get { return m_AppSettings.ColorOfHighlightingDocString.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingDocString), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_ConstantsColor", typeof(Resources))]
        public Color HighlightingConstantsColor
        {
            get { return m_AppSettings.ColorOfHighlightingConstants.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingConstants), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_MockColor", typeof(Resources))]
        public Color HighlightingMockAttributeColor
        {
            get { return m_AppSettings.ColorOfHighlightingMockAttribute.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.ColorOfHighlightingMockAttribute), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinFolding", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_DefaultTableFolding", typeof(Resources))]
        public bool IsCloseTablesFoldingByDefault
        {
            get { return m_AppSettings.IsCloseTablesFoldingByDefault; }
            set
            {
                m_AppSettings.IsCloseTablesFoldingByDefault = value;
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_GherkinFolding", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_DefaultScenarioFolding", typeof(Resources))]
        public bool IsCloseScenarioFoldingByDefault
        {
            get { return m_AppSettings.IsCloseScenarioFoldingByDefault; }
            set
            {
                m_AppSettings.IsCloseScenarioFoldingByDefault = value;
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_GherkinFolding", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_FoldTextColor", typeof(Resources))]
        public Color FoldingTextColor
        {
            get { return m_AppSettings.ColorOfFoldingText.ToColor(); }
            set
            {
                ChangeFoldingTextColorOfAvalonEdit(value);
                m_AppSettings.ColorOfFoldingText = value.ToName();
                RefreshFolding4AllEditors();
                base.OnPropertyChanged();
            }
        }
        
        public void ChangeFoldingTextColorOfAvalonEdit()
        {
            ChangeFoldingTextColorOfAvalonEdit(FoldingTextColor);
        }

        private void ChangeFoldingTextColorOfAvalonEdit(Color color)
        {
            ICSharpCode.AvalonEdit.Folding.FoldingElementGenerator.TextBrush = new SolidColorBrush(color);
        }

        private void RefreshFolding4AllEditors()
        {
            m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateFoldings(refresh: true));
        }

        [Category("Editor window")]
        [LocalizedDisplayName("MenuSetting_ShowScenarioIndexByDefault", typeof(Resources))]
        public bool ShowScenarioIndexByDefault
        {
            get { return m_AppSettings.ShowScenarioIndexByDefault; }
            set
            {
                m_AppSettings.ShowScenarioIndexByDefault = value;
                base.OnPropertyChanged();
            }
        }

        [Category("Editor window")]
        [LocalizedDisplayName("MenuSetting_SplitEditorByDefault", typeof(Resources))]
        public bool ShowSplitViewByDefault
        {
            get { return m_AppSettings.ShowSplitViewByDefault; }
            set
            {
                m_AppSettings.ShowSplitViewByDefault = value;
                base.OnPropertyChanged();
            }
        }

        [Category("Editor window")]
        [DisplayName("Word wrap")]
        public bool WordWrap
        {
            get { return m_AppSettings.WordWrap; }
            set
            {
                m_AppSettings.WordWrap = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateWordWrap());
                base.OnPropertyChanged();
            }
        }

        [Category("Tab")]
        [LocalizedDisplayName("MenuSetting_ConvertTabsToSpaces", typeof(Resources))]
        public bool ConvertTabsToSpaces
        {
            get { return m_AppSettings.ConvertTabsToSpaces; }
            set
            {
                m_AppSettings.ConvertTabsToSpaces = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateConvertTabsToSpaces());
                base.OnPropertyChanged();
            }
        }

        [Category("Tab")]
        [LocalizedDisplayName("MenuSetting_ConvertTabsToSpaces", typeof(Resources))]
        [ItemsSource(typeof(IndentationSizeItemsSource))]
        public int IndentionSize
        {
            get { return m_AppSettings.IndentationSize; }
            set
            {
                m_AppSettings.IndentationSize = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateIndentationSize());
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_SpecialSymbols", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_ShowEndOfLine", typeof(Resources))]
        public bool ShowEndOfLine
        {
            get { return m_AppSettings.ShowEndOfLine; }
            set
            {
                m_AppSettings.ShowEndOfLine = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateShowEndOfLine());
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_SpecialSymbols", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_ShowSpaces", typeof(Resources))]
        public bool ShowSpaces
        {
            get { return m_AppSettings.ShowSpaces; }
            set
            {
                m_AppSettings.ShowSpaces = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateShowSpaces());
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_SpecialSymbols", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_ShowTabs", typeof(Resources))]
        public bool ShowTabs
        {
            get { return m_AppSettings.ShowTabs; }
            set
            {
                m_AppSettings.ShowTabs = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateShowTabs());
                base.OnPropertyChanged();
            }
        }

        [DisplayName("Image Cache Size")]
        public int ImageCacheSize
        {
            get { return m_AppSettings.ImageCacheSize; }
            set
            {
                if ((value >= 10) && (value <=200))
                {
                    m_AppSettings.ImageCacheSize = value;
                    Model.BitmapImageCache.CacheSizee = value;
                    Model.LaTexImageCache.CacheSizee = value;
                }
                base.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Set highlighting color.
        /// </summary>
        /// <param name="colorPropertyNameOfAppSettings"></param>
        /// <param name="newColor"></param>
        /// <param name="selectedColorName"></param>
        private void SetHighlightingColor(string colorPropertyNameOfAppSettings, Color newColor, [CallerMemberName] string selectedColorName = null)
        {
            var appSettingProperty = typeof(IAppSettings).GetProperty(colorPropertyNameOfAppSettings);

            string currentColorName = appSettingProperty.GetValue(m_AppSettings, null) as string;
            string newColorName = newColor.ToName();
            if (currentColorName != newColorName)
            {
                appSettingProperty.SetValue(m_AppSettings, newColorName);
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateGherkinHighlighing(installFolding: false));
                base.OnPropertyChanged(selectedColorName);
            }
        }

        /// <summary>
        /// Get font family name.
        /// We dont support localized font family name in setting to avoid
        /// inconsistence with main window
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private FontFamily GetFontFamily(string name)
        {
            return new FontFamily(name);
//            return FontFamilyItemsSource.GetLocalizedFontFamily(new FontFamily(name));
        }

        private void OnResetHighlightingColors()
        {
            ResetProperty(nameof(HighlightingKeywordColor));
            ResetProperty(nameof(HighlightingStepWordColor));
            ResetProperty(nameof(HighlightingTableColor));
            ResetProperty(nameof(HighlightingTagColor));
            ResetProperty(nameof(HighlightingDocStringColor));
            ResetProperty(nameof(HighlightingConstantsColor));
            ResetProperty(nameof(HighlightingMockAttributeColor));
            ResetProperty(nameof(FoldingTextColor));
        }
        private bool CanExecuteResetHighlightingColors()
        {
            return (HighlightingKeywordColor != KeywordColorDefault) ||
                   (HighlightingStepWordColor != StepWordColorDefault) ||
                   (HighlightingTableColor != TableColorDefault) ||
                   (HighlightingTagColor != TagColorDefault) ||
                   (HighlightingDocStringColor != DocStringColorDefault) ||
                   (HighlightingConstantsColor != ConstantsColorDefault) ||
                   (HighlightingMockAttributeColor != MockAttributeColorDefault) ||
                   (FoldingTextColor != FolderTextColorDefault);
        }
    }

    class FontFamilyItemsSource : IItemsSource
    {
        private static List<FontFamily> s_SystemFonts;
        private static ItemCollection s_Items;

        public ItemCollection GetValues()
        {
            if (s_Items == null)
            {
                List<FontFamily> fonts = SystemFonts;
                s_Items = new ItemCollection();
                fonts.ForEach(x => s_Items.Add(x));
            }

            return s_Items;
        }

        public List<FontFamily> SystemFonts
        {
            get
            {
                if (s_SystemFonts == null)
                {
                    LoadSystemFonts();
                }
                return s_SystemFonts;
            }
        }

        private void LoadSystemFonts()
        {
            s_SystemFonts = Fonts.SystemFontFamilies.OrderBy(f => f.ToString()).ToList();
        }

        /// <summary>
        /// Convert to localized FontFamily
        /// Note: It is not used currently
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        private static FontFamily GetLocalizedFontFamily(FontFamily font)
        {
            var currentLang = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag);
            string localizedName = font.FamilyNames.FirstOrDefault(o => o.Key == currentLang).Value;

            return (!string.IsNullOrEmpty(localizedName)) ? new FontFamily(localizedName) : font;
        }
    }

    class IndentationSizeItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection() { 2, 4, 6, 8};
            return sizes;
        }
    }

    class MathFormulaScaleItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection() { 10, 15, 20, 25, 30, 35, 40, 50, 60, 70, 80, 90, 100 };
            return sizes;
        }
    }

    class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        readonly ResourceManager _resourceManager;
        readonly string _resourceKey;

        public LocalizedDisplayNameAttribute(string resourceKey, Type resourceType)
        {
            _resourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string DisplayName
        {
            get
            {
                string displayName = _resourceManager.GetString(_resourceKey);
                return string.IsNullOrWhiteSpace(displayName) ? string.Format("[[{0}]]", _resourceKey) : displayName;
            }
        }
    }

    class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        readonly ResourceManager _resourceManager;
        readonly string _resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            _resourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string description = _resourceManager.GetString(_resourceKey);
                return string.IsNullOrWhiteSpace(description) ? string.Format("[[{0}]]", _resourceKey) : description;
            }
        }
    }

    class LocalizedCategoryAttribute : CategoryAttribute
    {
        readonly ResourceManager _resourceManager;
        readonly string _resourceKey;

        public LocalizedCategoryAttribute(string resourceKey, Type resourceType)
        {
            _resourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        protected override string GetLocalizedString(string value)
        {
            string category = _resourceManager.GetString(_resourceKey);
            return string.IsNullOrWhiteSpace(category) ? string.Format("[[{0}]]", _resourceKey) : category;
        }
    }
}
