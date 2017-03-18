using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.ViewModel
{
    public class ScenarioIndex : NotifyPropertyChangedBase
    {
        private string m_Title;
        public int Offset { get; set; }
        public string Title
        {
            get { return m_Title; }
            set
            {
                string title = value.Trim();
                if (title != m_Title)
                {
                    m_Title = title;
                    base.OnPropertyChanged();
                }
            }
        }
    }
}
