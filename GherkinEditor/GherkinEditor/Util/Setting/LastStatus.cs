using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util
{
    public class LastStatus
    {
        public string LastSelectedFile { get; set; } = "";
        public string LastFolderToCopyFile { get; set; } = "";

        public List<string> LastSearchedTexts { get; set; } = new List<string>();
        public List<string> LastGreppedTexts { get; set; } = new List<string>();
        public List<string> LastFileExtensions { get; set; } = new List<string>();
        public List<string> LastGreppedFolders { get; set; } = new List<string>();

        public bool IsCaseSensitiveInFind { get; set; }
        public bool IsMatchWholeWordInFind { get; set; }
        public bool IsUseRegexInFind { get; set; }
        public bool IsUseWildcardsInFind { get; set; }

        public string LastUsedFileExtension
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                var list = LastFileExtensions;
                InsertFirst(list, value);
                LastFileExtensions = list;
            }
        }

        public string LastSearchedText
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                var list = LastSearchedTexts;
                InsertFirst(list, value);
                LastSearchedTexts = list;
            }
        }

        public string LastGreppedText
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                var list = LastGreppedTexts;
                InsertFirst(list, value);
                LastGreppedTexts = list;
            }
        }

        public string LastGreppedFolder
        {
            get
            {
                List<string> folders = LastGreppedFolders;
                return folders.Count > 0 ? folders[0] : null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                var list = LastGreppedFolders;
                InsertFirst(list, value);
                LastGreppedFolders = list;
            }
        }

        /// <summary>
        /// Insert item at the front of list.
        /// And limit length of list to 20;
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        private void InsertFirst(List<string> list, string item)
        {
            list.RemoveAll(x => x == item);
            list.Insert(0, item);
            var remove_count = list.Count - 20;
            if (remove_count > 0)
            {
                // remove that number of items from the start of the list
                list.RemoveRange(20, remove_count);
            }
        }
    }
}
