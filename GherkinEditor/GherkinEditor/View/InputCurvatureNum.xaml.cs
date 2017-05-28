using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Gherkin.View
{
    /// <summary>
    /// Interaction logic for InputCurvatureNum.xaml
    /// </summary>
    public partial class InputCurvatureNum : Window
    {
        public List<string> CurvatureCounts { get; set; } = new List<string>() { Properties.Resources.InputCurvatureNum_SameAsInput, "100" };

        public InputCurvatureNum()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string InputValue { get; private set; }

        private void OnOKButton_Click(object sender, RoutedEventArgs e)
        {
            string input = this.comboBox.Text;
            if (string.IsNullOrEmpty(input) || !Util.Util.IsDigitsOnly(input))
            {
                InputValue = CurvatureCounts[0];
            }
            else
            {
                InputValue = input;
            }
            this.Close();
        }
    }
}
