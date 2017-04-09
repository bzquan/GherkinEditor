using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.ViewModel
{
    public class EditorLoadedArg : EventArgs
    {
        private EditorTabContentViewModel m_ViewModel;
        public EditorLoadedArg(EditorTabContentViewModel viewModel)
        {
            m_ViewModel = viewModel;
        }

        public EditorTabContentViewModel EditorTabContentViewModel
        {
            get { return m_ViewModel; }
        }
    }
}
