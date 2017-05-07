using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace Gherkin.Model
{
    /// <summary>
    /// Image Element Generators for AvalonEdit
    /// 
    /// To use this class:
    /// textEditor.TextArea.TextView.ElementGenerators.Add(new ImageElementGenerator());
    /// 
    /// Image must be specified by following mark down.
    /// mark down syntax: ![IMAGE](image path), e.g. ![IMAGE](Gherkin.png)
    /// BasePath is the directory of document.FileName
    /// 
    /// http://danielgrunwald.de/coding/AvalonEdit/rendering.php
    /// </summary>
    public class ImageElementGenerator : CustomElementGenerator
    {
        public static readonly string ImagePrefix = "![IMAGE";
        // mark down syntax: ![IMAGE](image path), e.g. ![IMAGE](Images/Gherkin.png)
        private readonly static Regex s_ImageRegex = new Regex(@"!\[IMAGE\s*([0-9]*)\]\(([^)]+)\)", RegexOptions.IgnoreCase);

        public ImageElementGenerator(TextDocument document) : base(document)
        {
        }

        protected override Regex GetRegex()
        {
            return s_ImageRegex;
        }

        /// Constructs an element at the specified offset.
        /// May return null if no element should be constructed.
        protected override InlineObjectElement ConstructMainElement(int offset)
        {
            Match m = FindMatch(offset);
            // check whether there's a match exactly at offset
            if (m.Success && m.Index == 0)
            {
                BitmapImage bitmap = LoadBitmap(m.Groups[2].Value);
                UIElement uiElement;
                if (bitmap != null)
                {
                    string scale = m.Groups[1].Value;
                    uiElement = CreateImageControl(scale, bitmap);
                }
                else
                {
                    uiElement = CreateErrorMesageTextBlock();
                }

                // Pass the length of the match to the 'documentLength' parameter of InlineObjectElement.
                return new InlineObjectElement(m.Length, uiElement);
            }
            return null;
        }

        private BitmapImage LoadBitmap(string fileName)
        {
            if (BasePath != null)
            {
                string fullFileName = Path.Combine(BasePath, fileName);
                return BitmapImageCache.Instance.LoadImage(fullFileName);
            }

            return null;
        }

        private static Image CreateImageControl(string scale, BitmapImage bitmap)
        {
            Image image = new Image();
            image.Source = bitmap;

            double zoom = CalcZoom(bitmap, scale);
            image.Width = bitmap.PixelWidth * zoom;
            image.Height = bitmap.PixelHeight * zoom;
            image.Cursor = Cursors.Arrow;

            return image;
        }

        private static double CalcZoom(BitmapImage bitmap, string scale)
        {
            double scale_value = ImageScale(scale);
            double width = Math.Min(1024, bitmap.PixelWidth * scale_value);
            double height = Math.Min(1024, bitmap.PixelHeight * scale_value);
            double zoomX = width / (double)bitmap.PixelWidth;
            double zoomY = height / (double)bitmap.PixelHeight;

            return Math.Min(zoomX, zoomY);
        }

        /// <summary>
        /// Scale would be between 0.1 and 5.0
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        private static double ImageScale(string scale)
        {
            double scale_value = 1.0;
            if (!string.IsNullOrWhiteSpace(scale))
            {
                scale_value = int.Parse(scale) / 100.0;
            }

            scale_value = Math.Max(0.1, scale_value);
            scale_value = Math.Min(5.0, scale_value);

            return scale_value;
        }

        private TextBlock CreateErrorMesageTextBlock()
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = (BasePath == null) ? "File not saved" : "Image file not found",
                Foreground = new SolidColorBrush(Colors.Red),
                Cursor = Cursors.Arrow
            };

            return textBlock;
        }

        private string BasePath
        {
            get
            {
                string filePath = Document.FileName;
                if (File.Exists(filePath))
                    return Path.GetDirectoryName(filePath);
                else
                    return null;
            }
        }
    }
}
