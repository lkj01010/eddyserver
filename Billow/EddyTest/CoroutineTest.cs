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
        OneShotEvent<CoroutineTest> event3 = new OneShotEvent<CoroutineTest>();
        int count = 0;

        private IEnumerable<Waiter> DoSomething()
        {
            count += 1;
            yield return WaitForOneShotEvent(event1);
            count += 2;
            int value = 0;
            yield return WaitForOneShotEvent(event2, (x) => value = x);
            Assert.AreEqual(value, 99);
            count += 3;
            CoroutineTest test = null;
            yield return WaitForOneShotEvent(event3, (x) => test = x);
            count += 4;
            Assert.AreSame(this, test);
        }

        private IEnumerable<Waiter> CombineCoroutine()
        {
            int value = 0;
            yield return WaitForAny(
                WaitForOneShotEvent(event1),
                WaitForOneShotEvent<int>(event2, (x) => value = x));
            if (value == 0)
                count += 1;
            else
                count += value;
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
            event3.Raise(this);
            Assert.AreEqual(10, count);

            // test if auto removed
            event1.Raise();
            Assert.AreEqual(10, count);
            event2.Raise(98);
            Assert.AreEqual(10, count);
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

        private IEnumerable<Waiter> IndexedChainCoroutine(Action<int> indexGetter)
        {
            yield return WaitForAny(indexGetter,
                WaitForOneShotEvent(event1),
                WaitForOneShotEvent(event2, null));
        }

        [TestMethod]
        public void TestIndexedCombine()
        {
            int index = 0;
            StartCoroutine(IndexedChainCoroutine((x) => index = x));
            event1.Raise();
            Assert.AreEqual(0, index);

            StartCoroutine(IndexedChainCoroutine((x) => index = x));
            event2.Raise(0);
            Assert.AreEqual(1, index);
        }
    }
}
