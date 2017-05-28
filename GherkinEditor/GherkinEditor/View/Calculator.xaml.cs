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
using System.Windows.Shapes;
using Gherkin.Util;

namespace Gherkin.View
{
    /// <summary>
    /// Calculator by Extended WPF Toolkit
    /// </summary>
    public partial class Calculator : Window
    {
        public Calculator()
        {
            InitializeComponent();
        }

        private void Copy(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, this.calculator.DisplayText);
            e.Handled = true;
        }

        private void CanCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(this.calculator.DisplayText);
            e.Handled = true;
        }


        private void Paste(object sender, ExecutedRoutedEventArgs e)
        {
            var text = Clipboard.GetData(DataFormats.Text) as string;
            decimal value;
            if (Decimal.TryParse(text, out value))
            {
                this.calculator.Value = value;
            }
            e.Handled = true;
        }

        private void CanPaste(object sender, CanExecuteRoutedEventArgs e)
        {
            var text = Clipboard.GetData(DataFormats.Text) as string;
            decimal value;
            e.CanExecute = Decimal.TryParse(text, out value);
            e.Handled = true;
        }
    }
}
