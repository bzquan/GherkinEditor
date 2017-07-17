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
    /// Interaction logic for InputExpectedPointsNum.xaml
    /// </summary>
    public partial class InputExpectedPointsNum : Window
    {
        public List<string> ExpectedPointsNum { get; set; }

        public InputExpectedPointsNum(List<string> expectedPointsNum)
        {
            InitializeComponent();
            ExpectedPointsNum = expectedPointsNum;
            this.DataContext = this;
        }

        public string InputValue { get; private set; }

        private void OnOKButton_Click(object sender, RoutedEventArgs e)
        {
            string input = this.comboBox.Text;
            if (string.IsNullOrEmpty(input) || !Util.StringUtil.IsDigitsOnly(input))
            {
                InputValue = ExpectedPointsNum[0];
            }
            else
            {
                InputValue = input;
            }
            this.Close();
        }
    }
}
