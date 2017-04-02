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

namespace Gherkin.View
{
    public class EditorTabItem : TabItem
    {
        private EditorTabHeaderViewModel EditorTabHeaderViewModel { get; set; }
        public EditorTabContentViewModel EditorTabContentViewModel { get; set; }

        public EditorTabItem(System.Windows.FrameworkElement parent, ObservableCollection<EditorTabItem> tabPanels, string filePath, IAppSettings appSettings)
        {
            this.HeaderTemplate = parent.FindResource("tabItemHeader") as DataTemplate;

            EditorTabContentViewModel = new EditorTabContentViewModel(filePath, appSettings);
            base.Content = new EditorTabContent(parent, EditorTabContentViewModel);

            EditorTabHeaderViewModel = new EditorTabHeaderViewModel(EditorTabContentViewModel, tabPanels);
            this.DataContext = EditorTabHeaderViewModel;

            base.Loaded += OnEditorTabLoaded;
        }

        public void SetMaxWidth(double width)
        {
            EditorTabHeaderViewModel.MaxWidthOfFileNameText = width;
        }

        private void OnEditorTabLoaded(object sender, RoutedEventArgs e)
        {
            EventAggregator<AdjustMaxWidthOfEditorTabArg>.Instance.Publish(this, new AdjustMaxWidthOfEditorTabArg());
        }
    }
}
