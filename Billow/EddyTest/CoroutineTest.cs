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
            Count = 0; 
        }

        OneShotEvent OneShotNoArgEvent = new OneShotEvent();
        OneShotEvent<int> OneShotIntArgEvent = new OneShotEvent<int>();
        OneShotEvent<CoroutineTest> OneShotClassArgEvent = new OneShotEvent<CoroutineTest>();
        int Count { get; set; }

        private IEnumerable<Waiter> OneShotEventCoroutine()
        {
            Count += 1;
            yield return WaitForOneShotEvent(OneShotNoArgEvent);
            Count += 2;
            int value = 0;
            yield return WaitForOneShotEvent(OneShotIntArgEvent, (x) => value = x);
            Assert.AreEqual(value, 99);
            Count += 3;
            CoroutineTest test = null;
            yield return WaitForOneShotEvent(OneShotClassArgEvent, (x) => test = x);
            Count += 4;
            Assert.AreSame(this, test);
        }

        private IEnumerable<Waiter> CombineCoroutine()
        {
            int value = 0;
            yield return WaitForAny(
                WaitForOneShotEvent(OneShotNoArgEvent),
                WaitForOneShotEvent<int>(OneShotIntArgEvent, (x) => value = x));
            if (value == 0)
                Count += 1;
            else
                Count += value;
        }

        [TestMethod]
        public void TestOneShotEvent()
        {
            StartCoroutine(OneShotEventCoroutine());
            Assert.AreEqual(1, Count);
            OneShotNoArgEvent.Raise();
            Assert.AreEqual(3, Count);
            OneShotIntArgEvent.Raise(99);
            Assert.AreEqual(6, Count);
            OneShotClassArgEvent.Raise(this);
            Assert.AreEqual(10, Count);

            // test if auto removed
            OneShotNoArgEvent.Raise();
            Assert.AreEqual(10, Count);
            OneShotIntArgEvent.Raise(98);
            Assert.AreEqual(10, Count);
        }

        [TestMethod]
        public void TestCombiner()
        {
            StartCoroutine(CombineCoroutine());
            OneShotNoArgEvent.Raise();
            Assert.AreEqual(1, Count);

            StartCoroutine(CombineCoroutine());
            OneShotIntArgEvent.Raise(99);
            Assert.AreEqual(100, Count);

            var controller = StartCoroutine(CombineCoroutine());
            controller.Stop();
            OneShotNoArgEvent.Raise();
            Assert.AreEqual(100, Count);
        }

        private IEnumerable<Waiter> SubChainCoroutine()
        {
            Count += 2;
            yield return WaitForOneShotEvent(OneShotNoArgEvent);
            Count += 3;
            yield return WaitForOneShotEvent(OneShotNoArgEvent);
        }

        private IEnumerable<Waiter> ChainCoroutine()
        {
            yield return WaitForCoroutine(SubChainCoroutine());
            Count += 4;
        }

        [TestMethod]
        public void TestChain()
        {
            StartCoroutine(ChainCoroutine());
            OneShotNoArgEvent.Raise();
            OneShotNoArgEvent.Raise();
            Assert.AreEqual(9, Count);

            // test stop
            Count = 0;
            var controller = StartCoroutine(ChainCoroutine());
            OneShotNoArgEvent.Raise();
            controller.Stop();
            OneShotNoArgEvent.Raise();
            Assert.AreEqual(5, Count);
        }

        private IEnumerable<Waiter> IndexedCombineCoroutine(Action<int> indexGetter)
        {
            yield return WaitForAny(indexGetter,
                WaitForOneShotEvent(OneShotNoArgEvent),
                WaitForOneShotEvent(OneShotIntArgEvent, null));
        }

        [TestMethod]
        public void TestIndexedCombine()
        {
            int index = 0;
            StartCoroutine(IndexedCombineCoroutine((x) => index = x));
            OneShotNoArgEvent.Raise();
            Assert.AreEqual(0, index);

            // test stop
            StartCoroutine(IndexedCombineCoroutine((x) => index = x));
            OneShotIntArgEvent.Raise(0);
            Assert.AreEqual(1, index);
        }

        private class SomeMessage
        {
        }

        private IEnumerable<Waiter> MessageCoroutine()
        {
            SomeMessage message = null;
            yield return WaitForMessage<SomeMessage>((x) => message = x);
            Count = 1;
            Assert.AreNotEqual(null, message);
        }

        [TestMethod]
        public void TestMessage()
        {
            StartCoroutine(MessageCoroutine());
            OnMessage(new SomeMessage());
            Assert.AreEqual(1, Count);

            // test stop
            Count = 0;
            var controller = StartCoroutine(MessageCoroutine());
            controller.Stop();
            OnMessage(new SomeMessage());
            Assert.AreEqual(0, Count);
        }

        private event Action<int> IntArgEvent;

        private IEnumerable<Waiter> EventCoroutine()
        {
            Count += 1;
            int value = 0;
            yield return WaitForEvent<int>((x) => IntArgEvent += x, (x) => IntArgEvent -= x, (x) => value = x);
            Count += value;
        }

        [TestMethod]
        public void TestEvent()
        {
            StartCoroutine(EventCoroutine());
            Assert.AreEqual(1, Count);
            IntArgEvent(2);
            Assert.AreEqual(3, Count);

            // test stop
            Count = 0;
            var controller = StartCoroutine(EventCoroutine());
            Assert.AreEqual(1, Count);
            controller.Stop();
            if (IntArgEvent != null)
                IntArgEvent(2);
            Assert.AreEqual(1, Count);
        }
    }
}
