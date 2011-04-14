using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EddyTest
{
    [TestClass]
    public class OneShotEventTest
    {
        public OneShotEventTest()
        {
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
