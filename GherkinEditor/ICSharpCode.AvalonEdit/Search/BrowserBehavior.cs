using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ICSharpCode.AvalonEdit.Search
{
    /// <summary>
    /// HTML text to webbrower control
    /// </summary>
    public static class BrowserBehavior
    {
        /// <summary>
        /// Html property
        /// </summary>
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html",
            typeof(string),
            typeof(BrowserBehavior),
            new FrameworkPropertyMetadata(OnHtmlChanged));

        /// <summary>
        /// Get html
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        /// <summary>
        /// Set html
        /// </summary>
        /// <param name="d"></param>
        /// <param name="value"></param>
        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser wb = d as WebBrowser;
            if (wb != null)
                wb.NavigateToString(e.NewValue as string);
        }
    }
}
