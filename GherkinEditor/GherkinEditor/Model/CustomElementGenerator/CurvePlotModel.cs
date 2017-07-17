using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Annotations;
using Gherkin.Util.Geometric;
using Gherkin.Util.NURBS;
using Gherkin.Util.Bezier;
using Gherkin.Util;

namespace Gherkin.Model
{
    public class CurvePlotModel : OxyPlot.PlotModel
    {
        private bool ShowMarker { get; set; }
        private CurveInfo CurveInfo { get; set; }
        private CurveManeuverParameter CurveManeuverParameter { get; set; }
        private double PointExtractionDurationTimeMS { get; set; }

        public string Key { get; private set; } = "";

        public CurvePlotModel(string featureFileName,
                              List<GPoint> points,
                              bool showMarker,
                              CurveInfo curveInfo,
                              CurveManeuverParameter curveManeuverParameter,
                              double pointExtractionDuration)
        {
            this.ShowMarker = showMarker;
            this.CurveInfo = curveInfo;
            this.CurveManeuverParameter = curveManeuverParameter;
            this.PointExtractionDurationTimeMS = pointExtractionDuration;

            Key = MakeKey(featureFileName, points, curveInfo);

            double curveLength = BezierCurveLength.LengthByQuadratic(points);
            base.PlotMargins = new OxyPlot.OxyThickness(60, 0, 0, 40);
            base.Title = curveInfo.Title;

            List<GPoint> newPoint;
            switch (curveInfo.Option)
            {
                case "simplify":
                    PlotOriginalCurve(points);
                    PlotSimplifyCurvature(points);
                    break;
                case "simplify_cm":
                    newPoint = ConvertCurvatureUnit(points, curveInfo);
                    PlotOriginalCurve(newPoint);
                    PlotSimplifyCurvature(newPoint);
                    break;
                case "bezier":
                    PlotOriginalCurve(points);
                    AddBezierByNURBS(points);
                    PlotCurveManeuver(points, curveLength, points.Count - 1);
                    break;
                case "bspline":
                    PlotOriginalCurve(points);
                    AddBSplineByNURBS(points, curveLength);
                    PlotCurveManeuver(points, curveLength, SplineDegree(points));
                    break;
                case "brcurvature": // Raw curvature
                    PlotBSplineRawCurvatureByNURBS(points, curveLength);
                    break;
                case "bcurvature":  // Bezier curvature
                case "curvature":
                    PlotBezierCurvatureByNURBS(points, curveLength);
                    break;
                case "tangent":   // Tangent
                    PlotTangent(points);
                    break;
                case "degtangent":   // Tangent by degree of Bezier curve
                    PlotBezierTangentByDegree(points);
                    break;
                case "linreg":   // linear regression
                    PlotOriginalCurve(points, useLinearSeries: false);
                    PlotLinearRegression(points);
                    break;
                default:
                    PlotOriginalCurve(points);
                    break;
            }
        }

