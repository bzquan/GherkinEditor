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

            Model.CacheBase.CacheSize = appSettings.ImageCacheSize;
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
            get { return m_AppSettings.EditorSettings.SupportUnicode; }
            set
            {
                m_AppSettings.EditorSettings.SupportUnicode = value;
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
            get { return m_AppSettings.EditorSettings.HighlightCurrentLine; }
            set
            {
                m_AppSettings.EditorSettings.HighlightCurrentLine = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.HighlightCurrentLine = value);
                base.OnPropertyChanged();
            }
        }

        [Category("Current Line")]
        [LocalizedDisplayName("MenuSetting_ShowCurrentLineBorder", typeof(Resources))]
        public bool ShowCurrentLineBorder
        {
            get { return m_AppSettings.EditorSettings.ShowCurrentLineBorder; }
            set
            {
                m_AppSettings.EditorSettings.ShowCurrentLineBorder = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.ShowCurrentLineBorder = value);
                base.OnPropertyChanged();
            }
        }

        [Category("Open hyper link")]
        [LocalizedDisplayName("MenuSetting_RequireControlModifierForHyperlinkClick", typeof(Resources))]
        [LocalizedDescription("Tooltip_RequireControlModifierForHyperlinkClick", typeof(Resources))]
        public bool RequireControlModifierForHyperlinkClick
        {
            get { return m_AppSettings.EditorSettings.RequireControlModifierForHyperlinkClick; }
            set
            {
                m_AppSettings.EditorSettings.RequireControlModifierForHyperlinkClick = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateRequireControlModifierForHyperlinkClick());
                base.OnPropertyChanged();
            }
        }

        [Category("Column ruler")]
        [LocalizedDisplayName("MenuSetting_ShowColumnRuler", typeof(Resources))]
        public bool ShowColumnRuler
        {
            get { return m_AppSettings.EditorSettings.ShowColumnRuler; }
            set
            {
                m_AppSettings.EditorSettings.ShowColumnRuler = value;
                UpdateColumnRuler();
                base.OnPropertyChanged();
            }
        } 

        [Category("Column ruler")]
        [LocalizedDisplayName("MenuSetting_ColumnRulerPosition", typeof(Resources))]
        public int ColumnRulerPositon
        {
            get { return m_AppSettings.EditorSettings.ColumnRulerPositon; }
            set
            {
                int v = Math.Max(60, value);
                int pos = Math.Min(300, v);
                m_AppSettings.EditorSettings.ColumnRulerPositon = pos;
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
            get { return GetFontFamily(m_AppSettings.Fonts.FontFamilyName); }
            set
            {
                m_AppSettings.Fonts.FontFamilyName = value.ToString();
                base.OnPropertyChanged();
            }
        }

        [Category("Font")]
        [LocalizedDisplayName("Tooltip_FontSizeForGherkin", typeof(Resources))]
        public int FontSize4Gherkin
        {
            get { return int.Parse(m_AppSettings.Fonts.FontSize); }
            set
            {
                if ((value >= 10) && (value <= 28))
                {
                    m_AppSettings.Fonts.FontSize = value.ToString();
                }
                base.OnPropertyChanged();
            }
        }

        [Category("Font")]
        [LocalizedDisplayName("Tooltip_FontForNonGherkin", typeof(Resources))]
        [ItemsSource(typeof(FontFamilyItemsSource))]
        public FontFamily FontFamily4NonGherkin
        {
            get { return GetFontFamily(m_AppSettings.Fonts.FontFamilyName4NonGherkin); }
            set
            {
                m_AppSettings.Fonts.FontFamilyName4NonGherkin = value.ToString();
                base.OnPropertyChanged();
            }
        }

        [Category("Font")]
        [LocalizedDisplayName("Tooltip_FontSizeForNonGherkin", typeof(Resources))]
        public int FontSize4NonGherkin
        {
            get { return int.Parse(m_AppSettings.Fonts.FontSize4NonGherkin); }
            set
            {
                if ((value >= 10) && (value <= 28))
                {
                    m_AppSettings.Fonts.FontSize4NonGherkin = value.ToString();
                }
                base.OnPropertyChanged();
            }
        }

        [Category("Font")]
        [DisplayName("Font name for Graphviz unicode")]
        [ItemsSource(typeof(FontFamilyItemsSource))]
        public FontFamily FontFamilyName4GraphvizUnicode
        {
            get { return GetFontFamily(m_AppSettings.Fonts.FontFamilyName4GraphvizUnicode); }
            set
            {
                var fontname = value.ToString();
                m_AppSettings.Fonts.FontFamilyName4GraphvizUnicode = fontname;

                EventAggregator<GraphvizFontnameChangedArg>.Instance.Publish(this, new GraphvizFontnameChangedArg(fontname));
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_KeywordColor", typeof(Resources))]
        public Color HighlightingKeywordColor
        {
            get { return m_AppSettings.Colors.ColorOfHighlightingKeyword.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.Colors.ColorOfHighlightingKeyword), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_StepWordColor", typeof(Resources))]
        public Color HighlightingStepWordColor
        {
            get { return m_AppSettings.Colors.ColorOfHighlightingStepWord.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.Colors.ColorOfHighlightingStepWord), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_TableColor", typeof(Resources))]
        public Color HighlightingTableColor
        {
            get { return m_AppSettings.Colors.ColorOfHighlightingTable.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.Colors.ColorOfHighlightingTable), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_TagColor", typeof(Resources))]
        public Color HighlightingTagColor
        {
            get { return m_AppSettings.Colors.ColorOfHighlightingTag.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.Colors.ColorOfHighlightingTag), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_DocStringColor", typeof(Resources))]
        public Color HighlightingDocStringColor
        {
            get { return m_AppSettings.Colors.ColorOfHighlightingDocString.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.Colors.ColorOfHighlightingDocString), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_ConstantsColor", typeof(Resources))]
        public Color HighlightingConstantsColor
        {
            get { return m_AppSettings.Colors.ColorOfHighlightingConstants.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.Colors.ColorOfHighlightingConstants), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinHighlighting", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_MockColor", typeof(Resources))]
        public Color HighlightingMockAttributeColor
        {
            get { return m_AppSettings.Colors.ColorOfHighlightingMockAttribute.ToColor(); }
            set
            {
                SetHighlightingColor(nameof(m_AppSettings.Colors.ColorOfHighlightingMockAttribute), value);
            }
        }

        [LocalizedCategory("MenuSetting_GherkinFolding", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_DefaultTableFolding", typeof(Resources))]
        public bool IsCloseTablesFoldingByDefault
        {
            get { return m_AppSettings.EditorSettings.IsCloseTablesFoldingByDefault; }
            set
            {
                m_AppSettings.EditorSettings.IsCloseTablesFoldingByDefault = value;
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_GherkinFolding", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_DefaultScenarioFolding", typeof(Resources))]
        public bool IsCloseScenarioFoldingByDefault
        {
            get { return m_AppSettings.EditorSettings.IsCloseScenarioFoldingByDefault; }
            set
            {
                m_AppSettings.EditorSettings.IsCloseScenarioFoldingByDefault = value;
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_GherkinFolding", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_FoldTextColor", typeof(Resources))]
        public Color FoldingTextColor
        {
            get { return m_AppSettings.Colors.ColorOfFoldingText.ToColor(); }
            set
            {
                ChangeFoldingTextColorOfAvalonEdit(value);
                m_AppSettings.Colors.ColorOfFoldingText = value.ToName();
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
            get { return m_AppSettings.EditorSettings.ShowScenarioIndexByDefault; }
            set
            {
                m_AppSettings.EditorSettings.ShowScenarioIndexByDefault = value;
                base.OnPropertyChanged();
            }
        }
        
        [Category("Editor window")]
        [LocalizedDisplayName("MenuSetting_SplitHorizontalEditorByDefault", typeof(Resources))]
        public bool ShowSplitHorizontalViewByDefault
        {
            get { return m_AppSettings.EditorSettings.ShowSplitHorizontalViewByDefault; }
            set
            {
                m_AppSettings.EditorSettings.ShowSplitHorizontalViewByDefault = value;
                base.OnPropertyChanged();
            }
        }

        [Category("Editor window")]
        [LocalizedDisplayName("MenuSetting_SplitVerticalEditorByDefault", typeof(Resources))]
        public bool ShowSplitVertiViewByDefault
        {
            get { return m_AppSettings.EditorSettings.ShowSplitVerticalViewByDefault; }
            set
            {
                m_AppSettings.EditorSettings.ShowSplitVerticalViewByDefault = value;
                base.OnPropertyChanged();
            }
        }

        [Category("Editor window")]
        [DisplayName("Word wrap")]
        public bool WordWrap
        {
            get { return m_AppSettings.EditorSettings.WordWrap; }
            set
            {
                m_AppSettings.EditorSettings.WordWrap = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateWordWrap());
                base.OnPropertyChanged();
            }
        }

        [Category("Tab")]
        [LocalizedDisplayName("MenuSetting_ConvertTabsToSpaces", typeof(Resources))]
        public bool ConvertTabsToSpaces
        {
            get { return m_AppSettings.EditorSettings.ConvertTabsToSpaces; }
            set
            {
                m_AppSettings.EditorSettings.ConvertTabsToSpaces = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateConvertTabsToSpaces());
                base.OnPropertyChanged();
            }
        }

        [Category("Tab")]
        [LocalizedDisplayName("MenuSetting_ConvertTabsToSpaces", typeof(Resources))]
        [ItemsSource(typeof(IndentationSizeItemsSource))]
        public int IndentionSize
        {
            get { return m_AppSettings.EditorSettings.IndentationSize; }
            set
            {
                m_AppSettings.EditorSettings.IndentationSize = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateIndentationSize());
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_SpecialSymbols", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_ShowEndOfLine", typeof(Resources))]
        public bool ShowEndOfLine
        {
            get { return m_AppSettings.EditorSettings.ShowEndOfLine; }
            set
            {
                m_AppSettings.EditorSettings.ShowEndOfLine = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateShowEndOfLine());
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_SpecialSymbols", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_ShowSpaces", typeof(Resources))]
        public bool ShowSpaces
        {
            get { return m_AppSettings.EditorSettings.ShowSpaces; }
            set
            {
                m_AppSettings.EditorSettings.ShowSpaces = value;
                m_TabPanels.ForEach(tab => tab.EditorTabContentViewModel.UpdateShowSpaces());
                base.OnPropertyChanged();
            }
        }

        [LocalizedCategory("MenuSetting_SpecialSymbols", typeof(Resources))]
        [LocalizedDisplayName("MenuSetting_ShowTabs", typeof(Resources))]
        public bool ShowTabs
        {
            get { return m_AppSettings.EditorSettings.ShowTabs; }
            set
            {
                m_AppSettings.EditorSettings.ShowTabs = value;
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
                    Model.CacheBase.CacheSize = value;
                }
                base.OnPropertyChanged();
            }
        }

        [LocalizedDisplayName("MenuView_UseNativeDocumentApplication", typeof(Resources))]
        [LocalizedDescription("Tooltip_UseNativeDocumentApplication", typeof(Resources))]
        public bool OpenDocumentByNativeApplication
        {
            get { return m_AppSettings.OpenDocumentByNativeApplication; }
            set
            {
                m_AppSettings.OpenDocumentByNativeApplication = value;
                base.OnPropertyChanged();
            }
        }

        [Category("Curve Maneuver Parameter")]
        [DisplayName("1.Curvature unit")]
        [ItemsSource(typeof(CurvatureUnitItemsSource))]
        public string CurvatureUnit
        {
            get
            {
                if (m_AppSettings.CurveManeuverParameter.Unit == Util.CurvatureUnit.Meter)
                    return "1/m";
                else
                    return "1/cm";
            }
            set
            {
                if (value.Equals("1/m", StringComparison.InvariantCultureIgnoreCase))
                    m_AppSettings.CurveManeuverParameter.Unit = Util.CurvatureUnit.Meter;
                else
                    m_AppSettings.CurveManeuverParameter.Unit = Util.CurvatureUnit.Centimeter;

                base.OnPropertyChanged();
                PublishCurveManeuverParameterChangedEvent();
            }
        }

        [Category("Curve Maneuver Parameter")]
        [DisplayName("2.Curve minimum distance")]
        public double CurveMinDistance
        {
            get { return m_AppSettings.CurveManeuverParameter.CurveMinDistance; }
            set
            {
                m_AppSettings.CurveManeuverParameter.CurveMinDistance = value;
                base.OnPropertyChanged();
                PublishCurveManeuverParameterChangedEvent();
            }
        }

        [Category("Curve Maneuver Parameter")]
        [DisplayName("3.Douglas-Peucker tolerance")]
        public double DouglasPeuckerTolerance
        {
            get { return m_AppSettings.CurveManeuverParameter.DouglasPeuckerTolerance; }
            set
            {
                m_AppSettings.CurveManeuverParameter.DouglasPeuckerTolerance = value;
                base.OnPropertyChanged();
                PublishCurveManeuverParameterChangedEvent();
            }
        }

        [Category("Curve Maneuver Parameter")]
        [DisplayName("4.Curve curvature threshold")]
        public double CurveCurvatureThreshold
        {
            get { return m_AppSettings.CurveManeuverParameter.CurveCurvatureThreshold; }
            set
            {
                m_AppSettings.CurveManeuverParameter.CurveCurvatureThreshold = value;
                base.OnPropertyChanged();
                PublishCurveManeuverParameterChangedEvent();
            }
        }

        [Category("Curve Maneuver Parameter")]
        [DisplayName("5.YTolerance")]
        public double YTolerance
        {
            get { return m_AppSettings.CurveManeuverParameter.YTolerance; }
            set
            {
                m_AppSettings.CurveManeuverParameter.YTolerance = value;
                base.OnPropertyChanged();
                PublishCurveManeuverParameterChangedEvent();
            }
        }

        [Category("Curve Maneuver Parameter")]
        [DisplayName("6.Thin out By Douglas-Peucker")]
        public bool ThinoutByDouglasPeuckerN
        {
            get { return m_AppSettings.CurveManeuverParameter.ThinoutByDouglasPeuckerN; }
            set
            {
                m_AppSettings.CurveManeuverParameter.ThinoutByDouglasPeuckerN = value;
                base.OnPropertyChanged();
                PublishCurveManeuverParameterChangedEvent();
            }
        }

        [Category("Curve Maneuver Parameter")]
        [DisplayName("7.NURBS Degree")]
        public int NURBSDegree
        {
            get { return m_AppSettings.CurveManeuverParameter.NURBSDegree; }
            set
            {
                m_AppSettings.CurveManeuverParameter.NURBSDegree = value;
                base.OnPropertyChanged();
                PublishCurveManeuverParameterChangedEvent();
            }
        }

        private void PublishCurveManeuverParameterChangedEvent()
        {
            var arg = new CurveManeuverParameterArg(m_AppSettings.CurveManeuverParameter);
            EventAggregator<CurveManeuverParameterArg>.Instance.Publish(this, arg);
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
            s_SystemFonts = System.Windows.Media.Fonts.SystemFontFamilies.OrderBy(f => f.ToString()).ToList();
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

    class CurvatureUnitItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection() { "1/m", "1/cm" };
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
