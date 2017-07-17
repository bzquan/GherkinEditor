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
using Gherkin.Util.Geometric;
using Gherkin.Util.Bezier;
using System.Diagnostics;

namespace Gherkin.Model
{
    public class CurveViewCache : CacheBase
    {
        private static readonly Lazy<CurveViewCache> s_Singleton =
            new Lazy<CurveViewCache>(() => new CurveViewCache());

        private List<CurveViewItem> m_CurveViewItems = new List<CurveViewItem>();
        private CurveManeuverParameter m_CurveManeuverParameter;
        private bool ShowPlotMarker { get; set; }

        public static CurveViewCache Instance => s_Singleton.Value;

        private CurveViewCache()
        {
            var setting = new AppSettings();
            m_CurveManeuverParameter = setting.CurveManeuverParameter;
            ShowPlotMarker = setting.ShowCurvePlotMarker4GherkinTable;

            EventAggregator<ShowCurvePlotMarker4GherkinTableArg>.Instance.Event += OnShowPlotMarker;
            EventAggregator<CurveManeuverParameterArg>.Instance.Event += OnCurveManeuverParameterChanged;
            EventAggregator<EditorClosedArg>.Instance.Event += OnEditorClosed;
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

        private void OnCurveManeuverParameterChanged(object sender, CurveManeuverParameterArg arg)
        {
            m_CurveManeuverParameter = arg.CurveManeuverParameter;
            m_CurveViewItems.Clear();
        }

        private void OnEditorClosed(object sender, EditorClosedArg arg)
        {
            if (string.IsNullOrEmpty(arg.FileName)) return;

            for (int i = m_CurveViewItems.Count - 1; i >= 0; i--)
            {
                if (m_CurveViewItems[i].Key.StartsWith(arg.FileName, StringComparison.Ordinal))
                {
                    m_CurveViewItems.RemoveAt(i);
                }
            }
        }

        private CurveViewItem LoadCurveViewItem(TextDocument document, int offset, CurveInfo curveInfo)
        {
            var tableHeader = GetTableHeaderLine(document, offset, curveInfo);
            if (tableHeader == null) return null;

            double duration;
            List<GPoint> points = ExtractPoints(document, tableHeader, curveInfo.IsGeoCoordinate, out duration);
            if (points.Count < 3) return null;

            string key = CurvePlotModel.MakeKey(document.FileName, points, curveInfo);
            CurveViewItem curveViewItem = m_CurveViewItems.FirstOrDefault(x => x.Key == key);
            if (curveViewItem == null)
            {
                var curveModel = new CurvePlotModel(document.FileName, points, ShowPlotMarker, curveInfo, m_CurveManeuverParameter, duration);
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

        private List<GPoint> ExtractPoints(TextDocument document, TableHeader tableHeader, bool isGeoCoordinate, out double duration)
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

            points = Thinout(points, maxPointsCount: 500, duration: out duration); // 500以内になるように間引く
            if (isGeoCoordinate)
            {
                points = LonLat2XY(points);
            }

            return points;
        }

        /// <summary>
        /// maxCount以内になるように間引く
        /// </summary>
        /// <param name="points">input</param>
        /// <param name="maxPointsCount">expected max number of points</param>
        /// <returns></returns>
        private List<GPoint> Thinout(List<GPoint> points, int maxPointsCount, out double duration)
        {
            duration = 0.0;
            if (points.Count <= maxPointsCount)
            {
                return points;
            }

            List<GPoint> simplified = null;
            duration = Util.Util.MeasureExecTime(() => {
                if (m_CurveManeuverParameter.ThinoutByDouglasPeuckerN)
                    simplified = PolylineSimplication.SimplifyByDouglasPeuckerN(points.ToArray(), maxPointsCount);
                else
                    simplified = PolylineSimplication.SimplifyPointsN(points.ToArray(), maxPointsCount);
            });
            return simplified;
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
