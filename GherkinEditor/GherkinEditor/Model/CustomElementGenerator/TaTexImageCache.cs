using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfMath;

namespace Gherkin.Model
{
    class LaTexBitmapImage
    {
        public string Key { get; set; }
        public BitmapImage BitmapImage { get; set; }
    }

    class TaTexImageCache
    {
        private static readonly Lazy<TaTexImageCache> s_Singleton =
            new Lazy<TaTexImageCache>(() => new TaTexImageCache());

        private List<LaTexBitmapImage> m_BitmapImages = new List<LaTexBitmapImage>();

        public static TaTexImageCache Instance => s_Singleton.Value;

        private TaTexImageCache() { }

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
                    Util.Util.RemoveLastItems(m_BitmapImages, max_num: 50);
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

        private static string s_LastExceptionMessage;

        public BitmapImage ParseLaTex()
        {
            BitmapImage bitmapImage = null;
            try
            {
                var formula = s_FormulaParser.Value.Parse(m_LaTex);
                var renderer = formula.GetRenderer(TexStyle.Display, m_scale);
                var bitmapsource = renderer.RenderToBitmap(0, 0);

                bitmapImage = ToPNGImage(bitmapsource);
            }
            catch (Exception ex)
            {
                if (ex.Message != s_LastExceptionMessage)
                {
                    s_LastExceptionMessage = ex.Message;
                    Util.EventAggregator<Util.StatusChangedArg>.Instance.Publish(this, new Util.StatusChangedArg(ex.Message));
                }
                bitmapImage = null;
            }

            return bitmapImage;
        }

        private BitmapImage ToPNGImage(BitmapSource bitmapsource)
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
    }
}
