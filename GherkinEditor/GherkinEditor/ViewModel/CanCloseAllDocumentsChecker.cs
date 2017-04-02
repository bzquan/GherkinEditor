using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.View;

namespace Gherkin.ViewModel
{
    public class CanCloseAllDocumentsChecker : ICanCloseAllDocumentsChecker
    {
        private ObservableCollection<EditorTabItem> TabPanels;

        public CanCloseAllDocumentsChecker(ObservableCollection<EditorTabItem> tabPanels)
        {
            TabPanels = tabPanels;
        }

        public bool CanCloseAllDocuments()
        {
            return (TabPanels.Count > 1) || !TabPanels[0].EditorTabContentViewModel.IsEmptyFile();
        }

        public bool CanCloseAllButThis()
        {
            return (TabPanels.Count > 1);
        }
    }
}
