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
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public GherkinFonts Fonts
        {
            get { return (GherkinFonts)this["Fonts"]; }
            set { this["Fonts"] = value; }
        }

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public GherkinColors Colors
        {
            get { return (GherkinColors)this["Colors"]; }
            set { this["Colors"] = value; }
        }

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public LastStatus LastStatus
        {
            get { return (LastStatus)this["LastStatus"]; }
            set { this["LastStatus"] = value; }
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
                Fonts.FontSize = fontSize;
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
            return fileInfo ?? NewGherkinFileInfo(filePath);
        }

        private GherkinFileInfo NewGherkinFileInfo(string filePath)
        {
            GherkinFileInfo fileInfo = new GherkinFileInfo() { FilePath = filePath };
            if (Model.GherkinUtil.IsFeatureFile(filePath))
            {
                fileInfo.FontFamilyName = Fonts.FontFamilyName;
                fileInfo.FontSize = Fonts.FontSize;
            }
            else
            {
                fileInfo.FontFamilyName = Fonts.FontFamilyName4NonGherkin;
                fileInfo.FontSize = Fonts.FontSize4NonGherkin;
            }

            return fileInfo;
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
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public EditorSettings EditorSettings
        {
            get { return (EditorSettings)this["EditorSettings"]; }
            set { this["EditorSettings"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool IsAllowRunningMultiApps
        {
            get { return (bool)this["IsAllowRunningMultiApps"]; }
            set { this["IsAllowRunningMultiApps"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("True")]
        public bool GenerateGUIDforScenario
        {
            get { return (bool)this["GenerateGUIDforScenario"]; }
            set { this["GenerateGUIDforScenario"] = value; }
        }

         [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool SynchronizeCursorPositions
        {
            get { return (bool)this["SynchronizeCursorPositions"]; }
            set { this["SynchronizeCursorPositions"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool ShowCurvePlotMarker4GherkinTable
        {
            get { return (bool)this["ShowCurvePlotMarker4GherkinTable"]; }
            set { this["ShowCurvePlotMarker4GherkinTable"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("50")]
        public int ImageCacheSize
        {
            get { return (int)this["ImageCacheSize"]; }
            set { this["ImageCacheSize"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("False")]
        public bool OpenDocumentByNativeApplication
        {
            get { return (bool)this["OpenDocumentByNativeApplication"]; }
            set { this["OpenDocumentByNativeApplication"] = value; }
        }

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValueAttribute("")]
        public CurveManeuverParameter CurveManeuverParameter
        {
            get
            {
                CurveManeuverParameter param = (CurveManeuverParameter)this["CurveManeuverParameter"];
                return param ?? new CurveManeuverParameter();
            }
            set
            {
                this["CurveManeuverParameter"] = value;
            }
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
                files.Insert(0, NewGherkinFileInfo(newFilePath));
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
