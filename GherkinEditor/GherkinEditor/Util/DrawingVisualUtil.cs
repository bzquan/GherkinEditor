using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gherkin.Util
{
    public static class DrawingVisualUtil
    {
        public static BitmapSource ToBitmapSource(this Visual visual, Brush transparentBackground)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(visual);
            var bitmapSource = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(visual);
                drawingContext.DrawRectangle(transparentBackground, null, new Rect(new Point(), bounds.Size));
                drawingContext.DrawRectangle(visualBrush, null, new Rect(new Point(), bounds.Size));
            }
            bitmapSource.Render(drawingVisual);
            return bitmapSource;
        }

        public static BitmapSource ToBitmapSource(this Visual visual)
        {
            return ToBitmapSource(visual, Brushes.White);
        }

        public static void ToPngFile(this BitmapSource bitmapSource, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using (var file = File.Create(fileName)) encoder.Save(file);
        }

        public static BitmapImage ToPNGImage(this BitmapSource bitmapsource)
        {
            BitmapImage bitmapImage;
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapsource));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public static BitmapImage ToPNGImage(this Visual visual)
        {
            var bitmapsource = visual.ToBitmapSource();
            return bitmapsource.ToPNGImage();
        }
    }
}
