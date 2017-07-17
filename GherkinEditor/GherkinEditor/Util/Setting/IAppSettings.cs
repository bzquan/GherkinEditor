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
        bool IsMainWindowStateMaximized { get; set; }

        LastStatus LastStatus { get; set; }
        string LastUsedFile { get; set; }
        List<string> LastOpenedFiles { get; set; }
        List<GherkinFileInfo> RecentFilesInfo { get; }

        GherkinFonts Fonts { get; set; }
        GherkinColors Colors { get; set; }
        void UpdateFontFamilyName(string filePath, string fontFamilyName);
        void UpdateFontSize(string filePath, string fontSize);
        void UpdateCursorPos(string filePath, int line, int column);
        void UpdateCodePage(string filePath, int codePage);
        GherkinFileInfo GetFileInfo(string filePath);

        EditorSettings EditorSettings { get; set; }

        bool IsAllowRunningMultiApps { get; set; }
        bool GenerateGUIDforScenario { get; set; }
        bool SynchronizeCursorPositions { get; set; }
        bool ShowCurvePlotMarker4GherkinTable { get; set; }

        int ImageCacheSize { get; set; }
        bool OpenDocumentByNativeApplication { get; set; }
        CurveManeuverParameter CurveManeuverParameter { get; set; }

        void Save();
    }
}
