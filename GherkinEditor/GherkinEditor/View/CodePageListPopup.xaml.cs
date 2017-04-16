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

namespace Gherkin.View
{
    /// <summary>
    /// Interaction logic for CodePageListPopup.xaml
    /// Note: Drag WPF Popup control
    /// http://stackoverflow.com/questions/222029/drag-wpf-popup-control
    /// </summary>
    public partial class CodePageListPopup : Popup
    {
        public CodePageListPopup()
        {
            InitializeComponent();
            SetupDraggablePopup();
            this.Opened += delegate
                            {
                                ForceToUpdateSelectedIndexOfList();
                                codePageList.Focus();
                            };
        }

        private void ForceToUpdateSelectedIndexOfList()
        {
            int selectedIndex = codePageList.SelectedIndex;
            codePageList.SelectedIndex = 0;
            codePageList.SelectedIndex = selectedIndex;
        }

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
    }
}
