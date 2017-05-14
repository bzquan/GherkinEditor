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
        private string m_Key = "";

        public CurvePlotModel(List<OxyPlot.DataPoint> points, string title, string xTitle, string yTitle, bool showMarker, string spline)
        {
            m_Key = MakeKey(points, xTitle, yTitle, spline);
            base.PlotMargins = new OxyPlot.OxyThickness(60, 0, 0, 40);
            base.Title = title;
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = yTitle,
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = xTitle,
                Position = OxyPlot.Axes.AxisPosition.Bottom
            });
            AddLineAnnotation();
            AddPoints(points, showMarker);
            if (spline.Length > 0)
            {
                AddSplineByTinoKluge(points);
                //AddSplineByCodeProject(points);
            }
        }

        public string Key => m_Key;

        private void AddLineAnnotation()
        {
            var line = new LineAnnotation()
            {
                Type = LineAnnotationType.Horizontal,
                Y = 0,
                MinimumX = 0,
                Color = OxyColors.Blue
            };
            base.Annotations.Add(line);
        }

        private string MakeKey(List<OxyPlot.DataPoint> points, string xTitle, string yTitle, string spline)
        {
            double xValue = 0;
            double yValue = 0;
            for (int i  = 0; i < points.Count; i++)
            {
                xValue += points[i].X * (i + 1);
                yValue += points[i].Y * (i + 1);
            }

            return spline +"_" +  xTitle + yTitle + "x_" + xValue.ToString() + "y_" + yValue.ToString();
        }

        private void AddPoints(List<OxyPlot.DataPoint> points, bool showMarker)
        {
            var ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Input",
                MarkerType = showMarker ? MarkerType.Circle : MarkerType.None
            };
            ls.Points.AddRange(points);
            base.Series.Add(ls);
        }

        private void AddSplineByTinoKluge(List<OxyPlot.DataPoint> points)
        {
            if (points.Count() < 3) return;

            List<double> X = new List<double>(points.Count());
            List<double> Y = new List<double>(points.Count());
            foreach (var p in points)
            {
                X.Add(p.X);
                Y.Add(p.Y);
            }
            CubicSpline spline = new CubicSpline();
            spline.PreparePoints(X, Y);

            var ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Spline",
                MarkerType = MarkerType.Cross
            };

            const int INTERVAL_NUM = 50;
            if (spline.SplineKind() == CubicSpline.SplineType.OriginalXY)
            {
                double min_x = points.Min(p => p.X);
                double max_x = points.Max(p => p.X);
                double step = (max_x - min_x) / INTERVAL_NUM;
                for (double x = min_x; x <= max_x + step * 2; x += step)
                {
                    var p = new OxyPlot.DataPoint(x, 0);
                    ls.Points.Add(spline.CalculateY(p));
                }
            }
            else
            {
                double min_y = points.Min(p => p.Y);
                double max_y = points.Max(p => p.Y);
                double step = (max_y - min_y) / INTERVAL_NUM;
                for (double y = min_y; y <= max_y + step * 2; y += step)
                {
                    var p = new OxyPlot.DataPoint(0, y);
                    ls.Points.Add(spline.CalculateY(p));
                }
            }

            base.Series.Add(ls);
        }

        //private void AddSplineByCodeProject(List<OxyPlot.DataPoint> points)
        //{
        //    if (points.Count() < 3) return;
        //    double[] x = new double[points.Count()];
        //    double[] y = new double[points.Count()];
        //    for (int i = 0; i < points.Count(); i++)
        //    {
        //        x[i] = points[i].X;
        //        y[i] = points[i].Y;
        //    }

        //    double[] xs, ys;
        //    int resultN = 50;
        //    CubicSpline.FitParametric(x, y, resultN, out xs, out ys);
        //    var ls = new OxyPlot.Series.LineSeries()
        //    {
        //        Title = "Spline CodeProject",
        //        MarkerType = MarkerType.Plus,
        //        MarkerStroke = OxyPlot.OxyColors.Blue
        //    };

        //    for (int i = 0; i < xs.Length; i++)
        //    {
        //        ls.Points.Add(new DataPoint(xs[i], ys[i]));
        //    }

        //    base.Series.Add(ls);
        //}
    }
}
