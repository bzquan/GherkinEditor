using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gherkin.View
{
    public class AutoScrollListBox : ListBox
    {
        public AutoScrollListBox() : base()
        {
            SelectionChanged += delegate { ScrollIntoView(SelectedItem); };
        }
    }
}
