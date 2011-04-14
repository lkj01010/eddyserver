using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eddy;
using Eddy.Coroutine;
using Eddy.Timers;

namespace EddyTest
{
    /// <summary>
    /// Summary description for CoroutineTest
    /// </summary>
    [TestClass]
    public class CoroutineTest
    {
        public CoroutineTest()
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

        OneShotEvent event1 = new OneShotEvent();
        OneShotEvent<int> event2 = new OneShotEvent<int>();
        Executor executor = new Executor();
        int count = 0;

        private IEnumerable<Waiter> DoSomething()
        {
            count += 1;
            yield return executor.WaitForOneShotEvent(event1);
            count += 2;
            var extractor = new ValueExtractor<int>();
            yield return executor.WaitForOneShotEvent<int>(event2, extractor);
            Assert.AreEqual(extractor.Value, 99);
            count += 3;
        }

        private IEnumerable<Waiter> CombineCoroutine()
        {
            var extractor = new ValueExtractor<int>();
            yield return executor.WaitForAny(
                executor.WaitForOneShotEvent(event1),
                executor.WaitForOneShotEvent<int>(event2, extractor));
            if (extractor.Value == 0)
                count += 1;
            else
                count += extractor.Value;
        }

        [TestMethod]
        public void TestOneShotEvent()
        {
            executor.StartCoroutine(DoSomething());
            Assert.AreEqual(1, count);
            event1.Raise();
            Assert.AreEqual(3, count);
            event2.Raise(99);
            Assert.AreEqual(6, count);
            event1.Raise();

            // test if auto removed
            Assert.AreEqual(6, count);
            event2.Raise(98);
            Assert.AreEqual(6, count);
            count = 0;
        }

        [TestMethod]
        public void TestCombiner()
        {
            executor.StartCoroutine(CombineCoroutine());
            event1.Raise();
            Assert.AreEqual(1, count);
            executor.StartCoroutine(CombineCoroutine());
            event2.Raise(99);
            Assert.AreEqual(100, count);
        }

        private IEnumerable<Waiter> SubChainCoroutine()
        {
            count += 2;
            yield return executor.WaitForOneShotEvent(event1);
            count += 3;
        }

        private IEnumerable<Waiter> ChainCoroutine()
        {
            yield return executor.WaitForCoroutine(SubChainCoroutine());
            count += 4;
        }

        [TestMethod]
        public void TestChain()
        {
            executor.StartCoroutine(ChainCoroutine());
            event1.Raise();
            Assert.AreEqual(9, count);
            count = 0;
        }

        private IEnumerable<Waiter> IndexedChainCoroutine(ValueExtractor<int> index)
        {
            yield return executor.WaitForAny(index,
                executor.WaitForOneShotEvent(event1),
                executor.WaitForOneShotEvent(event2, new ValueExtractor<int>()));
        }

        [TestMethod]
        public void TestIndexedCombine()
        {
            var index = Executor.ExtractValue<int>();
            executor.StartCoroutine(IndexedChainCoroutine(index));
            event1.Raise();
            Assert.AreEqual(0, index.Value);

            executor.StartCoroutine(IndexedChainCoroutine(index));
            event2.Raise(0);
            Assert.AreEqual(1, index.Value);
        }
    }
}
