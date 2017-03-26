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
        private ObservableCollection<string> m_FontSizes = new ObservableCollection<string> { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28" };
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
