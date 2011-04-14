using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eddy;
using Eddy.Timers;

namespace EddyTest
{
    /// <summary>
    /// Summary description for SimpleDispatcherTest
    /// </summary>
    [TestClass]
    public class SimpleDispatcherTest
    {
        public SimpleDispatcherTest()
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
         [TestCleanup()]
        public void MyTestCleanup() { count = 0;  }
        
        #endregion

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
