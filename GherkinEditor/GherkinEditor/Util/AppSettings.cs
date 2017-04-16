using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace Gherkin.Util
{
    public class AppSettings : ApplicationSettingsBase, IAppSettings
    {
        [UserScopedSetting()]
        public Languages Language
        {
            get
            {
                var lang = this["Language"];
                return (lang != null) ? (Languages)lang : GetLanguageFromCultureInfo();
            }
            set { this["Language"] = value; }
        }

        private Languages GetLanguageFromCultureInfo()
        {
            CultureInfo ci = CultureInfo.InstalledUICulture;
            switch (ci.Name)
            {
                case "ja-JP":
                    return Languages.Japanese;
                case "zh-CN":
                    return Languages.Chinese;
                default:
                    return Languages.English;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("KaiTi")]
        public string FontFamilyName
        {
            get { return (string)this["FontFamilyName"]; }
            set { this["FontFamilyName"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("11")]
        public string FontSize
        {
            get { return (string)this["FontSize"]; }
            set { this["FontSize"] = value; }
        }

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public List<GherkinFileInfo> RecentFilesInfo
        {
            get
            {
                List<GherkinFileInfo> files = (List<GherkinFileInfo>)this["RecentFilesInfo"];
                files.RemoveAll(f => !File.Exists(f.FilePath));
                RecentFilesInfo = files;

                return files;
            }
            private set { this["RecentFilesInfo"] = value; }
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

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public List<string> LastSearchedTexts
        {
            get { return (List<string>)this["LastSearchedTexts"]; }
            set { this["LastSearchedTexts"] = value; }
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

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public List<string> LastGreppedTexts
        {
            get { return (List<string>)this["LastGreppedTexts"]; }
            set { this["LastGreppedTexts"] = value; }
        }

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

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public List<string> LastFileExtensions
        {
            get { return (List<string>)this["LastFileExtensions"]; }
            set { this["LastFileExtensions"] = value; }
        }

        public string LastGreppedFolder
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                var list = LastGreppedFolders;
                InsertFirst(list, value);
                LastGreppedFolders = list;
            }
        }

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public List<string> LastGreppedFolders
        {
            get { return (List<string>)this["LastGreppedFolders"]; }
            set { this["LastGreppedFolders"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool IsCaseSensitiveInFind
        {
            get { return (bool)this["IsCaseSensitiveInFind"]; }
            set { this["IsCaseSensitiveInFind"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool IsMatchWholeWordInFind
        {
            get { return (bool)this["IsMatchWholeWordInFind"]; }
            set { this["IsMatchWholeWordInFind"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool IsUseRegexInFind
        {
            get { return (bool)this["IsUseRegexInFind"]; }
            set { this["IsUseRegexInFind"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool IsUseWildcardsInFind
        {
            get { return (bool)this["IsUseWildcardsInFind"]; }
            set { this["IsUseWildcardsInFind"] = value; }
        }

        public void UpdateFontFamilyName(string filePath, string fontFamilyName)
        {
            if (fontFamilyName == null) return;

            var fileInfo = GetExistingFileInfo(filePath);
            if ((fileInfo != null) && (fileInfo.FontFamilyName != fontFamilyName))
            {
                fileInfo.FontFamilyName = fontFamilyName;
            }
        }

        public void UpdateFontSize(string filePath, string fontSize)
        {
            if (fontSize == null) return;

            var fileInfo = GetExistingFileInfo(filePath);
            if ((fileInfo != null) && (fileInfo.FontSize != fontSize))
            {
                fileInfo.FontSize = fontSize;
                FontSize = fontSize;
            }
        }

        public void UpdateCursorPos(string filePath, int line, int column)
        {
            var fileInfo = GetExistingFileInfo(filePath);
            if (fileInfo != null)
            {
                fileInfo.CursorLine = line;
                fileInfo.CursorColumn = column;
            }
        }

        public void UpdateCodePage(string filePath, int codePage)
        {
            var fileInfo = GetExistingFileInfo(filePath);
            if (fileInfo != null)
            {
                fileInfo.CodePage = codePage;
            }
        }

        private GherkinFileInfo GetExistingFileInfo(string filePath)
        {
            List<GherkinFileInfo> files = RecentFilesInfo;
            return files.LastOrDefault(x => x.FilePath == filePath);
        }

        public GherkinFileInfo GetFileInfo(string filePath)
        {
            List<GherkinFileInfo> files = RecentFilesInfo;
            GherkinFileInfo fileInfo = files.LastOrDefault(x => x.FilePath == filePath);
            if (fileInfo == null)
            {
                fileInfo = new GherkinFileInfo() { FilePath = filePath, FontFamilyName = this.FontFamilyName, FontSize = this.FontSize };
            }
            return fileInfo;
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("")]
        public string LastSelectedFile
        {
            get { return (string)this["LastSelectedFile"]; }
            set { this["LastSelectedFile"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("")]
        public string LastUsedFile
        {
            get { return (string)this["LastUsedFile"]; }
            set
            {
                this["LastUsedFile"] = value;
                UpdateRecentFiles(value);
            }
        }

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public List<string> LastOpenedFiles
        {
            get
            {
                List<string> files = (List<string>)this["LastOpenedFiles"];
                files.RemoveAll(f => !File.Exists(f));
                return files;
            }
            set { this["LastOpenedFiles"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("1024, 800")]
        public Size MainWindowSize
        {
            get { return (Size)this["MainWindowSize"]; }
            set { this["MainWindowSize"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool IsMainWindowStateMaximized
        {
            get { return (bool)this["IsMainWindowStateMaximized"]; }
            set { this["IsMainWindowStateMaximized"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool ShowMessageWindow
        {
            get { return (bool)this["ShowMessageWindow"]; }
            set { this["ShowMessageWindow"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool SupportUnicode
        {
            get { return (bool)this["SupportUnicode"]; }
            set { this["SupportUnicode"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool IsAllowRunningMultiApps
        {
            get { return (bool)this["IsAllowRunningMultiApps"]; }
            set { this["IsAllowRunningMultiApps"] = value; }
        }


        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool IsCloseTablesFoldingByDefault
        {
            get { return (bool)this["IsCloseTablesFoldingByDefault"]; }
            set { this["IsCloseTablesFoldingByDefault"] = value; }
        }


        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool IsCloseScenarioFoldingByDefault
        {
            get { return (bool)this["IsCloseScenarioFoldingByDefault"]; }
            set { this["IsCloseScenarioFoldingByDefault"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool ShowScenarioIndexByDefault
        {
            get { return (bool)this["ShowScenarioIndexByDefault"]; }
            set { this["ShowScenarioIndexByDefault"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool ShowSplitViewByDefault
        {
            get { return (bool)this["ShowSplitViewByDefault"]; }
            set { this["ShowSplitViewByDefault"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("True")]
        public bool HighlightCurrentLine
        {
            get { return (bool)this["HighlightCurrentLine"]; }
            set { this["HighlightCurrentLine"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("True")]
        public bool ShowCurrentLineBorder
        {
            get { return (bool)this["ShowCurrentLineBorder"]; }
            set { this["ShowCurrentLineBorder"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("True")]
        public bool GenerateGUIDforScenario
        {
            get { return (bool)this["GenerateGUIDforScenario"]; }
            set { this["GenerateGUIDforScenario"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Blue")]
        public string ColorOfFoldingText
        {
            get { return (string)this["ColorOfFoldingText"]; }
            set { this["ColorOfFoldingText"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Blue")]
        public string ColorOfHighlightingKeyword
        {
            get { return (string)this["ColorOfHighlightingKeyword"]; }
            set { this["ColorOfHighlightingKeyword"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("DarkCyan")]
        public string ColorOfHighlightingStepWord
        {
            get { return (string)this["ColorOfHighlightingStepWord"]; }
            set { this["ColorOfHighlightingStepWord"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Purple")]
        public string ColorOfHighlightingTag
        {
            get { return (string)this["ColorOfHighlightingTag"]; }
            set { this["ColorOfHighlightingTag"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Peru")]
        public string ColorOfHighlightingTable
        {
            get { return (string)this["ColorOfHighlightingTable"]; }
            set { this["ColorOfHighlightingTable"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Peru")]
        public string ColorOfHighlightingDocString
        {
            get { return (string)this["ColorOfHighlightingDocString"]; }
            set { this["ColorOfHighlightingDocString"] = value; }
        }

        // Digits, string, table cell
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Peru")]
        public string ColorOfHighlightingConstants
        {
            get { return (string)this["ColorOfHighlightingConstants"]; }
            set { this["ColorOfHighlightingConstants"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("Green")]
        public string ColorOfHighlightingMockAttribute
        {
            get { return (string)this["ColorOfHighlightingMockAttribute"]; }
            set { this["ColorOfHighlightingMockAttribute"] = value; }
        }

        private void UpdateRecentFiles(string newFilePath)
        {
            List<GherkinFileInfo> files = new List<GherkinFileInfo>();
            files.AddRange(RecentFilesInfo);
            var file = files.LastOrDefault(x => x.FilePath == newFilePath);
            if (file != null)
            {
                files.RemoveAll(x => x.FilePath == newFilePath);
                files.Insert(0, file);
            }
            else
            {
                files.Insert(0, new GherkinFileInfo() { FilePath = newFilePath });
            }

            RemoveExtraFileInfo(files);

            RecentFilesInfo = files;
        }

        private static void RemoveExtraFileInfo(List<GherkinFileInfo> files)
        {
            files.RemoveAll(f => !File.Exists(f.FilePath));
            int max_files = ConfigReader.GetValue<int>("max_recent_files_for_backup", 100);
            if (files.Count() > max_files)
            {
                var count = files.Count() - max_files;
                files.RemoveRange(max_files, count);
            }
        }
    }
}
