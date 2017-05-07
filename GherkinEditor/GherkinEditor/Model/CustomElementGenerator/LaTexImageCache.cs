using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfMath;
using Gherkin.Util;

namespace Gherkin.Model
{
    class LaTexBitmapImage
    {
        public string Key { get; set; }
        public BitmapImage BitmapImage { get; set; }
    }

    class LaTexImageCache
    {
        private static readonly Lazy<LaTexImageCache> s_Singleton =
            new Lazy<LaTexImageCache>(() => new LaTexImageCache());

        private List<LaTexBitmapImage> m_BitmapImages = new List<LaTexBitmapImage>();

        public static LaTexImageCache Instance => s_Singleton.Value;
        public static int CacheSizee { get; set; } = 50;

        private LaTexImageCache() { }

        public BitmapImage LoadImage(string laTex, string scale)
        {
            string key = scale + "_" + laTex;
            LaTexBitmapImage laTexBitmap = m_BitmapImages.FirstOrDefault(x => x.Key == key);
            if (laTexBitmap != null)
            {
                m_BitmapImages.Remove(laTexBitmap);
                m_BitmapImages.Insert(0, laTexBitmap);
            }
            else
            {
                int scale_value = FormulaScale(scale);
                var laTexImage = new LaTexParser(laTex, scale_value);
                var bitmapImage = laTexImage.ParseLaTex();
                if (bitmapImage != null)
                {
                    laTexBitmap = new LaTexBitmapImage() { Key = key, BitmapImage = bitmapImage };
                    m_BitmapImages.Insert(0, laTexBitmap);
                    Util.Util.RemoveLastItems(m_BitmapImages, max_num: CacheSizee);
                }
            }

            return laTexBitmap?.BitmapImage;
        }

        /// <summary>
        /// Convert [0-9]* string to mathematical formula scale.
        /// The scale is rounded between 10 to 100 and the default
        /// scale is 20 if scale string is empty
        /// </summary>
        /// <param name="scale">[0-9]* string</param>
        /// <returns>value between 10 -100</returns>
        private int FormulaScale(string scale)
        {
            int scale_value = 20;
            if (!string.IsNullOrWhiteSpace(scale))
            {
                scale_value = int.Parse(scale);
            }

            scale_value = Math.Max(10, scale_value);
            scale_value = Math.Min(100, scale_value);

            return scale_value;
        }
    }

    class LaTexParser
    {
        private readonly static Lazy<TexFormulaParser> s_FormulaParser =
                                new Lazy<TexFormulaParser>(() => new TexFormulaParser());

        private string m_LaTex;
        private double m_scale;

        public LaTexParser(string laTex, double scale)
        {
            m_LaTex = laTex;
            m_scale = scale;
        }

        public BitmapImage ParseLaTex()
        {
            try
            {
                var formula = s_FormulaParser.Value.Parse(m_LaTex);
                var renderer = formula.GetRenderer(TexStyle.Display, m_scale);
                var bitmapsource = renderer.RenderToBitmap(0, 0);

                return bitmapsource.ToPNGImage();
            }
            catch (Exception ex)
            {
                var err = new ErrorMessageDrawingVisual(ex.Message);
                return DrawingVisualUtil.ToPNGImage(err);
            }
        }
    }
}
