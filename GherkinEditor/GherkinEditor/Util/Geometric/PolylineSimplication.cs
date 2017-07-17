using Gherkin.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Util.Geometric
{
    /// <summary>
    /// Defines the key of a polyline
    /// </summary>
    class KeyInfo
    {
        public int Index { get; set; }  // index of the key
        public double Dist2 { get; set; }  // squared distance of the key to a segment
        public KeyInfo(int index = 0, double dist2 = 0)
        {
            Index = index;
            Dist2 = dist2;
        }
    }

    /// <summary>
    /// Defines a sub polyline
    /// </summary>
    class SubPoly
    {
        public int First { get; set; }    // index of the first point
        public int Last { get; set; }     // index of the last point
        public SubPoly(int first = 0, int last = 0)
        {
            First = first;
            Last = last;
        }
    };

    /// <summary>
    /// Defines a sub polyline including its key
    /// </summary>
    class SubPolyAlt : IComparable
    {
        public int First { get; set; }          // index of the first point
        public int Last { get; set; }           // index of the last point
        public KeyInfo KeyInfo { get; set; }    // key of this sub poly

        public SubPolyAlt(int first = 0, int last = 0)
        {
            First = first;
            Last = last;
        }

        public int CompareTo(object obj)
        {
            SubPolyAlt other = obj as SubPolyAlt;
            if (other == null) return -1;

            if (KeyInfo.Dist2 < other.KeyInfo.Dist2)
                return -1;
            else if (KeyInfo.Dist2 < other.KeyInfo.Dist2)
                return 1;
            else
                return 0;
        }
    }

    /// <summary>
    /// Simplification of a 2D-polyline
    /// More information:
    /// https://github.com/imshz/simplify-net
    /// http://psimpl.sourceforge.net/index.html
    /// http://psimpl.sf.net
    /// https://www.codeproject.com/articles/114797/polyline-simplification
    /// </summary>
    public class PolylineSimplication
    {
        /// <summary>
        /// Simplification by using Douglas-Peucker algorithm
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<GPoint> SimplifyByDouglasPeucker(GPoint[] points, double tolerance)
        {
            if (points.Length < 3) return points.ToList();

            double tol2 = tolerance * tolerance;    // squared distance tolerance

            int pointCount = points.Length;
            bool[] keys = new bool[pointCount];
            keys[0] = true;                         // the first point is always a key
            keys[pointCount - 1] = true;            // the last point is always a key

            Stack<SubPoly> stack = new Stack<SubPoly>(); // LIFO job-queue containing sub-polylines
            SubPoly subPoly = new SubPoly(0, pointCount - 1);
            stack.Push(subPoly);                    // add complete poly

            while (stack.Count > 0)
            {
                subPoly = stack.Pop();              // take a sub poly and remove it
                KeyInfo keyInfo = FindKey(points, subPoly.First, subPoly.Last);
                if ((keyInfo.Index > 0) && (tol2 < keyInfo.Dist2))
                {
                    // store the key if valid
                    keys[keyInfo.Index] = true;
                    // split the polyline at the key and recurse
                    stack.Push(new SubPoly(keyInfo.Index, subPoly.Last));
                    stack.Push(new SubPoly(subPoly.First, keyInfo.Index));
                }
            }

            return ExtractPoints(points, keys);
        }

        /// <summary>
        /// Simplification by using Douglas-Peucker algorithm
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<GPoint> SimplifyByDouglasPeuckerAnother(GPoint[] points, double tolerance)
        {
            if (points == null || points.Length == 0)
                return new List<GPoint>();

            double sqTolerance = tolerance * tolerance;
            var len = points.Length;
            var markers = new int?[len];
            int? first = 0;
            int? last = len - 1;
            int? index = 0;
            var stack = new List<int?>();
            var newPoints = new List<GPoint>();

            markers[first.Value] = markers[last.Value] = 1;

            while (last != null)
            {
                var maxSqDist = 0.0d;

                for (int? i = first + 1; i < last; i++)
                {
                    var sqDist = points[i.Value].SquareDistance2Segment(points[first.Value], points[last.Value]);

                    if (sqDist > maxSqDist)
                    {
                        index = i;
                        maxSqDist = sqDist;
                    }
                }

                if (maxSqDist > sqTolerance)
                {
                    markers[index.Value] = 1;
                    stack.AddRange(new[] { first, index, index, last });
                }


                if (stack.Count > 0)
                {
                    last = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                    last = null;

                if (stack.Count > 0)
                {
                    first = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                    first = null;
            }

            for (var i = 0; i < len; i++)
            {
                if (markers[i] != null)
                    newPoints.Add(points[i]);
            }

            return newPoints;
        }

        /// <summary>
        /// Simplification by using Douglas-Peucker variant algorithm
        /// </summary>
        /// <param name="points">input points</param>
        /// <param name="maxPoints">the maximum number of points of the simplified polyline</param>
        /// <returns>simplified polyline</returns>
        public static List<GPoint> SimplifyByDouglasPeuckerN(GPoint[] points, int maxPoints)
        {
            int pointCount = points.Length;
            if (maxPoints >= pointCount)
            {
                return points.ToList();
            }

            if (maxPoints == 2)
            {
                return new List<GPoint> { points.First(), points.Last() };
            }

            bool[] keys = SimplifyByDouglasPeuckerNHelp(points, maxPoints);
            return ExtractPoints(points, keys);
        }


        /// <summary>
        /// Simplification by using Douglas-Peucker variant algorithm
        /// </summary>
        /// <param name="points">input points</param>
        /// <param name="maxPoints">the maximum number of points of the simplified polyline</param>
        /// <returns>simplified polyline</returns>
        private static bool[] SimplifyByDouglasPeuckerNHelp(GPoint[] points, int maxPoints)
        {
            int pointCount = points.Length;
            bool[] keys = new bool[pointCount];
            keys[0] = true;                 // the first point is always a key
            keys[pointCount - 1] = true;    // the last point is always a key
            int keyCount = 2;

            PriorityQueue<SubPolyAlt> queue = new PriorityQueue<SubPolyAlt>();
            SubPolyAlt subPoly = new SubPolyAlt(0, pointCount - 1);
            subPoly.KeyInfo = FindKey(points, subPoly.First, subPoly.Last);
            queue.Push(subPoly);           // add complete poly

            while (!queue.IsEmpty())
            {
                subPoly = queue.Top();     // take a sub poly
                queue.Pop();
                // store the key
                keys[subPoly.KeyInfo.Index] = true;
                // check point count tolerance
                keyCount++;
                if (keyCount == maxPoints)
                {
                    break;
                }
                // split the polyline at the key and recurse
                SubPolyAlt left = new SubPolyAlt(subPoly.First, subPoly.KeyInfo.Index);
                left.KeyInfo = FindKey(points, left.First, left.Last);
                if (left.KeyInfo.Index > 0)
                {
                    queue.Push(left);
                }
                SubPolyAlt right = new SubPolyAlt(subPoly.KeyInfo.Index, subPoly.Last);
                right.KeyInfo = FindKey(points, right.First, right.Last);
                if (right.KeyInfo.Index > 0)
                {
                    queue.Push(right);
                }
            }

            return keys;
        }

        private static List<GPoint> ExtractPoints(GPoint[] points, bool[] keys)
        {
            List<GPoint> newPoints = new List<GPoint>();
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i]) newPoints.Add(points[i]);
            }
            return newPoints;
        }

        /// <summary>
        /// Finds the key for the given sub polyline.
        /// Finds the point in the range [first, last] that is furthest away from the
        /// segment (first, last). This point is called the key.
        /// </summary>
        /// <param name="points">polyline points</param>
        /// <param name="first">the first polyline point</param>
        /// <param name="last">the last polyline point</param>
        /// <returns>the index of the key and its distance, or last when a key could not be found</returns>
        private static KeyInfo FindKey(GPoint[] points, int first, int last)
        {
            KeyInfo keyInfo = new KeyInfo();
            for (int current = first + 1; current < last; current++)
            {
                double sqDist = points[current].SquareDistance2Segment(points[first], points[last]);
                if (sqDist < keyInfo.Dist2)
                {
                    continue;
                }
                // update maximum squared distance and the point it belongs to
                keyInfo.Index = current;
                keyInfo.Dist2 = sqDist;
            }
            return keyInfo;
        }

        /// <summary>
        /// Simplifying by using point weight, which is curve distance between points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="weightTolerance">minimum distance difference of two points</param>
        /// <returns></returns>
        public static List<GPoint> SimplifyByWeight(GPoint[] points, double weightTolerance)
        {
            if (points == null || points.Length == 0)
                return new List<GPoint>();

            List<GPoint> newPoints = new List<GPoint>();
            newPoints.Add(points[0]);
            GPoint keyPoint = points[0];
            for (int i = 1; i < points.Length - 1; i++)
            {
                if (points[i].W - keyPoint.W > weightTolerance)
                {
                    newPoints.Add(points[i]);
                    keyPoint = points[i];
                }
            }
            newPoints.Add(points.Last());

            return newPoints;
        }

        /// <summary>
        /// Simplifying by using slope direction changing.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="yTolerance">y value is considered zero within the tolerance </param>
        /// <returns></returns>
        public static List<GPoint> SimplifyHeadingChange(GPoint[] points, double yTolerance)
        {
            if (points.Length < 2)
            {
                return points.ToList();
            }

            List<GPoint> newPoints = new List<GPoint>();
            newPoints.Add(points[0]);
            for (int i = 1; i < points.Length - 1; i++)
            {
                int predir = Direction(points[i - 1], points[i], yTolerance);
                int nextdir = Direction(points[i], points[i + 1], yTolerance);
                if (predir != nextdir)
                {
                    newPoints.Add(points[i]);
                }
            }
            newPoints.Add(points.Last());

            return newPoints;
        }

        private static int Direction(GPoint from, GPoint to, double yTolerance)
        {
            double yDiff = to.Y - from.Y;
            return (Math.Abs(yDiff) < yTolerance) ? 0 : Math.Sign(yDiff);
        }

        /// <summary>
        /// Remove points where |p.Y| < yTolerance
        /// </summary>
        /// <param name="points">Points original list of points</param>
        /// <param name="yTolerance">y value tolerance</param>
        /// <returns></returns>
        public static List<GPoint> SimplifyByYTolerance(GPoint[] points, double yTolerance)
        {
            List<GPoint> newPoints = new List<GPoint>();
            if ((points == null) || (points.Length == 0)) return newPoints;

            for (int i = 0; i < points.Length; i++)
            {

                if (Math.Abs(points[i].Y) > yTolerance)
                {
                    newPoints.Add(points[i]);
                }
            }

            return newPoints;
        }

        /// <summary>
        /// 一定区間の抽出
        /// </summary>
        /// <param name="points">Points original list of points</param>
        /// <param name="yTolerance">y value tolerance</param>
        /// <returns></returns>
        public static List<GPoint> ExtractSameRanges(GPoint[] points, double yTolerance)
        {
            List<GPoint> newPoints = new List<GPoint>();
            if (points == null) return newPoints;

            int index = 0;
            while (index < points.Length - 1)
            {
                if (Math.Abs(points[index + 1].Y - points[index].Y) <= yTolerance)
                {
                    newPoints.Add(points[index]);
                    newPoints.Add(points[index + 1]);
                    index += 2;
                }
                else
                {
                    index++;
                }
            }

            return newPoints;
        }

        /// <summary>
        /// Simplifiy points so that the result is not more than maxPoints + 2.
        /// Remark: First and last points will be kept always
        /// </summary>
        /// <param name="points">input points</param>
        /// <param name="maxPoints">maxium points of remaining</param>
        /// <returns></returns>
        public static List<GPoint> SimplifyPointsN(GPoint[] points, int maxPoints)
        {
            List<GPoint> newPoints = new List<GPoint>();
            int TOTAL = points.Length;

            if (TOTAL <= maxPoints)
            {
                newPoints.AddRange(points);
                return newPoints;
            }

            float interval = TOTAL / (float)maxPoints;
            int lastIndex = -1;
            for (float i = 0; i < TOTAL - 1; i += interval)
            {
                int index = (int)i;
                if (index != lastIndex)
                {
                    newPoints.Add(points[index]);
                    lastIndex = index;
                }
            }
            newPoints.Add(points.Last());

            return newPoints;
        }

        /// <summary>
        /// Simplification by Radial Distance
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<GPoint> SimplifyByRadialDistance(GPoint[] points, double tolerance)
        {
            double sqTolerance = tolerance * tolerance;
            var prevPoint = points[0];
            var newPoints = new List<GPoint> { prevPoint };
            GPoint point = null;

            for (var i = 1; i < points.Length; i++)
            {
                point = points[i];

                if (point.SquareDistance(prevPoint) > sqTolerance)
                {
                    newPoints.Add(point);
                    prevPoint = point;
                }
            }

            if (point != null && !prevPoint.Equals(point))
                newPoints.Add(point);

            return newPoints;
        }
    }
}
