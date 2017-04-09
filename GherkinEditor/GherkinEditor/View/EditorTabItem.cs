using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using System.Windows;
using System.ComponentModel;
using Gherkin.Util;
using static Gherkin.Util.Util;
using System.Windows.Input;
using Gherkin.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace Gherkin.View
{
    public class EditorTabItem : BetterWpfControls.TabItem
    {
        private EditorTabHeaderViewModel EditorTabHeaderViewModel { get; set; }
        public EditorTabContentViewModel EditorTabContentViewModel { get; set; }

        public EditorTabItem(System.Windows.FrameworkElement parent, ICanCloseAllDocumentsChecker canCloseAllDocumentsChecker, string filePath, IAppSettings appSettings)
        {
            this.HeaderTemplate = parent.FindResource("tabItemHeader") as DataTemplate;
            EditorTabContentViewModel = new EditorTabContentViewModel(filePath, appSettings);
            base.Content = new EditorTabContent(parent, EditorTabContentViewModel);

            EditorTabHeaderViewModel = new EditorTabHeaderViewModel(EditorTabContentViewModel, canCloseAllDocumentsChecker);
            this.DataContext = EditorTabHeaderViewModel;
        }

        public string FileName => EditorTabHeaderViewModel.FileName;
        public string FilePath => EditorTabHeaderViewModel.FilePath;
    }
}
