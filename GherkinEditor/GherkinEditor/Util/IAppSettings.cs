using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gherkin.Util
{
    public interface IAppSettings
    {
        Languages Language { get; set; }
        Size MainWindowSize { get; set; }
        string LastUsedFile { get; set; }
        List<string> LastOpenedFiles { get; set; }
        string FontFamilyName { get; set; }
        string FontSize { get; set; }
        List<GherkinFileInfo> RecentFilesInfo { get; }
        void UpdateFontFamilyName(string filePath, string fontFamilyName);
        void UpdateFontSize(string filePath, string fontSize);
        void UpdateCursorPos(string filePath, int line, int column);
        GherkinFileInfo GetFileInfo(string filePath);
        bool IsMainWindowStateMaximized { get; set; }
        bool ShowMessageWindow { get; set; }
        bool SupportUnicode { get; set; }
        bool IsAllowRunningMultiApps { get; set; }
        bool IsCloseTablesFoldingByDefault { get; set; }
        bool IsCloseScenarioFoldingByDefault { get; set; }
        bool ShowScenarioIndexByDefault { get; set; }
        bool ShowSplitViewByDefault { get; set; }
        bool HighlightCurrentLine { get; set; }
        bool ShowCurrentLineBorder { get; set; }
        bool GenerateGUIDforScenario { get; set; }

        string ColorOfFoldingText { get; set; }
        string ColorOfHighlightingKeyword { get; set; }
        string ColorOfHighlightingStepWord { get; set; }
        string ColorOfHighlightingTag { get; set; }
        string ColorOfHighlightingTable { get; set; }
        string ColorOfHighlightingDocString { get; set; }
        // Digits, string, table cell
        string ColorOfHighlightingConstants { get; set; }
        string ColorOfHighlightingMockAttribute { get; set; }

        void Save();
    }
}
