using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gherkin.Util;
using Gherkin.ViewModel;

namespace Gherkin.View
{
    /// <summary>
    /// Interaction logic for FindReplacePopup.xaml
    /// </summary>
    public partial class FindReplacePopup : Popup
    {
        public FindReplacePopup()
        {
            InitializeComponent();

            SetupDraggablePopup();
        }

        /// <summary>
        /// Note: Drag WPF Popup control
        /// http://stackoverflow.com/questions/222029/drag-wpf-popup-control
        /// </summary>
        private void SetupDraggablePopup()
        {
            var dummy_thumb = new Thumb
            {
                Width = 0,
                Height = 0,
            };
            this.dummyCanvasForGragablePopup.Children.Add(dummy_thumb);

            MouseDown += (sender, e) =>
            {
                dummy_thumb.RaiseEvent(e);
            };

            dummy_thumb.DragDelta += (sender, e) =>
            {
                HorizontalOffset += e.HorizontalChange;
                VerticalOffset += e.VerticalChange;
            };
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            txtFindComboBox.Focus();
            txtFindComboBox2.Focus();
        }

        private void txtFindComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                EventAggregator<FindNextRequestedArg>.Instance.Publish(this, new FindNextRequestedArg());
                e.Handled = true;
            }
        }
    }
}
