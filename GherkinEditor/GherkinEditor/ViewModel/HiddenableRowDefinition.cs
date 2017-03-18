using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Gherkin.ViewModel
{
    public class HiddenableRowDefinition : RowDefinition
    {
        private static GridLength s_ZeroGridLength = new GridLength(0);
        private GridLength _height;

        public bool IsHidden
        {
            get { return (bool)GetValue(IsHiddenProperty); }
            set { SetValue(IsHiddenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHidden.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHiddenProperty =
            DependencyProperty.Register("IsHidden", typeof(bool), typeof(HiddenableRowDefinition), new PropertyMetadata(false, Changed));

        public static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var o = d as HiddenableRowDefinition;
            o.Toggle((bool)e.NewValue);
        }

        private void Toggle(bool isHidden)
        {
            if (isHidden)
            {
                _height = this.Height;
                this.Height = s_ZeroGridLength;
            }
            else
                this.Height = _height;
        }
    }
}
