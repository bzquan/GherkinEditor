using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Gherkin.Util.Geometric;

namespace UnitTestProject
{
    [TestClass]
    public class SimplifyByDouglasPeuckerNTest
    {
        List<GPoint> points;

        [TestInitialize]
        public void Setup()
        {
            points = new List<GPoint> {
                new GPoint(1, 1),
                new GPoint(2, 2),
                new GPoint(4, 4),
                new GPoint(6, 6),
                new GPoint(8, 8),
                new GPoint(3, 3),
                new GPoint(5, 5),
                new GPoint(7, 7),
                new GPoint(9, 9),
                new GPoint(10, 10),
            };
        }

        [TestMethod]
        public void TestDouglasPeuckerN_Total3()
        {
            // Given

            // When
            List<GPoint> newPoints = PolylineSimplication.SimplifyByDouglasPeuckerN(points.ToArray(), 3);

            // Then
            Assert.AreEqual(3, newPoints.Count);
        }

        [TestMethod]
        public void TestDouglasPeuckerN_Total5()
        {
            // Given

            // When
            List<GPoint> newPoints = PolylineSimplication.SimplifyByDouglasPeuckerN(points.ToArray(), 5);

            // Then
            Assert.AreEqual(5, newPoints.Count);
        }
    }
}
