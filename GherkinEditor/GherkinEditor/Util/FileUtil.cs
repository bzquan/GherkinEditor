using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gherkin.Util
{
    public static class FileUtil
    {
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

        public static string TempImageFolder
        {
            get
            {
                string path = Path.Combine(StartupFolder(), "TempImages");
                return path;
            }
        }

        public static string ToFileName(string text)
        {
            string fileName = text;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }

        public static void CreateFolder(string subFolder)
        {
            string path = Path.Combine(StartupFolder(), subFolder);
            Directory.CreateDirectory(path);

            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // do thing
                }
            }
        }
    }
}
