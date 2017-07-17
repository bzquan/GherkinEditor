using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gherkin.Util;

namespace UnitTestProject
{
    [TestClass]
    public class SortedListTest
    {
        [TestMethod]
        public void TestSortedListPush()
        {
            // Given
            PriorityQueue<int> queue = new PriorityQueue<int>();

            // When
            queue.Push(3);
            queue.Push(1);
            queue.Push(5);

            // Then
            Assert.AreEqual(1, queue[0]);
            Assert.AreEqual(3, queue[1]);
            Assert.AreEqual(5, queue[2]);
        }

        [TestMethod]
        public void TestSortedListPop()
        {
            // Given
            PriorityQueue<int> queue = new PriorityQueue<int>();
            queue.Push(3);
            queue.Push(1);
            queue.Push(5);

            // When
            int v = queue.Top();
            queue.Pop();

            // Then
            Assert.AreEqual(5, v);
            Assert.AreEqual(1, queue[0]);
            Assert.AreEqual(3, queue[1]);
        }
    }
}
