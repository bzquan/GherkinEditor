using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gherkin.ViewModel
{
    public class FontViewModel : NotifyPropertyChangedBase
    {
        private ObservableCollection<string> m_FontSizes = new ObservableCollection<string> { "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28" };
        private ObservableCollection<FontFamily> m_SystemFonts = new ObservableCollection<FontFamily>();

        public FontViewModel()
        {
            LoadSystemFonts();
        }

        public ObservableCollection<FontFamily> SystemFonts => m_SystemFonts;
        public ObservableCollection<string> FontSizes => m_FontSizes;

        private void LoadSystemFonts()
        {
            m_SystemFonts.Clear();
            var fontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());
            foreach (var f in fontFamilies) m_SystemFonts.Add(f);
        }
    }
}
