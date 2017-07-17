using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gherkin.Util;

namespace UnitTestProject
{
    public interface ITestInterface
    {
        int Value();
    }

    class A : ITestInterface
    {
        public int Value() => 10;
    }

    enum MyEnum { A, B }

    class ConcreteB
    {
        int m_value;
        string m_str;
        MyEnum m_MyEnum;

        public ConcreteB(int v, string str, MyEnum @enum)
        {
            m_value = v;
            m_str = str;
            m_MyEnum = @enum;
        }

        public int IntValue
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public string StrValue
        {
            get { return m_str; }
            set { m_str = value; }
        }

        public MyEnum EnumValue
        {
            get { return m_MyEnum; }
            set { m_MyEnum = value; }
        }
    }

    class C
    {
        public ITestInterface TestObject { get; private set; }
        public ConcreteB ConcreteBObject { get; private set; }

        public C(ITestInterface test, ConcreteB b)
        {
            TestObject = test;
            ConcreteBObject = b;
        }
    }

    class D : C
    {
        int m_value;

        public D(ITestInterface test, ConcreteB b, int v) : base(test, b)
        {
            m_value = v;
        }

        public int IntValue
        {
            get { return m_value; }
            set { m_value = value; }
        }
    }

    class E
    {
        private E() { }
    }

    class F
    {
    }

    [TestClass]
    public class DIContainerTest
    {
        [TestInitialize]
        public void Setup()
        {
        }

        [TestMethod]
        public void ResoveInterface()
        {
            // Given
            DIContainer container = new DIContainer();
            container.Register<ITestInterface, A>();

            // When
            ITestInterface obj = container.Resolve<ITestInterface>();

            // Then
            Assert.IsTrue(obj is A);
            Assert.AreEqual(10, obj.Value());
        }

        [TestMethod]
        public void ResoveConcreteClass()
        {
            // Given
            DIContainer container = new DIContainer();
            container.Register<ConcreteB, ConcreteB>();

            // When
            ConcreteB obj = container.Resolve<ConcreteB>();

            // Then
            Assert.IsNotNull(obj);
            Assert.AreEqual(0, obj.IntValue);
            Assert.IsNull(obj.StrValue);
            Assert.AreEqual(MyEnum.A, obj.EnumValue);
        }

        [TestMethod]
        public void ResoveObjectWithDependency()
        {
            // Given
            DIContainer container = new DIContainer();
            container.Register<ITestInterface, A>();

            // When
            var objC = container.Resolve<C>();

            // Then
            Assert.IsNotNull(objC);
            Assert.IsNotNull(objC.TestObject);
            Assert.IsNotNull(objC.ConcreteBObject);
        }

        [TestMethod]
        public void ResoveSubClassWithDependency()
        {
            // Given
            DIContainer container = new DIContainer();
            container.Register<ITestInterface, A>();

            // When
            var objD = container.Resolve<D>();

            // Then
            Assert.IsNotNull(objD);
            Assert.IsNotNull(objD.TestObject);
            Assert.IsNotNull(objD.ConcreteBObject);
            Assert.AreEqual(0, objD.IntValue);
        }

        [TestMethod]
        public void ResoveTwiceTest()
        {
            // Given
            DIContainer container = new DIContainer();
            container.Register<ITestInterface, A>();

            // When
            var objC1 = container.Resolve<C>();
            var objC2 = container.Resolve<C>();
            ConcreteB obj1 = container.Resolve<ConcreteB>();
            ConcreteB obj2 = container.Resolve<ConcreteB>();

            // Then
            Assert.IsNotNull(objC1);
            Assert.AreEqual(objC1, objC2);

            Assert.IsNotNull(obj1);
            Assert.AreEqual(obj1, obj2);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ClassWithoutPublicConstructorTest()
        {
            // Given
            DIContainer container = new DIContainer();

            // When
            var obj = container.Resolve<E>();

            // Then
        }

        [TestMethod]
        public void ResoveClassWithDefaultConstructorTest()
        {
            // Given
            DIContainer container = new DIContainer();

            // When
            var obj = container.Resolve<F>();

            // Then
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void ResoveRegisteredClassTest()
        {
            // Given
            DIContainer container = new DIContainer();
            container.Register<ITestInterface, A>();
            container.Register<C, D>();

            // When
            var obj = container.Resolve<C>();

            // Then
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj is D);
        }

        [TestMethod]
        public void SimpleTypeTest()
        {
            // Given

            // When

            // Then
            Assert.IsTrue(Util.IsSimpleType(typeof(string)));
            Assert.IsTrue(Util.IsSimpleType(typeof(int)));
            Assert.IsTrue(Util.IsSimpleType(typeof(decimal)));
            Assert.IsTrue(Util.IsSimpleType(typeof(float)));
            Assert.IsTrue(Util.IsSimpleType(typeof(StringComparison)));  // enum
            Assert.IsTrue(Util.IsSimpleType(typeof(int?)));
            Assert.IsTrue(Util.IsSimpleType(typeof(decimal?)));
            Assert.IsTrue(Util.IsSimpleType(typeof(StringComparison?)));
            Assert.IsFalse(Util.IsSimpleType(typeof(object)));
            Assert.IsFalse(Util.IsSimpleType(typeof(System.Text.StringBuilder))); // reference type
        }
    }
}
