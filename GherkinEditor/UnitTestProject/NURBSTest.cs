using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Gherkin.Util.Geometric;
using Gherkin.Util.NURBS;
using Gherkin.Util.Bezier;

namespace UnitTestProject
{
    [TestClass]
    public class NURBSTest
    {
        [TestMethod]
        public void FindSpanTest()
        {
            int p = 2;
            double[] U = { 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 5 };
            int span = NURBS.FindSpan(p, 5.0 / 2.0, U);
            Assert.AreEqual(4, span);

            span = NURBS.FindSpan(p, 4.0, U);
            Assert.AreEqual(7, span);

            span = NURBS.FindSpan(p, 4.5, U);
            Assert.AreEqual(7, span);
        }

        [TestMethod]
        public void BasisFunsTest0()
        {
            // page 71
            double[] U = { 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 5 };
            int degree = 0;
            double[] N = NURBS.BasisFuns(4, degree, U, 5.0 / 2.0);
            Assert.AreEqual(1.0, N[0]);
        }

        [TestMethod]
        public void BasisFunsTest1()
        {
            // page 71
            double[] U = { 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 5 };
            int degree = 1;
            double[] N = NURBS.BasisFuns(4, degree, U, 5.0 / 2.0);
            Assert.AreEqual(1.0 / 2, N[0]);
            Assert.AreEqual(1.0 / 2, N[1]);
        }

        [TestMethod]
        public void NipTest1()
        {
            double[] U = { 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 5 };
            int degree = 1;
            double N3_1 = NURBS.Nip(3, degree, U, 5.0 / 2.0);
            Assert.AreEqual(1.0 / 2, N3_1);
            double N4_1 = NURBS.Nip(4, degree, U, 5.0 / 2.0);
            Assert.AreEqual(1.0 / 2, N4_1);
        }

        [TestMethod]
        public void BasisFunsTest2()
        {
            // page 71
            double[] U = { 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 5 };
            int degree = 2;
            double[] N = NURBS.BasisFuns(4, degree, U, 5.0 / 2.0);
            Assert.AreEqual(1.0 / 8, N[0]);
            Assert.AreEqual(6.0 / 8, N[1]);
            Assert.AreEqual(1.0 / 8, N[2]);
        }

        [TestMethod]
        public void NipTest2()
        {
            double[] U = { 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 5 };
            int degree = 2;
            double N2_2 = NURBS.Nip(2, degree, U, 5.0 / 2.0);
            Assert.AreEqual(1.0 / 8, N2_2);
            double N3_2 = NURBS.Nip(3, degree, U, 5.0 / 2.0);
            Assert.AreEqual(6.0 / 8, N3_2);
            double N4_2 = NURBS.Nip(4, degree, U, 5.0 / 2.0);
            Assert.AreEqual(1.0 / 8, N4_2);
        }

        [TestMethod]
        public void DerBasisFunsTest()
        {
            double[] U = { 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 5 };
            int degree = 2;
            double[][] ders = NURBS.DerBasisFuns(4, 5.0 / 2.0, degree, n: 1, U: U);
            Assert.AreEqual(-1.0 / 2, ders[1][0]);
            Assert.AreEqual(0, ders[1][1]);
            Assert.AreEqual(1.0 / 2, ders[1][2]);
        }

        [TestMethod]
        public void BezierCurveLength3DQuadraticTest()
        {
            // Quadratic 3D Bézier curve
            // difference between calculated and interpolated lengts are less than 1%
            var curves = new BezierCurveLength3DQuadratic[] {
                new BezierCurveLength3DQuadratic(new V3D(0, 0, 0), new V3D(2, 0, 0), new V3D(1, 0, 0)),
                new BezierCurveLength3DQuadratic(new V3D(0, 0, 0), new V3D(0, 2, 0), new V3D(0, 1, 0)),
                new BezierCurveLength3DQuadratic(new V3D(0, 0, 0), new V3D(0, 0, 2), new V3D(0, 0, 1)),
                new BezierCurveLength3DQuadratic(new V3D(0, 0, 0), new V3D(2, 2, 2), new V3D(1, 1, 1)),
                new BezierCurveLength3DQuadratic(new V3D(0, 0, 0), new V3D(-1, -1, -1), new V3D(1, 1, 1)),
            };
            foreach (var curve in curves)
            {
                var error = Math.Abs(curve.Length - curve.InterpolatedLength);
                Assert.AreEqual(true, error < 0.01);
            }
        }

