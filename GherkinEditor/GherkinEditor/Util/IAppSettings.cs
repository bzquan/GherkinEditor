using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gherkin.Util
{
    public class GherkinFileInfo
    {
        public string FilePath = "";
        public string FontFamilyName = "KaiTi";
        public string FontSize = "11";
        public int CursorLine = 1;
        public int CursorColumn = 1;
        public int CodePage = 0;    // undefined: use automatic detection
    }

    public interface IAppSettings
    {
        Languages Language { get; set; }
        Size MainWindowSize { get; set; }
        string LastSelectedFile { get; set; }
        string LastUsedFile { get; set; }
        List<string> LastOpenedFiles { get; set; }
        string LastFolderToCopyFile { get; set; }

        string LastSearchedText { set; }
        List<string> LastSearchedTexts { get; set; }
        string LastGreppedText { set; }
        List<string> LastGreppedTexts { get; set; }
        string LastUsedFileExtension { set; }
        List<string> LastFileExtensions { get; set; }
        string LastGreppedFolder { get; set; }
        List<string> LastGreppedFolders { get; set; }
        bool IsCaseSensitiveInFind { get; set; }
        bool IsMatchWholeWordInFind { get; set; }
        bool IsUseRegexInFind { get; set; }
        bool IsUseWildcardsInFind { get; set; }
        bool RequireControlModifierForHyperlinkClick { get; set; }

        string FontFamilyName { get; set; }
        string FontFamilyName4NonGherkin { get; set; }
        string FontSize { get; set; }
        string FontSize4NonGherkin { get; set; }
        List<GherkinFileInfo> RecentFilesInfo { get; }
        void UpdateFontFamilyName(string filePath, string fontFamilyName);
        void UpdateFontSize(string filePath, string fontSize);
        void UpdateCursorPos(string filePath, int line, int column);
        void UpdateCodePage(string filePath, int codePage);
        GherkinFileInfo GetFileInfo(string filePath);
        bool IsMainWindowStateMaximized { get; set; }
        bool ShowColumnRuler { get; set; }
        int ColumnRulerPositon { get; set; }
        bool SupportUnicode { get; set; }
        bool IsAllowRunningMultiApps { get; set; }
        bool IsCloseTablesFoldingByDefault { get; set; }
        bool IsCloseScenarioFoldingByDefault { get; set; }
        bool ShowScenarioIndexByDefault { get; set; }
        bool ShowSplitHorizontalViewByDefault { get; set; }
        bool ShowSplitVerticalViewByDefault { get; set; }
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

        bool ConvertTabsToSpaces { get; set; }
        int IndentationSize { get; set; }
        bool ShowEndOfLine { get; set; }
        bool ShowSpaces { get; set; }
        bool ShowTabs { get; set; }
        bool WordWrap { get; set; }
        bool SynchronizeCursorPositions { get; set; }
        bool ShowCurvePlotMarker4GherkinTable { get; set; }

        int ImageCacheSize { get; set; }
        bool OpenDocumentByNativeApplication { get; set; }

        void Save();
    }
}
