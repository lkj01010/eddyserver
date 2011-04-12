using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EddyTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class OneShotEventTest
    {
        public OneShotEventTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        static int count = 0;
        static Eddy.OneShotEvent<int> ev = new Eddy.OneShotEvent<int>();

        [TestMethod]
        public void TestAdd()
        {
            ev.Add((x) => count += x);
            ev.Raise(5);
            Assert.AreEqual(count, 5);
            count = 0;
        }

        [TestMethod]
        public void TestRemove()
        {
            Action<int> action = (x) => count += x;
            ev.Add(action);
            ev.Remove(action);
            ev.Raise(5);
            Assert.AreEqual(count, 0);
            count = 0;
        }

        [TestMethod]
        public void TestOneShot()
        {
            Action<int> action = (x) => count += x;
            ev.Add(action);
            ev.Raise(5);
            ev.Raise(7);
            Assert.AreEqual(count, 5);
            count = 0;
        }
    }
}
