using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gherkin.Util;

namespace Gherkin.Model
{
    public class ErrorMessageDrawingVisual : DrawingVisual
    {
        public ErrorMessageDrawingVisual(string errorMessage)
        {
            Build(errorMessage);
        }

        public BitmapImage ToBitmapImage()
        {
            return DrawingVisualUtil.ToPNGImage(this);
        }

        private void Build(string errorMessage)
        {
            using (DrawingContext dc = this.RenderOpen())
            {
                string text = string.IsNullOrWhiteSpace(errorMessage) ? "Error" : errorMessage;
                var formattedText = new FormattedText(text,
                                                      CultureInfo.CurrentCulture,
                                                      FlowDirection.LeftToRight,
                                                      new Typeface("Courier New"),
                                                      15, Brushes.Red);
                dc.DrawText(formattedText, new Point(0, 0));
            }
        }

    }
}
