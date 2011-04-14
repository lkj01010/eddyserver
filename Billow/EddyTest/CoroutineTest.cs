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
    [TestClass]
    public class CoroutineTest : Executor
    {
        public CoroutineTest()
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
            Assert.AreEqual(0, GetControllerCount());
            count = 0; 
        }

        OneShotEvent event1 = new OneShotEvent();
        OneShotEvent<int> event2 = new OneShotEvent<int>();
        int count = 0;

        private IEnumerable<Waiter> DoSomething()
        {
            count += 1;
            yield return WaitForOneShotEvent(event1);
            count += 2;
            var extractor = new ValueExtractor<int>();
            yield return WaitForOneShotEvent<int>(event2, extractor);
            Assert.AreEqual(extractor.Value, 99);
            count += 3;
        }

        private IEnumerable<Waiter> CombineCoroutine()
        {
            var extractor = new ValueExtractor<int>();
            yield return WaitForAny(
                WaitForOneShotEvent(event1),
                WaitForOneShotEvent<int>(event2, extractor));
            if (extractor.Value == 0)
                count += 1;
            else
                count += extractor.Value;
        }

        [TestMethod]
        public void TestOneShotEvent()
        {
            StartCoroutine(DoSomething());
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
        }

        [TestMethod]
        public void TestCombiner()
        {
            StartCoroutine(CombineCoroutine());
            event1.Raise();
            Assert.AreEqual(1, count);
            StartCoroutine(CombineCoroutine());
            event2.Raise(99);
            Assert.AreEqual(100, count);
        }

        private IEnumerable<Waiter> SubChainCoroutine()
        {
            count += 2;
            yield return WaitForOneShotEvent(event1);
            count += 3;
        }

        private IEnumerable<Waiter> ChainCoroutine()
        {
            yield return WaitForCoroutine(SubChainCoroutine());
            count += 4;
        }

        [TestMethod]
        public void TestChain()
        {
            StartCoroutine(ChainCoroutine());
            event1.Raise();
            Assert.AreEqual(9, count);
        }

        private IEnumerable<Waiter> IndexedChainCoroutine(ValueExtractor<int> index)
        {
            yield return WaitForAny(index,
                WaitForOneShotEvent(event1),
                WaitForOneShotEvent(event2, new ValueExtractor<int>()));
        }

        [TestMethod]
        public void TestIndexedCombine()
        {
            var index = ExtractValue<int>();
            StartCoroutine(IndexedChainCoroutine(index));
            event1.Raise();
            Assert.AreEqual(0, index.Value);

            StartCoroutine(IndexedChainCoroutine(index));
            event2.Raise(0);
            Assert.AreEqual(1, index.Value);
        }
    }
}
