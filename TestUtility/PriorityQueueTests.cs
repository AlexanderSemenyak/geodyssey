using System;
using System.Collections.Generic;
using System.Text;

using Wintellect.PowerCollections;

namespace Utility.Collections
{
    using NUnit.Framework;

    [TestFixture]
    public class PriorityQueueFilling
    {
        PriorityQueueDictionary<int, string> priorityQueueDictionary;

        [Test]
        public void DefaultConstructor()
        {
            priorityQueueDictionary = new PriorityQueueDictionary<int, string>();
            Assert.AreEqual(0, priorityQueueDictionary.Count);
        }

        [Test]
        public void ConstructFromEnumerable()
        {
            string[] array = new string[] { "item one", "item two", "item three", "item four" };
            priorityQueueDictionary = new PriorityQueueDictionary<int,string>( array );
            Assert.IsTrue(priorityQueueDictionary.Contains("item one"));
            Assert.IsTrue(priorityQueueDictionary.Contains("item two"));
            Assert.IsTrue(priorityQueueDictionary.Contains("item three"));
            Assert.IsTrue(priorityQueueDictionary.Contains("item four"));
        }

        [Test]
        public void ConstructFromPairEnumerator()
        {
            List<Pair<int, string>> list = new List<Pair<int, string>>();
            list.Add(new Pair<int, string>(12, "item one"));
            list.Add(new Pair<int, string>(5, "item two"));
            list.Add(new Pair<int, string>(7, "item three"));
            list.Add(new Pair<int, string>(6, "item four"));
            priorityQueueDictionary = new PriorityQueueDictionary<int, string>(list);
            Assert.AreEqual("item one", priorityQueueDictionary.Dequeue());
            Assert.AreEqual("item three", priorityQueueDictionary.Dequeue());
            Assert.AreEqual("item four", priorityQueueDictionary.Dequeue());
            Assert.AreEqual("item two", priorityQueueDictionary.Dequeue());
        }

        [Test]
        public void Enqueuing()
        {
            priorityQueueDictionary = new PriorityQueueDictionary<int, string>();
            priorityQueueDictionary.Enqueue(12, "item one");
            Assert.AreEqual(1, priorityQueueDictionary.Count);
            Assert.AreEqual("item one", priorityQueueDictionary.Front);

            priorityQueueDictionary.Enqueue(5, "item two");
            Assert.AreEqual("item one", priorityQueueDictionary.Front);
            Assert.AreEqual(2, priorityQueueDictionary.Count);

            priorityQueueDictionary.Enqueue(7, "item three");
            Assert.AreEqual("item one", priorityQueueDictionary.Front);
            Assert.AreEqual(3, priorityQueueDictionary.Count);

            priorityQueueDictionary.Enqueue(6, "item four");
            Assert.AreEqual("item one", priorityQueueDictionary.Front);
            Assert.AreEqual(4, priorityQueueDictionary.Count);
        }

        [Test]
        public void EnumerationTest()
        {
            priorityQueueDictionary = new PriorityQueueDictionary<int, string>();
            Bag<string> bag = new Bag<string>();
            for (int i = 0; i < 100; ++i)
            {
                string s = "item: " + i.ToString();
                bag.Add(s);
                int p = i * 2;
                priorityQueueDictionary.Enqueue(p, s);
            }
            foreach (string s in priorityQueueDictionary)
            {
                Assert.IsTrue(bag.Contains(s));
                bag.Remove(s);
            }
            Assert.AreEqual(0, bag.Count);
        }
    }

    [TestFixture]
    public class PriorityQueueManipulation
    {
        PriorityQueueDictionary<int, string> priorityQueueDictionary;

        [SetUp]
        public void SetUp()
        {
            priorityQueueDictionary = new PriorityQueueDictionary<int, string>();
            priorityQueueDictionary.Enqueue(12, "item one");
            priorityQueueDictionary.Enqueue(5, "item two");
            priorityQueueDictionary.Enqueue(7, "item three");
            priorityQueueDictionary.Enqueue(6, "item four");
        }

        [Test]
        public void Dequeueing()
        {
            Assert.AreEqual("item one", priorityQueueDictionary.Dequeue());
            Assert.AreEqual(3, priorityQueueDictionary.Count);
            Assert.AreEqual("item three", priorityQueueDictionary.Dequeue());
            Assert.AreEqual(2, priorityQueueDictionary.Count);
            Assert.AreEqual("item four", priorityQueueDictionary.Dequeue());
            Assert.AreEqual(1, priorityQueueDictionary.Count);
            Assert.AreEqual("item two", priorityQueueDictionary.Dequeue());
            Assert.AreEqual(0, priorityQueueDictionary.Count);
        }

        [Test]
        public void Add()
        {
            Assert.IsTrue(priorityQueueDictionary.Contains("item one"));
            Assert.IsTrue(priorityQueueDictionary.Contains("item two"));
            Assert.IsTrue(priorityQueueDictionary.Contains("item three"));
            Assert.IsTrue(priorityQueueDictionary.Contains("item four"));
        }

        [Test]
        public void Contains()
        {
            Assert.IsTrue(priorityQueueDictionary.Contains("item three"));
            Assert.IsFalse(priorityQueueDictionary.Contains("item seven"));
        }

        [Test]
        public void Clear()
        {
            priorityQueueDictionary.Clear();
            Assert.AreEqual(0, priorityQueueDictionary.Count);
        }

        [Test]
        public void CopyTo()
        {
            string[] array = new string[priorityQueueDictionary.Count];
            priorityQueueDictionary.CopyTo(array, 0);
            Assert.Contains("item one", array);
            Assert.Contains("item two", array);
            Assert.Contains("item three", array);
            Assert.Contains("item four", array);
        }

        [Test]
        public void Count()
        {
            Assert.AreEqual(4, priorityQueueDictionary.Count);
            priorityQueueDictionary.Dequeue();
            Assert.AreEqual(3, priorityQueueDictionary.Count);
        }

        [Test]
        public void Remove()
        {
            priorityQueueDictionary.Remove("item three");
            Assert.AreEqual(3, priorityQueueDictionary.Count);
            Assert.IsFalse(priorityQueueDictionary.Contains("item three"));
        }
    }
}