        public static string MakeKey(string featureFileName, List<GPoint> points, CurveInfo curveInfo)
        {
            double xValue = 0;
            double yValue = 0;
            for (int i = 0; i < points.Count; i++)
            {
                xValue += points[i].X * (i + 1);
                yValue += points[i].Y * (i + 1);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(featureFileName)
              .Append(curveInfo.IsGeoCoordinate.ToString())
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

        private CurvatureUnit CurvatureUnit => this.CurveManeuverParameter.Unit;

        private List<GPoint> ConvertCurvatureUnit(List<GPoint> points, CurveInfo curveInfo)
        {
            if (curveInfo.Option != "simplify_cm") return points;

            List<GPoint> newPoints = new List<GPoint>(points.Count);
            points.ForEach(x => newPoints.Add(new GPoint(x.X, x.Y * 100.0)));
            return newPoints;
        }

        private void PlotOriginalCurve(List<GPoint> points, bool useLinearSeries = true)
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
            AddZeroLineAnnotation();

            if (useLinearSeries)
            {
                string title = "Input";
                if (PointExtractionDurationTimeMS > 0)
                {
                    title += $"({PointExtractionDurationTimeMS:0.###}ms)";
                }
                AddLinearSeries(points, title);
            }
            else
                AddScatterSeries(points, "Input");
        }

        private void AddZeroLineAnnotation()
        {
            var line = new LineAnnotation()
            {
                Type = LineAnnotationType.Horizontal,
                Y = 0,
                Color = OxyColors.Blue
            };
            base.Annotations.Add(line);
        }

        private void AddLinearSeries(List<GPoint> points, string title)
        {
            var ls = new OxyPlot.Series.LineSeries()
            {
                Title = title,
                MarkerType = ShowMarker ? MarkerType.Circle : MarkerType.None
            };

            foreach (var p in points)
            {
                ls.Points.Add(new OxyPlot.DataPoint(p.X, p.Y));
            }

            base.Series.Add(ls);
        }

        private void AddScatterSeries(List<GPoint> points, string title, MarkerType markerType = MarkerType.Diamond)
        {
            var scatterSeries = new OxyPlot.Series.ScatterSeries()
            {
                Title = title,
                MarkerType = markerType,
            };

            if (markerType != MarkerType.Diamond)
            {
                scatterSeries.MarkerStroke = OxyPlot.OxyColors.Blue;
            }

            foreach (var p in points)
            {
                scatterSeries.Points.Add(new OxyPlot.Series.ScatterPoint(p.X, p.Y));
            }

            base.Series.Add(scatterSeries);
        }

        private void AddBSplineByNURBS(List<GPoint> points, double curveLength)
        {
            if (points.Count() < 3) return;

            var ls = new OxyPlot.Series.LineSeries()
            {
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            int degree = SplineDegree(points);
            double duration = Util.Util.MeasureExecTime(()=>
            {
                NURBSCurve bspline = new NURBSCurve(points, degree);
                for (double u = 0; u <= 1.0; u += 0.01)
                {
                    GPoint p = bspline.NonRationalBSpline(u);
                    ls.Points.Add(new OxyPlot.DataPoint(p.X, p.Y));
                }
            });

            ls.Title = $"NURBS({degree})[len={curveLength:0.#}]({duration:0.###}ms)";
            base.Series.Add(ls);
        }

        private void AddBezierByNURBS(List<GPoint> points)
        {
            if (points.Count() < 3) return;

            int degree = points.Count - 1;
            var ls = new OxyPlot.Series.LineSeries()
            {
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            double duration = Util.Util.MeasureExecTime(() =>
            {
                const int INTERVAL_NUM = 100;
                double step = 1.0 / INTERVAL_NUM;
                NURBSCurve bspline = new NURBSCurve(points, degree);
                for (double t = 0; t < 1.0; t += step)
                {
                    GPoint p = bspline.NonRationalBSpline(t);
                    ls.Points.Add(new OxyPlot.DataPoint(p.X, p.Y));
                }
            });

            ls.Title = $"NURBS(Bezier-{degree})({duration:0.###}ms)";
            base.Series.Add(ls);
        }

        private void PlotBezierCurvatureByNURBS(List<GPoint> points, double curvelength)
        {
            // curvature
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = (CurvatureUnit == CurvatureUnit.Meter) ? "Curvature(1/m)" : "Curvature(1/cm)",
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            AddZeroLineAnnotation();

            int degree = points.Count - 1;
            OxyPlot.Series.LineSeries ls = new OxyPlot.Series.LineSeries()
            {
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            List<GPoint> curvatures = new List<GPoint>();
            double duration = Util.Util.MeasureExecTime(() =>
            {
                double scale = (CurvatureUnit == CurvatureUnit.Centimeter) ? 0.01 : 1.0; // 1/cm or 1/m
                NURBSCurve bspline = new NURBSCurve(points, degree);
                for (double u = 0.01; u < 1.0 - 0.01; u += 0.01)
                {
                    double curvature = bspline.Curvature(u) * scale;
                    double dist = curvelength * u;
                    ls.Points.Add(new OxyPlot.DataPoint(dist, curvature));
                    curvatures.Add(new GPoint(dist, curvature, weight: dist));
                }
            });

            ls.Title = $"NURBS(Bezier-{degree}) Curvature({duration:0.###}ms)";
            base.Series.Add(ls);

            PlotSimplifiedPolyline(curvatures);
            PlotManeuverOnCurvatureCurve(curvatures);
        }

        private void PlotManeuverOnCurvatureCurve(List<GPoint> curvatures)
        {
            List<GPoint> simplified = SimplifyPolyline(curvatures, CurveManeuverParameter.CurveMinDistance);
            List<GPoint> maneuvers = PolylineSimplication.SimplifyHeadingChange(simplified.ToArray(), CurveManeuverParameter.YTolerance);

            AddScatterSeries(maneuvers, "Maneuvers");
            PlotSameRanges(maneuvers);
        }

        private void PlotSameRanges(List<GPoint> maneuvers)
        {
            List<GPoint> sameRanges = PolylineSimplication.ExtractSameRanges(maneuvers.ToArray(), CurveManeuverParameter.YTolerance);
            AddScatterSeries(sameRanges, "YValue-Tolerance", MarkerType.Cross);
        }

        private void PlotBSplineRawCurvatureByNURBS(List<GPoint> points, double curvelength)
        {
            // curvature
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = (CurvatureUnit == CurvatureUnit.Meter) ? "Curvature(1/m)" : "Curvature(1/cm)",
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            AddZeroLineAnnotation();

            int degree = SplineDegree(points);
            OxyPlot.Series.LineSeries ls = new OxyPlot.Series.LineSeries()
            {
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            List<GPoint> curvatures = new List<GPoint>();
            double duration = Util.Util.MeasureExecTime(() =>
            {
                double scale = (CurvatureUnit == CurvatureUnit.Centimeter) ? 0.01 : 1.0; // 1/cm or 1/m
                NURBSCurve bspline = new NURBSCurve(points, degree);
                for (double u = 0.01; u < 1.0 - 0.01; u += 0.01)
                {
                    double curvature = bspline.Curvature(u) * scale;
                    double dist = curvelength * u;
                    ls.Points.Add(new OxyPlot.DataPoint(dist, curvature));
                    curvatures.Add(new GPoint(dist, curvature, weight: dist));
                }
            });

            ls.Title = $"NURBS({degree}) Curvature({duration:0.###}ms)";
            base.Series.Add(ls);

            PlotSimplifiedPolyline(curvatures);
            PlotManeuverOnCurvatureCurve(curvatures);
        }

        private int SplineDegree(List<GPoint> points) => Math.Min(CurveManeuverParameter.NURBSDegree, points.Count - 1);

        private void PlotCurveManeuver(List<GPoint> points, double curveLength, int degree)
        {
            if (points.Count < 2) return;

            double scale = (CurvatureUnit == CurvatureUnit.Centimeter) ? 0.01 : 1.0; // 1/cm or 1/m
            List<GPoint> curvatures = new List<GPoint>();
            NURBSCurve bspline = new NURBSCurve(points, degree);
            for (double u = 0.01; u < 1.0 - 0.01; u += 0.01)
            {
                double curvature = bspline.Curvature(u);
                double distanceFromStart = curveLength * u;
                curvatures.Add(new GPoint(u * 100.0, curvature * scale, weight: distanceFromStart));
            }

            List<GPoint> simplified = SimplifyPolyline(curvatures, CurveManeuverParameter.CurveMinDistance);
            List<GPoint> curvaturManeuvers = PolylineSimplication.SimplifyHeadingChange(simplified.ToArray(), CurveManeuverParameter.YTolerance);

            List<GPoint> maneuvers = new List<GPoint>();
            foreach (var p in curvaturManeuvers)
            {
                int index = Weight2Index(p, curveLength, points.Count);
                maneuvers.Add(points[index]);
            }

            AddScatterSeries(maneuvers, "Maneuvers");
        }

        private int Weight2Index(GPoint p, double curveLength, int totalPoints)
        {
            double indexFromCurvature = p.W / curveLength;
            return (int)((totalPoints - 1) * indexFromCurvature);
        }

        private List<GPoint> PlotSimplifiedPolyline(List<GPoint> curvatures)
        {
            OxyPlot.Series.LineSeries ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Polyline Simplication",
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Diamond,
                MarkerStroke = OxyPlot.OxyColors.Pink
            };
            List<GPoint> simplified = SimplifyPolyline(curvatures, 0);
            foreach (var p in simplified)
            {
                ls.Points.Add(new OxyPlot.DataPoint(p.X, p.Y));
            }

            base.Series.Add(ls);
            return simplified;
        }

        private void PlotSimplifyCurvature(List<GPoint> curvatures)
        {
            OxyPlot.Series.LineSeries ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Direct Simplication",
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Diamond,
                MarkerStroke = OxyPlot.OxyColors.Pink
            };
            List<GPoint> simplified1 = SimplifyPolyline(curvatures, 0);
            foreach (var p in simplified1)
            {
                ls.Points.Add(new OxyPlot.DataPoint(p.X, p.Y));
            }

            base.Series.Add(ls);

            // Plot maneuvers
            curvatures.ForEach(x => x.W = x.X); // X is distance
            List<GPoint> simplified2 = SimplifyPolyline(curvatures, CurveManeuverParameter.CurveMinDistance);
            List<GPoint> maneuvers = PolylineSimplication.SimplifyHeadingChange(simplified2.ToArray(), CurveManeuverParameter.YTolerance);

            AddScatterSeries(maneuvers, "Maneuvers");
            PlotSameRanges(maneuvers);
        }

        private List<GPoint> SimplifyPolyline(List<GPoint> curvatures, double distanceTolerance)
        {
            List<GPoint> simplified = PolylineSimplication.SimplifyByYTolerance(curvatures.ToArray(), CurveManeuverParameter.CurveCurvatureThreshold);
            List<GPoint> newPoints = PolylineSimplication.SimplifyByDouglasPeucker(simplified.ToArray(), CurveManeuverParameter.DouglasPeuckerTolerance);
            if (distanceTolerance > 0)
            {
                newPoints = PolylineSimplication.SimplifyByWeight(newPoints.ToArray(), distanceTolerance);
            }

            return newPoints;
        }

        private void PlotTangent(List<GPoint> points)
        {
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = "Tangent",
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            AddZeroLineAnnotation();

            OxyPlot.Series.LineSeries ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Tangent",
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            int degree = points.Count - 1;
            NURBSCurve bspline = new NURBSCurve(points, degree);
            for (double u = 0; u <= 1.0; u += 0.01)
            {
                double tangent = bspline.Tangent(u);
                ls.Points.Add(new OxyPlot.DataPoint(u * 100.0, tangent));
            }

            base.Series.Add(ls);
        }

        private void PlotBezierTangentByDegree(List<GPoint> points)
        {
            base.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = "Tangent of Bezier(Degree)",
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            AddZeroLineAnnotation();

            OxyPlot.Series.LineSeries ls = new OxyPlot.Series.LineSeries()
            {
                Title = "Tangent of Bezier(Degree)",
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            int degree = points.Count - 1;
            NURBSCurve bspline = new NURBSCurve(points, degree);
            for (double u = 0; u <= 1.0; u += 0.01)
            {
                double tangent = bspline.TangentByDegree(u);
                ls.Points.Add(new OxyPlot.DataPoint(u * 100.0, tangent));
            }

            base.Series.Add(ls);
        }

        private void PlotLinearRegression(List<GPoint> points)
        {
            var ls = new OxyPlot.Series.LineSeries()
            {
                MarkerType = ShowMarker ? MarkerType.None : MarkerType.Plus,
                MarkerStroke = OxyPlot.OxyColors.Blue
            };

            LinearFunction func;
            if (LinearRegression.Regression(points.ToArray(), out func))
            {
                int TOTAL = 10;
                double x = points.First().X;
                double stepX = (points.Last().X - x) / TOTAL;
                for (int i = 0; i <= TOTAL; i++)
                {
                    double y = func.Slope * x + func.Intercept;
                    ls.Points.Add(new OxyPlot.DataPoint(x, y));
                    x += stepX;
                }
                ls.Title = $"Linear Regression(R: {func.CorrelationCoeff:f5})";
            }
            else
            {
                ls.Title = "Linear Regression(Failed)";
            }

            base.Series.Add(ls);
        }
    }
}
