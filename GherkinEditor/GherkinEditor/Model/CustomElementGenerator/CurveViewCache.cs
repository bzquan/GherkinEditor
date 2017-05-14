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
        private bool ShowPlotMarker { get; set; }

        public static CurveViewCache Instance => s_Singleton.Value;

        private CurveViewCache()
        {
            var setting = new AppSettings();
            ShowPlotMarker = setting.ShowCurvePlotMarker4GherkinTable;

            EventAggregator<ShowCurvePlotMarker4GherkinTableArg>.Instance.Event += OnShowPlotMarker;
        }

        private void OnShowPlotMarker(object sender, ShowCurvePlotMarker4GherkinTableArg arg)
        {
            if ( ShowPlotMarker != arg.ShowMarker)
            {
                ShowPlotMarker = arg.ShowMarker;
                m_CurveViewItems.Clear();
            }
        }

        public OxyPlot.Wpf.PlotView LoadPlotView(TextDocument document, int offset, string description, string xColumn, string yColumn, string spline)
        {
            CurveViewItem curveViewItem = LoadCurveViewItem(document, offset, description, xColumn, yColumn, spline);

            return curveViewItem?.PlotView;
        }

        public BitmapImage LoadImage(TextDocument document, int offset, string description, string xColumn, string yColumn, string spline)
        {
            CurveViewItem curveViewItem = LoadCurveViewItem(document, offset, description, xColumn, yColumn, spline);

            return curveViewItem?.BitmapImage;
        }

        private CurveViewItem LoadCurveViewItem(TextDocument document, int offset, string description, string xColumn, string yColumn, string spline)
        {
            var tableHeader = GetTableHeaderLine(document, offset, description, xColumn, yColumn);
            if (tableHeader == null) return null;

            tableHeader.XTitle = xColumn;
            tableHeader.YTitle = yColumn;

            var curveModel = CreateModel(document, tableHeader, spline);

            CurveViewItem curveViewItem = m_CurveViewItems.FirstOrDefault(x => x.Key == curveModel.Key);
            if (curveViewItem != null)
                m_CurveViewItems.Remove(curveViewItem);
            else
                curveViewItem = new CurveViewItem(curveModel);

            m_CurveViewItems.Insert(0, curveViewItem);
            Util.Util.RemoveLastItems(m_CurveViewItems, max_num: CacheSize);

            return curveViewItem;
        }

        private TableHeader GetTableHeaderLine(TextDocument document, int offset, string description, string xColumn, string yColumn)
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
                    int xColumnIndex = tableRow.IndexOf(xColumn);
                    int yColumnIndex = tableRow.IndexOf(yColumn);

                    if ((xColumnIndex >= 0) && (yColumnIndex >= 0))
                    {
                        return new TableHeader
                        {
                            HeaderLine = line,
                            Description = description,
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

        CurvePlotModel CreateModel(TextDocument document, TableHeader tableHeader, string spline)
        {
            var line = tableHeader.HeaderLine.NextLine;
            List<OxyPlot.DataPoint> points = new List<OxyPlot.DataPoint>();
            while (line != null)
            {
                string rowText = document.GetText(line);
                var point = MakePoint(rowText, tableHeader.XColumn, tableHeader.YColumn);
                if (point != null)
                {
                    OxyPlot.DataPoint p = (OxyPlot.DataPoint)point;
                    points.Add(p);
                    line = line.NextLine;
                }
                else
                    break;
            }

            var plotModel = new CurvePlotModel(points, tableHeader.Description, tableHeader.XTitle, tableHeader.YTitle, ShowPlotMarker, spline);

            return plotModel;
        }

        private OxyPlot.DataPoint? MakePoint(string rowText, int XColumn, int YColumn)
        {
            var tableRow = ToRow(rowText);
            if ((tableRow.Count <= XColumn) || (tableRow.Count <= YColumn)) return null;

            try
            {
                double x = double.Parse(tableRow[XColumn]);
                double y = double.Parse(tableRow[YColumn]);
                return new OxyPlot.DataPoint(x, y);
            }
            catch
            {
                return null;
            }
        }
    }
}
