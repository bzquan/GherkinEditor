using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gherkin.Util
{
    public static class Util
    {
        public static void SetLanguage(Languages language)
        {
            string shortDatePattern = "dd/MMM/yyyy";
            string longDatePattern = "dd/MMM/yyyy HH:mm";
            string locale = "ja-JP";
            switch (language)
            {
                case Languages.Chinese:
                    locale = "zh-CN";
                    shortDatePattern = "yyyy/MM/dd";
                    longDatePattern = "yyyy/MM/dd HH:mm";
                    break;
                case Languages.English:
                    locale = "en";
                    shortDatePattern = "dd/MMM/yyyy";
                    longDatePattern = "dd/MMM/yyyy HH:mm";
                    break;
                case Languages.Japanese:
                    locale = "ja-JP";
                    shortDatePattern = "yyyy/MM/dd";
                    longDatePattern = "yyyy/MM/dd HH:mm";
                    break;
            }

            var cultureInfo = new CultureInfo(locale);
            cultureInfo.DateTimeFormat.ShortDatePattern = shortDatePattern;
            cultureInfo.DateTimeFormat.LongDatePattern = longDatePattern;
            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public static bool ExistsOnPath(params string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (GetFullPath(fileName) == null) return false;
            }
            return true;
        }

        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(';'))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath)) return fullPath;
            }
            return null;
        }

        public static bool ExistOnFolder(string folder, params string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                string fullPath = Path.Combine(folder, fileName);
                if (!File.Exists(fullPath)) return false;
            }
            return true;
        }

        public static string StartupFolder() => System.AppDomain.CurrentDomain.BaseDirectory;

        public static bool ExistOnStartupFolder(params string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (!ExistOnFolder(StartupFolder(), fileName)) return false;
            }
            return true;
        }

        public static string TraceMessage(string message,
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerMemberName] string memberName = "",
                             [CallerLineNumber] int sourceLineNumber = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message)
              .AppendLine("[")
              .AppendLine("File: " + sourceFilePath)
              .AppendLine("Function: " + memberName)
              .AppendLine("Line: " + sourceLineNumber)
              .AppendLine("]");

            return sb.ToString();
        }

        /// <summary>
        /// Make an image from overlapped two images
        /// </summary>
        /// <param name="image_name1"></param>
        /// <param name="image_name2"></param>
        /// <returns></returns>
        public static DrawingImage DrawingImageByOverlapping(string image_name1, string image_name2)
        {
            var group = new DrawingGroup();
            var image1 = ImageFromResource(image_name1);
            group.Children.Add(new ImageDrawing(image1, new Rect(0, 0, image1.Width, image1.Height)));

            // draw image2 on top of image1, using same rect as image1
            var image2 = ImageFromResource(image_name2);
            group.Children.Add(new ImageDrawing(image2, new Rect(0, 0, image1.Width, image1.Height)));

            return new DrawingImage(group);
        }

        public static DrawingImage DrawingImageFromResource(string image_name)
        {
            var image = ImageFromResource(image_name);
            return new DrawingImage(new ImageDrawing(image, new Rect(0, 0, image.Width, image.Height)));
        }

        /// <summary>
        /// Resourceからイメージオブジェクトを取得する
        /// </summary>
        /// <param name="image_name"></param>
        /// <returns>イメージ</returns>
        public static BitmapImage ImageFromResource(string image_name)
        {
            string uri = PackImageURI(image_name);
            return (uri != null) ? new BitmapImage(new Uri(uri)) : null;
        }

        public static string PackImageURI(string image_name)
        {
            if (image_name != null)
                return "pack://application:,,,/View/Images/" + image_name;
            else
                return null;
        }

        /// <summary>
        /// Imageの上に文字を書いたImageを生成する
        /// </summary>
        /// <param name="image_name"></param>
        /// <param name="text"></param>
        /// <param name="font_color"></param>
        /// <returns></returns>
        public static DrawingImage ImageFromResource(string image_name, string text, Color font_color)
        {
            BitmapImage bitmapSource = ImageFromResource(image_name);
            FormattedText formattedText =
                new FormattedText(
                    text,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Comic Sans MS"),
                    bitmapSource.PixelWidth,
                    new SolidColorBrush(font_color));

            var visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                Rect rect = new Rect(0, 0, bitmapSource.PixelWidth, bitmapSource.PixelHeight);
                dc.DrawImage(bitmapSource, rect);
                Point position = new Point((rect.Width - formattedText.Width) / 2,
                                           (rect.Height - formattedText.Height) / 2);
                dc.DrawText(formattedText, position);
            }

            return new DrawingImage(visual.Drawing);
        }

        /// <summary>
        /// Usage sample :  obj.IfNotNull(x => statements);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public static void IfNotNull<T>(this T obj, Action<T> action) where T : class
        {
            if (obj != null) action(obj);
        }

        public static TR IfNotNull<T, TR>(this T obj, Func<T, TR> func) where T : class
        {
            return (obj != null) ? func(obj) : default(TR);
        }

        public static SolidColorBrush BrushFromColorName(string colorName)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(colorName);
        }

        public static int RoundUpByDivision(int total, int part)
        {
            if (part == 0) return 0;
            return (total + part - 1) / part;
        }

        public static void RestartApplication()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        public static string ToName(this System.Windows.Media.Color color)
        {
            Type colors = typeof(System.Windows.Media.Colors);
            foreach (var prop in colors.GetProperties())
            {
                if (((System.Windows.Media.Color)prop.GetValue(null, null)) == color)
                    return prop.Name;
            }

            return "Gray";
        }

        public static Color ToColor(this string name)
        {
            return (Color)ColorConverter.ConvertFromString(name);
        }
    }
}
