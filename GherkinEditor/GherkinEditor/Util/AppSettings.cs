using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [DefaultSettingValueAttribute("")]
        public string LastUsedFile
        {
            get { return (string)this["LastUsedFile"]; }
            set
            {
                this["LastUsedFile"] = value;
                AdjustRecentFiles(value);
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("")]
        public List<string> RecentFiles
        {
            get
            {
                List<string> recentFiles = (List<string>)this["RecentFiles"];
                recentFiles.RemoveAll(f => !File.Exists(f));
                this["RecentFiles"] = recentFiles;
                return recentFiles;
            }
            private set { this["RecentFiles"] = value; }
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
        [DefaultSettingValueAttribute("Fangsong")]
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
        [DefaultSettingValueAttribute("True")]
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

        private void AdjustRecentFiles(string newFilePath)
        {
            List<string> files = RecentFiles;
            files.RemoveAll(f => (f == newFilePath));
            files.Insert(0, newFilePath);
            KeepFirstNFiles(files);

            RecentFiles = files;
        }

        private static void KeepFirstNFiles(List<string> files)
        {
            const int MAX_FILES = 20;
            if (files.Count > MAX_FILES)
            {
                // remove that number of items from the start of the list
                int remove_count = files.Count - MAX_FILES;
                files.RemoveRange(MAX_FILES, remove_count);
            }
        }
    }
}
