using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Annotations;

namespace Gherkin.Model
{
    public class CurvePlotModel : OxyPlot.PlotModel
    {
        private bool ShowMarker { get; set; }
        private CurveInfo CurveInfo { get; set; }
        private bool UseCachedBSpline { get; set; } = true;
        private bool UseMatrixBSpline { get; set; } = true;

        public string Key { get; private set; } = "";

        public CurvePlotModel(List<GPoint> points, bool showMarker, CurveInfo curveInfo)
        {
            this.ShowMarker = showMarker;
            this.CurveInfo = curveInfo;
            Key = MakeKey(points, curveInfo);

            base.PlotMargins = new OxyPlot.OxyThickness(60, 0, 0, 40);
            base.Title = curveInfo.Title;

            switch (curveInfo.Option)
            {
                case "spline":
                    PlotOriginalCurve(points);
                    AddSplineByTinoKluge(points);
                    break;
                case "bspline":
                    PlotOriginalCurve(points);
                    AddBSpline(points);
                    break;
                case "bezier":
                    PlotOriginalCurve(points);
                    AddBezier(points);
                    break;
                case "brcurvature": // B-Spline Raw
                    PlotBSplineRawCurvature(points);
                    break;
                case "bcurvature":  // B-Spline -> Bezier
                    PlotBSplineCurvature(points);
                    break;
                case "curvature":   // Bezier
                    PlotBezierCurvature(points);
                    break;
                default:
                    PlotOriginalCurve(points);
                    break;
            }
        }

        public static string MakeKey(List<GPoint> points, CurveInfo curveInfo)
        {
            double xValue = 0;
            double yValue = 0;
            for (int i = 0; i < points.Count; i++)
            {
                xValue += points[i].X * (i + 1);
                yValue += points[i].Y * (i + 1);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(curveInfo.IsGeoCoordinate.ToString())
              .Append(curveInfo.Option)
              .Append("_")
              .Append(curveInfo.Description)
              .Append("_")
              .Append(curveInfo.XColumn)
              .Append(curveInfo.YColumn)
              .Append(xValue)
              .Append(yValue);

            return sb.ToString();
        }

        private void PlotOriginalCurve(List<GPoint> points)
        {
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = CurveInfo.YColumn,
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = CurveInfo.XColumn,
                Position = OxyPlot.Axes.AxisPosition.Bottom
            });
            AddLineAnnotation();

            AddPoints(points);
        }

        private void AddLineAnnotation()
        {
            var line = new LineAnnotation()
            {
                Type = LineAnnotationType.Horizontal,
                Y = 0,
                Color = OxyColors.Blue
            };
            base.Annotations.Add(line);
        }

        private void AddPoints(List<GPoint> points)
        {
            var ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Input",
                MarkerType = ShowMarker ? MarkerType.Circle : MarkerType.None
            };

            foreach (var p in points)
            {
                ls.Points.Add(new OxyPlot.DataPoint(p.X, p.Y));
            }

