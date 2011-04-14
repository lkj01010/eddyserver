using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eddy.Timers;
using Eddy;

namespace EddyTest
{
    /// <summary>
    /// Summary description for SlotTimerTest
    /// </summary>
    [TestClass]
    public class SlotTimerTest
    {
        public SlotTimerTest()
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
        // public void MyTestCleanup() {}
        //
        #endregion


        [TestMethod]
        public void TestSlotTimer()
        {
            var timer = new SlotTimer();
            var startTime = DateTimeProvider.Now;
            var count = 0;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += () =>
                {
                    var timespan = (DateTimeProvider.Now - startTime);
                    Assert.IsTrue(Math.Abs((timespan - timer.Interval).TotalMilliseconds) < 50);
                    startTime = DateTimeProvider.Now;
                    ++count;
                    if (count == 3)
                    {
                        timer.Stop();
                        SimpleDispatcher.CurrentDispatcher.Shutdown();
                    }
                };
            timer.Start();
            SimpleDispatcher.CurrentDispatcher.Run();
            Assert.AreEqual(3, count);
        }
    }
}
