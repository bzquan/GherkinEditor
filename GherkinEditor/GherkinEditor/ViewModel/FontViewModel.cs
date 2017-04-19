using Gherkin.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Gherkin.ViewModel
{
    public class FontViewModel : NotifyPropertyChangedBase
    {
        private readonly string s_DefaultFontFamilyName = "KaiTi";
        private readonly string s_DefaultFontSize = "11";

        private readonly string s_DefaultFontFamilyName4NonGherkin = "Meiryo";
        private readonly string s_DefaultFontSize4NonGherkin = "10";

        private IAppSettings m_AppSettings;

        private ObservableCollection<string> m_FontSizes = new ObservableCollection<string> { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28" };
        private ObservableCollection<FontFamily> m_SystemFonts = new ObservableCollection<FontFamily>();

        public ICommand ResetFontCmd => new DelegateCommandNoArg(OnResetFont, CanResetFont);
        public ICommand ResetFont4NonGherkinCmd => new DelegateCommandNoArg(OnResetFont4NonGherkin, CanResetFont4NonGherkin);

        public FontViewModel(IAppSettings appSettings)
        {
            m_AppSettings = appSettings;
            LoadSystemFonts();
        }

        public ObservableCollection<FontFamily> SystemFonts => m_SystemFonts;
        public ObservableCollection<string> FontSizes => m_FontSizes;


        public FontFamily DefaultFontFamily
        {
            get { return new FontFamily(m_AppSettings.FontFamilyName); }
            set
            {
                string name = value.ToString();
                if (m_AppSettings.FontFamilyName != name)
                {
                    m_AppSettings.FontFamilyName = name;
                    base.OnPropertyChanged();
                }
            }
        }

        public string DefaultFontSize
        {
            get { return m_AppSettings.FontSize; }
            set
            {
                if (m_AppSettings.FontSize != value)
                {
                    m_AppSettings.FontSize = value;
                    base.OnPropertyChanged();
                }
            }
        }

        public FontFamily DefaultFontFamily4NonGherkin
        {
            get { return new FontFamily(m_AppSettings.FontFamilyName4NonGherkin); }
            set
            {
                string name = value.ToString();
                if (m_AppSettings.FontFamilyName4NonGherkin != name)
                {
                    m_AppSettings.FontFamilyName4NonGherkin = name;
                    base.OnPropertyChanged();
                }
            }
        }

        public string DefaultFontSize4NonGherkin
        {
            get { return m_AppSettings.FontSize4NonGherkin; }
            set
            {
                if (m_AppSettings.FontSize4NonGherkin != value)
                {
                    m_AppSettings.FontSize4NonGherkin = value;
                    base.OnPropertyChanged();
                }
            }
        }

        private void LoadSystemFonts()
        {
            m_SystemFonts.Clear();
            var fontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());
            foreach (var f in fontFamilies) m_SystemFonts.Add(f);
        }

        private void OnResetFont()
        {
            DefaultFontFamily = new FontFamily(s_DefaultFontFamilyName);
            DefaultFontSize = s_DefaultFontSize;
        }
        private bool CanResetFont()
        {
            return (DefaultFontFamily.ToString() != s_DefaultFontFamilyName) ||
                   (DefaultFontSize != s_DefaultFontSize);
        }

        private void OnResetFont4NonGherkin()
        {
            DefaultFontFamily4NonGherkin = new FontFamily(s_DefaultFontFamilyName4NonGherkin);
            DefaultFontSize4NonGherkin = s_DefaultFontSize4NonGherkin;
        }
        private bool CanResetFont4NonGherkin()
        {
            return (DefaultFontFamily4NonGherkin.ToString() != s_DefaultFontFamilyName4NonGherkin) ||
                   (DefaultFontSize4NonGherkin != s_DefaultFontSize4NonGherkin);
        }
    }
}
