using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eddy.Timers;
using Eddy;

namespace EddyTest
{
    [TestClass]
    public class TimerTest
    {
        public TimerTest()
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

        [TestMethod]
        public void TestSlotTimer()
        {
            var timer = new Timer();
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