        [TestMethod]
        public void BezierCurveLength3DCubicTest()
        {
            // Cubic Bézier curve length
            var test = new V3D[] {
                new V3D(-21298.4, 0.2, 2627.51),
                new V3D(-11.3359, 0.0, 0.0),
                new V3D(11.2637, 0.0, -1.28198),
                new V3D(-21332.3, 0.2, 2629.43)
            };
            var testCurve = new BezierCurveLength3DCubic(test[0], test[0] + test[1], test[2] + test[3], test[3]);
            double length = testCurve.Length;
            double interpolatedLength = testCurve.InterpolatedLength;
            Assert.AreEqual(true, Math.Abs(33.972527859609976 - length) < 1.0E-10);
            Assert.AreEqual(true, Math.Abs(length - interpolatedLength) < 0.01);
        }

        [TestMethod]
        public void PolylineSimplication_thinout2HalfTest()
        {
            // Given
            int TOTAL = 10;
            List<GPoint> origPoints = new List<GPoint>(TOTAL);
            for (int i = 0; i < TOTAL; i++)
            {
                origPoints.Add(new GPoint(i));
            }

            // When
            int maxPoints = TOTAL / 2;
            List<GPoint> newPoints = PolylineSimplication.SimplifyPointsN(origPoints.ToArray(), maxPoints);

            // Then
            Assert.IsTrue(Math.Abs(maxPoints - newPoints.Count) < 2);
            Assert.AreEqual(0, newPoints[0].X);
            Assert.AreEqual(9, newPoints.Last().X);

            Assert.AreEqual(2, newPoints[1].X);
            Assert.AreEqual(4, newPoints[2].X);
            Assert.AreEqual(6, newPoints[3].X);
            Assert.AreEqual(8, newPoints[4].X);
        }

        [TestMethod]
        public void PolylineSimplication_thinoutOneThird()
        {
            // Given
            int TOTAL = 10;
            List<GPoint> origPoints = new List<GPoint>(TOTAL);
            for (int i = 0; i < TOTAL; i++)
            {
                origPoints.Add(new GPoint(i));
            }

            // When
            int maxPoints = 7;
            List<GPoint> newPoints = PolylineSimplication.SimplifyPointsN(origPoints.ToArray(), maxPoints);

            // Then
            Assert.AreEqual(true, Math.Abs(maxPoints - newPoints.Count) < 2);
            Assert.AreEqual(0, newPoints[0].X);
            Assert.AreEqual(9, newPoints.Last().X);

            Assert.AreEqual(5, newPoints[4].X);
            Assert.AreEqual(8, newPoints[6].X);
        }

        [TestMethod]
        public void PolylineSimplication_thinoutTwoThird()
        {
            // Given
            int TOTAL = 10;
            List<GPoint> origPoints = new List<GPoint>(TOTAL);
            for (int i = 0; i < TOTAL; i++)
            {
                origPoints.Add(new GPoint(i));
            }

            // When
            int maxPoints = 3;
            List<GPoint> newPoints = PolylineSimplication.SimplifyPointsN(origPoints.ToArray(), maxPoints);

            // Then
            Assert.AreEqual(true, Math.Abs(maxPoints - newPoints.Count) < 2);
            Assert.AreEqual(0, newPoints[0].X);
            Assert.AreEqual(9, newPoints.Last().X);

            Assert.AreEqual(3, newPoints[1].X);
            Assert.AreEqual(6, newPoints[2].X);
        }
    }
}