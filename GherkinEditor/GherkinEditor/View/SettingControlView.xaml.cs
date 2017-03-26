using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for SettingControlView.xaml
    /// </summary>
    public partial class SettingControlView : UserControl
    {
        public SettingControlView()
        {
            InitializeComponent();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.ChangedButton == MouseButton.Left)
            {
                Window parent = Parent as Window;
                if (parent != null)
                {
                    parent.DragMove();
                }
            }
        }
    }
}