            base.Series.Add(ls);
        }

        private void AddBSpline(List<GPoint> points)
        {
            if (points.Count() < 3) return;

            var ls = new OxyPlot.Series.LineSeries()
            {
                Title = "BSpline",
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            IBSpline bspline = CreateBSpline(points);
            for (double u = 0; u <= 1.0; u += 0.01)
            {
                GPoint p = bspline.GetPoint(u);
                if (p != null)
                {
                    ls.Points.Add(new OxyPlot.DataPoint(p.X, p.Y));
                }
            }

            base.Series.Add(ls);
        }

        private void AddBezier(List<GPoint> points)
        {
            if (points.Count() < 3) return;

            var ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Bezier",
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            const int INTERVAL_NUM = 100;
            double step = 1.0 / INTERVAL_NUM;
            BezierCurve bezier = new BezierCurve(points);
            for (double t = 0; t < 1.0; t += step)
            {
                GPoint p = bezier.Bezier(t);
                ls.Points.Add(new OxyPlot.DataPoint(p.X, p.Y));
            }

            base.Series.Add(ls);
        }

        private void PlotBSplineCurvature(List<GPoint> points)
        {
            OxyPlot.Series.LineSeries ls;
            List<GPoint> curvaturePoints = PrepareBSplineCurvature(points, "B-Spline(->Bezier) Curvature", out ls);

            // Adjust curvature by bezier
            const int INTERVAL_NUM = 100;
            double step = 1.0 / INTERVAL_NUM;
            BezierCurve curvatureByBezier = new BezierCurve(curvaturePoints);
            for (double t = 0; t < 1.0; t += step)
            {
                GPoint p = curvatureByBezier.Bezier(t);
                ls.Points.Add(new OxyPlot.DataPoint(p.X, p.Y));
            }

            base.Series.Add(ls);
        }

        private List<GPoint> PrepareBSplineCurvature(List<GPoint> points, string curvatureTitle, out OxyPlot.Series.LineSeries ls)
        {
            // curvature
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = (CurveViewCache.Instance.CurvatureUnit == CurvatureUnit.Meter) ? "Curvature(1/m)" : "Curvature(1/cm)",
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            AddLineAnnotation();

            ls = new OxyPlot.Series.LineSeries()
            {
                Title = curvatureTitle,
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            // Raw curvature
            IBSpline bspline = CreateBSpline(points);
            List<GPoint> curvaturePoints = new List<GPoint>(100);
            for (double u = 0; u <= 1.0; u += 0.01)
            {
                double? curvature = bspline.Curvature(u);
                if (curvature != null)
                {
                    curvaturePoints.Add(new GPoint(u * 100.0, curvature.Value));
                }
            }

            return curvaturePoints;
        }

        private IBSpline CreateBSpline(List<GPoint> points)
        {
            if (UseCachedBSpline)
                return new CachedUniformCubicBSpline(points, UseMatrixBSpline);
            else
                return new UniformCubicBSpline(points);
        }

        private void PlotBSplineRawCurvature(List<GPoint> points)
        {
            OxyPlot.Series.LineSeries ls;
            List<GPoint> curvaturePoints = PrepareBSplineCurvature(points, "B-Spline Raw Curvature", out ls);
            foreach (var k in curvaturePoints)
            {
                ls.Points.Add(new OxyPlot.DataPoint(k.X, k.Y));    // Raw curvature curve
            }

            base.Series.Add(ls);
        }

        private void PlotBezierCurvature(List<GPoint> points)
        {
            // curvature
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = (CurveViewCache.Instance.CurvatureUnit == CurvatureUnit.Meter) ? "Curvature(1/m)" : "Curvature(1/cm)",
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            AddLineAnnotation();

            var ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Bezier Curvature",
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            BezierCurve bezier = new BezierCurve(points);
            for (int i = 0; i < points.Count; i++)
            {
                double curvature = bezier.Curvature(i);
                ls.Points.Add(new OxyPlot.DataPoint(i, curvature));
            }

            base.Series.Add(ls);
        }

        private void AddSplineByTinoKluge(List<GPoint> points)
        {
            CubicSpline spline = new CubicSpline();
            bool canCalculateSpline = MakeSpline(points, spline);
            if (!canCalculateSpline) return;

            var ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Spline",
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            const int INTERVAL_NUM = 50;
            if (spline.SplineKind() == CubicSpline.SplineType.OriginalXY)
            {
                double min_x = points.Min(p => p.X);
                double max_x = points.Max(p => p.X);
                double step = (max_x - min_x) / INTERVAL_NUM;
                for (double x = min_x; x <= max_x + step * 2; x += step)
                {
                    var p = new GPoint(x, 0);
                    ls.Points.Add(spline.CalcXorY(p));
                }
            }
            else
            {
                double min_y = points.Min(p => p.Y);
                double max_y = points.Max(p => p.Y);
                double step = (max_y - min_y) / INTERVAL_NUM;
                for (double y = min_y; y <= max_y + step * 2; y += step)
                {
                    var p = new GPoint(0, y);
                    ls.Points.Add(spline.CalcXorY(p));
                }
            }

            base.Series.Add(ls);
        }

        private bool MakeSpline(List<GPoint> points, CubicSpline spline)
        {
            if (points.Count() < 3) return false;

            List<double> X = new List<double>(points.Count());
            List<double> Y = new List<double>(points.Count());
            foreach (var p in points)
            {
                X.Add(p.X);
                Y.Add(p.Y);
            }
            return spline.PreparePoints(X, Y);
        }

        private void PlotSplineCurvature(List<GPoint> points)
        {
            CubicSpline spline = new CubicSpline();
            bool canCalculateSpline = MakeSpline(points, spline);
            if (!canCalculateSpline) return;

            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = "Curvature(1/m)",
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            AddLineAnnotation();

            OxyPlot.Series.LineSeries ls = CreateSplineCurvature(points, spline);

            base.Series.Add(ls);
        }

        private OxyPlot.Series.LineSeries CreateSplineCurvature(List<GPoint> points, CubicSpline spline)
        {
            var ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Curvature(1/m)"
            };

            if (ShowMarker) ls.MarkerType = MarkerType.Cross;

            const int INTERVAL_NUM = 200;
            if (spline.SplineKind() == CubicSpline.SplineType.OriginalXY)
            {
                AddSplineCurvatureByX(points, spline, ls, INTERVAL_NUM);
                AddXAxisDescription(CurveInfo.XColumn);
            }
            else
            {
                AddSplineCurvatureByY(points, spline, ls, INTERVAL_NUM);
                AddXAxisDescription(CurveInfo.YColumn);
            }

            return ls;
        }

        private static void AddSplineCurvatureByX(List<GPoint> points, CubicSpline spline, OxyPlot.Series.LineSeries ls, int INTERVAL_NUM)
        {
            double min_x = points.Min(p => p.X);
            double max_x = points.Max(p => p.X);
            double step = (max_x - min_x) / INTERVAL_NUM;
            for (double x = min_x; x <= max_x + step * 2; x += step)
            {
                double curvature = spline.CalculateCurvature(x);
                ls.Points.Add(new OxyPlot.DataPoint(x, curvature));
            }
        }

        private static void AddSplineCurvatureByY(List<GPoint> points, CubicSpline spline, OxyPlot.Series.LineSeries ls, int INTERVAL_NUM)
        {
            double min_y = points.Min(p => p.Y);
            double max_y = points.Max(p => p.Y);
            double step = (max_y - min_y) / INTERVAL_NUM;
            for (double y = min_y; y <= max_y + step * 2; y += step)
            {
                double curvature = spline.CalculateCurvature(y);
                ls.Points.Add(new OxyPlot.DataPoint(y, curvature));
            }
        }

        private void AddXAxisDescription(string title)
        {
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = title,
                Position = OxyPlot.Axes.AxisPosition.Bottom
            });
        }
    }
}
