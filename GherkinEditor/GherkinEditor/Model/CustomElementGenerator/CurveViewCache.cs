using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ICSharpCode.AvalonEdit.Document;
using Gherkin.Util;
using System.IO;
using System.Windows.Input;

namespace Gherkin.Model
{
    public class CurveViewCache : CacheBase
    {
        private static readonly Lazy<CurveViewCache> s_Singleton =
            new Lazy<CurveViewCache>(() => new CurveViewCache());

        private List<CurveViewItem> m_CurveViewItems = new List<CurveViewItem>();
        private CurvatureUnit m_CurvatureUnit = CurvatureUnit.Meter;
        private bool ShowPlotMarker { get; set; }

        public static CurveViewCache Instance => s_Singleton.Value;

        private CurveViewCache()
        {
            var setting = new AppSettings();
            bool isCentimeter = setting.CurvatureUnit.Equals("1/cm", StringComparison.InvariantCultureIgnoreCase);
            m_CurvatureUnit = isCentimeter ? CurvatureUnit.Centimeter : CurvatureUnit.Meter;
            ShowPlotMarker = setting.ShowCurvePlotMarker4GherkinTable;

            EventAggregator<ShowCurvePlotMarker4GherkinTableArg>.Instance.Event += OnShowPlotMarker;
        }

        public CurvatureUnit CurvatureUnit
        {
            get { return m_CurvatureUnit; }
            set
            {
                m_CurvatureUnit = value;
                ClearCache();
            }
        }

        public void ClearCache()
        {
            m_CurveViewItems.Clear();
        }

        public OxyPlot.Wpf.PlotView LoadPlotView(TextDocument document, int offset, CurveInfo curveInfo)
        {
            CurveViewItem curveViewItem = LoadCurveViewItem(document, offset, curveInfo);

            return curveViewItem?.PlotView;
        }

        public BitmapImage LoadImage(TextDocument document, int offset, CurveInfo curveInfo)
        {
            CurveViewItem curveViewItem = LoadCurveViewItem(document, offset, curveInfo);

            return curveViewItem?.BitmapImage;
        }

        private CurveViewItem LoadCurveViewItem(TextDocument document, int offset, CurveInfo curveInfo)
        {
            var tableHeader = GetTableHeaderLine(document, offset, curveInfo);
            if (tableHeader == null) return null;

            List<GPoint> points = ExtractPoints(document, tableHeader, curveInfo.IsGeoCoordinate);
            string key = CurvePlotModel.MakeKey(points, curveInfo);
            CurveViewItem curveViewItem = m_CurveViewItems.FirstOrDefault(x => x.Key == key);
            if (curveViewItem == null)
            {
                var curveModel = new CurvePlotModel(points, ShowPlotMarker, curveInfo);
                curveViewItem = new CurveViewItem(curveModel);
                m_CurveViewItems.Insert(0, curveViewItem);
            }

            Util.Util.RemoveLastItems(m_CurveViewItems, max_num: CacheSize);

            return curveViewItem;
        }

        private TableHeader GetTableHeaderLine(TextDocument document, int offset, CurveInfo curveInfo)
        {
            var line = document.GetLineByOffset(offset)?.NextLine;
            while (line != null)
            {
                string tableHeader = document.GetText(line).Trim();
                if (IsEmptyOrCommentLine(tableHeader))
                {
                    line = line.NextLine;
                }
                else
                {
                    var tableRow = ToRow(tableHeader);
                    int xColumnIndex = tableRow.IndexOf(curveInfo.XColumn);
                    int yColumnIndex = tableRow.IndexOf(curveInfo.YColumn);

                    if ((xColumnIndex >= 0) && (yColumnIndex >= 0))
                    {
                        return new TableHeader
                        {
                            HeaderLine = line,
                            XColumn = xColumnIndex,
                            YColumn = yColumnIndex
                        };
                    }
                    return null;
                }
            }

            return null;
        }

        private bool IsEmptyOrCommentLine(string line)
        {
            return string.IsNullOrEmpty(line) ||
                   line.StartsWith("#", StringComparison.InvariantCultureIgnoreCase);
        }

        private List<string> ToRow(string tableRow)
        {
            var candidatecolumns = tableRow.Split('|');
            List<string> columns = new List<string>();
            for (int i = 1; i < candidatecolumns.Length - 1; i++)
            {
                // create cells except first and last empty string
                columns.Add(candidatecolumns[i].Trim());
            }

            return columns;
        }

        private List<GPoint> ExtractPoints(TextDocument document, TableHeader tableHeader, bool isGeoCoordinate)
        {
            var line = tableHeader.HeaderLine.NextLine;
            List<GPoint> points = new List<GPoint>();
            while (line != null)
            {
                string rowText = document.GetText(line);
                var point = MakePoint(rowText, tableHeader.XColumn, tableHeader.YColumn);
                if (point != null)
                {
                    GPoint p = (GPoint)point;
                    points.Add(p);
                    line = line.NextLine;
                }
                else
                    break;
            }

            ThinOut(points);    // 間引き処理
            if (isGeoCoordinate)
            {
                points = LonLat2XY(points);
            }

            return points;
        }

        /// <summary>
        /// 200以内になるように間引く
        /// </summary>
        private void ThinOut(List<GPoint> points)
        {
            const int N = 200;
            int TOTAL = points.Count;

            if (TOTAL <= N) return;

            if (TOTAL >= N * 2)
            {
                int pos = 0;
                int interval = TOTAL / N;
                for (int i = 0; i < TOTAL; i += interval, pos++)
                {
                    points[pos] = points[i];
                }
                points.RemoveRange(pos, TOTAL - pos);
            }
            else
            {
                int pos = 0;
                int extraCount = TOTAL - N;
                int interval = TOTAL / extraCount;
                for (int i = 0; i < TOTAL; i++)
                {
                    if ((i == 0) || (i == TOTAL - 1) || (i % interval != 0))
                    {
                        points[pos] = points[i];
                        pos++;
                    }
                }
                points.RemoveRange(pos, points.Count - pos);
            }
        }

        public static List<GPoint> LonLat2XY(List<GPoint> points)
        {
            List<WGS8LonLat> posList = new List<WGS8LonLat>(points.Count);
            foreach (var p in points)
            {
                posList.Add(new WGS8LonLat(p.X, p.Y));
            }

            return WGS8GeoCoordinate.ToPlaneCoordinate(posList);
        }

        private GPoint MakePoint(string rowText, int XColumn, int YColumn)
        {
            var tableRow = ToRow(rowText);
            if ((tableRow.Count <= XColumn) || (tableRow.Count <= YColumn)) return null;

            try
            {
                double x = double.Parse(tableRow[XColumn]);
                double y = double.Parse(tableRow[YColumn]);
                return new GPoint(x, y);
            }
            catch
            {
                return null;
            }
        }

        private void OnShowPlotMarker(object sender, ShowCurvePlotMarker4GherkinTableArg arg)
        {
            if (ShowPlotMarker != arg.ShowMarker)
            {
                ShowPlotMarker = arg.ShowMarker;
                m_CurveViewItems.Clear();
            }
        }
    }
}
