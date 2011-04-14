using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eddy;
using Eddy.Timers;

namespace EddyTest
{
    [TestClass]
    public class SimpleDispatcherTest
    {
        public SimpleDispatcherTest()
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

        [TestCleanup()]
        public void MyTestCleanup()
        {
            count = 0;
        }

        int count;

        [TestMethod]
        public void TestSimpleDispatcher()
        {
            var dispatcher = SimpleDispatcher.CurrentDispatcher;
            dispatcher.Invoke(
                () =>
                {
                    count += 1;
                    dispatcher.Invoke(() =>
                        {
                            count += 2;
                            dispatcher.Shutdown();
                        });
                });
            dispatcher.Run();
            Assert.AreEqual(3, count);
        }
    }
}
