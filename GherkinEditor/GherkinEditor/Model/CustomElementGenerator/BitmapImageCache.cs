using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Gherkin.Model
{
    public class BitmapImageCache
    {
        private static readonly Lazy<BitmapImageCache> s_Singleton =
            new Lazy<BitmapImageCache>(() => new BitmapImageCache());

        private List<BitmapImageFile> m_BitmapImages = new List<BitmapImageFile>();

        public static BitmapImageCache Instance => s_Singleton.Value;

        public static int CacheSizee { get; set; } = 50;

        private BitmapImageCache() { }

        public BitmapImage LoadImage(string filePath)
        {
            BitmapImageFile bitmapFile = m_BitmapImages.FirstOrDefault(x => x.FilePath == filePath);
            if (bitmapFile != null)
                m_BitmapImages.Remove(bitmapFile);
            else
                bitmapFile = new BitmapImageFile(filePath);

            m_BitmapImages.Insert(0, bitmapFile);
            Util.Util.RemoveLastItems(m_BitmapImages, max_num: CacheSizee);

            return bitmapFile.LoadImage();
        }
    }

    class BitmapImageFile
    {
        private BitmapImage m_BitmapImage;
        private long m_FileModificationTicks;

        public BitmapImageFile(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; private set; }

        public BitmapImage LoadImage()
        {

            if (File.Exists(FilePath))
                LoadImageFromFile();
            else
                m_BitmapImage = null;

            return m_BitmapImage;
        }

        private void LoadImageFromFile()
        {
            try
            {
                DateTime modification = File.GetLastWriteTime(FilePath);
                if ((m_BitmapImage == null) || (modification.Ticks > m_FileModificationTicks))
                {
                    m_BitmapImage = new BitmapImage();
                    m_BitmapImage.BeginInit();
                    m_BitmapImage.UriSource = new Uri(FilePath);
                    m_BitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    m_BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    m_BitmapImage.EndInit();
                    m_FileModificationTicks = modification.Ticks;
                }
                else
                {
                    m_BitmapImage.CreateOptions = BitmapCreateOptions.None;
                }
            }
            catch (Exception)
            {
                m_BitmapImage = null;
            }
        }
    }
}
