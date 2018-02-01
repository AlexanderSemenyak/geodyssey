using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utility.Collections
{
    using NUnit.Framework;

    [TestFixture]
    public class CircularLinkedListLinearTest
    {
        CircularLinkedList<int> emptyList;
        CircularLinkedList<int> oneList;
        CircularLinkedList<int> twoList;
        CircularLinkedList<int> threeList;
        CircularLinkedList<string> stringList;

        [SetUp]
        public void Setup()
        {
            emptyList = new CircularLinkedList<int>();
            emptyList.IsCircular = false;

            oneList = new CircularLinkedList<int>( new int[] { 42 });
            oneList.IsCircular = false;

            twoList = new CircularLinkedList<int>(new int[] { 37, 51 });
            twoList.IsCircular = false;

            threeList = new CircularLinkedList<int>();
            threeList.IsCircular = false;

            // 2 3 4
            threeList.AddLast(3);
            threeList.AddLast(4);
            threeList.AddFirst(2);

            string[] tmpStrings = new string[] { "foo", "bar", "baz" };
            // FIXME workaround for 74953

            List<string> workaround = new List<string>();
            foreach (string s in tmpStrings)
                workaround.Add(s);

            // strings = new CircularLinkedList <string> (tmpStrings);
            stringList = new CircularLinkedList<string>(workaround);
            stringList.IsCircular = false;
        }

        [Test]
        public void AddedTest()
        {
            int i = 2;
            foreach (int current in threeList)
            {
                Assert.AreEqual(i, current);
                i++;
            }
            Assert.AreEqual(5, i);
        }

        [Test]
        public void CreatedTest()
        {
            string[] values = new string[] { "foo", "bar", "baz" };
            int i = 0;
            foreach (string current in stringList)
            {
                Assert.AreEqual(values[i], current);
                i++;
            }
            Assert.AreEqual(3, i);
        }

        [Test]
        public void NonCircularNodeTest()
        {
            CircularLinkedListNode<int> node = threeList.First;
            Assert.AreEqual(2, node.Value);
            CircularLinkedListNode<int> previous = node.Previous;
            Assert.IsNull(previous);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(3, node.Value);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(4, node.Value);

            node = node.Next;
            Assert.IsNull(node);
        }

        [Test]
        public void BecomeCircularTest()
        {
            threeList.IsCircular = true;

            CircularLinkedListNode<int> node = threeList.First;
            Assert.AreEqual(2, node.Value);
            CircularLinkedListNode<int> previous = node.Previous;
            Assert.AreEqual(4, previous.Value);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(3, node.Value);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(4, node.Value);
            Assert.AreSame(previous, node);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(2, node.Value);
            Assert.AreSame(node, threeList.First);
        }

        [Test]
        public void ClearTest()
        {
            threeList.Clear();
            Assert.AreEqual(0, threeList.Count);
        }

        [Test]
        public void ContainsTest()
        {
            Assert.IsTrue(threeList.Contains(3));
            Assert.IsFalse(threeList.Contains(5));
        }

        [Test]
        public void AddBeforeAndAfterTest()
        {
            CircularLinkedListNode<int> node = threeList.Find(3);
            threeList.AddAfter(node, new CircularLinkedListNode<int>(5));
            CircularLinkedListNode<int> sixNode = threeList.AddAfter(node, 6);
            CircularLinkedListNode<int> sevenNode = threeList.AddBefore(node, 7);
            threeList.AddBefore(node, new CircularLinkedListNode<int>(8));

            Assert.AreEqual(6, sixNode.Value);
            Assert.AreEqual(7, sevenNode.Value);

            // 2 7 8 3 6 5 4
            int[] values = new int[] { 2, 7, 8, 3, 6, 5, 4 };
            int i = 0;
            foreach (int current in threeList)
            {
                Assert.AreEqual(values[i], current);
                i++;
            }
            for (CircularLinkedListNode<int> current = threeList.First; current != null; current = current.Next)
                Assert.AreSame(threeList, current.List);
        }

        [Test]
        public void SpliceFirstTest()
        {
            twoList.SpliceFirst(threeList.First, threeList.First.Next);
            Assert.AreEqual(4, twoList.Count);
            Assert.AreEqual(1, threeList.Count);

            int[] values1 = new int[] { 2, 3, 37, 51 };
            int i = 0;
            foreach (int current in twoList)
            {
                Assert.AreEqual(values1[i], current);
                i++;
            }

            Assert.AreEqual(4, threeList.First.Value);
        }

        [Test]
        public void SpliceLastTest()
        {
            twoList.SpliceLast(threeList.First.Next, threeList.Last);
            Assert.AreEqual(4, twoList.Count);
            Assert.AreEqual(1, threeList.Count);

            int[] values1 = new int[] { 37, 51, 3, 4 };
            int i = 0;
            foreach (int current in twoList)
            {
                Assert.AreEqual(values1[i], current);
                i++;
            }

            Assert.AreEqual(2, threeList.First.Value);
        }

        [Test]
        public void CopyToTest()
        {
            int[] values = new int[] { 2, 3, 4 };
            int[] output = new int[3];
            threeList.CopyTo(output, 0);
            for (int i = 0; i < 3; i++)
                Assert.AreEqual(values[i], output[i]);
        }

        [Test]
        public void FindPositiveTest()
        {
            threeList.AddFirst(4);

            CircularLinkedListNode<int> head, tail;
            head = threeList.Find(4);
            tail = threeList.FindLast(4);
            Assert.AreEqual(threeList.First, head);
            Assert.AreEqual(threeList.Last, tail);
        }

        [Test]
        public void FindNegativeTest()
        {
            CircularLinkedListNode<int> fiveNode = threeList.Find(5);
            Assert.IsNull(fiveNode);
        }

        [Test]
        public void FindPredicatePositiveTest()
        {
            CircularLinkedListNode<int> found = threeList.Find(delegate (int a)
            {
                return a == 4;
            });
            Assert.AreEqual(threeList.Last, found);
        }

        [Test]
        public void FindPredicateNegativeTest()
        {
            CircularLinkedListNode<int> found = threeList.Find(delegate(int a)
            {
                return a == 5;
            });
            Assert.AreEqual(null, found);
        }

        [Test]
        public void FindNodePredicatePositiveTest()
        {
            CircularLinkedListNode<int> found = threeList.FindNode(delegate(CircularLinkedListNode<int> a)
            {
                return a.Value == 4;
            });
            Assert.AreEqual(threeList.Last, found);
        }

        [Test]
        public void FindNodePredicateNegativeTest()
        {
            CircularLinkedListNode<int> found = threeList.FindNode(delegate(CircularLinkedListNode<int> a)
            {
                return a.Value == 5;
            });
            Assert.AreEqual(null, found);
        }

        [Test]
        public void FindPairPositiveTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindPair(3, 4);
            Assert.AreEqual(threeList.Last, pairNode.Next);
        }

        [Test]
        public void FindPairNegativeTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindPair(4, 2);
            Assert.IsNull(pairNode);
        }

        [Test]
        public void FindPredicatePairPositiveTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindPair(delegate(int a, int b)
            {
                return a == 3 && b == 4;
            });
            Assert.AreEqual(threeList.Last, pairNode.Next);
        }

        [Test]
        public void FindPredicatePairNegativeTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindPair(delegate(int a, int b)
            {
                return a == 4 && b == 2;
            });
            Assert.AreEqual(null, pairNode);
        }

        [Test]
        public void FindPredicateNodePairPositiveTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                return a.Value == 3 && b.Value == 4;
            });
            Assert.AreEqual(threeList.Last, pairNode.Next);
        }

        [Test]
        public void FindPredicateNodePairNegativeTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                return a.Value == 4 && b.Value == 2;
            });
            Assert.AreEqual(null, pairNode);
        }

        [Test]
        public void RemoveTest()
        {
            Assert.IsTrue(threeList.Remove(3));
            Assert.AreEqual(2, threeList.Count);

            int[] values = { 2, 4 };
            int i = 0;
            foreach (int current in threeList)
            {
                Assert.AreEqual(values[i], current);
                i++;
            }
            Assert.IsFalse(threeList.Remove(5));

            CircularLinkedListNode<string> node = stringList.Find("baz");
            stringList.Remove(node);

            Assert.IsNull(node.List);
            Assert.IsNull(node.Previous);
            Assert.IsNull(node.Next);

            string[] values2 = { "foo", "bar" };
            i = 0;
            foreach (string current in stringList)
            {
                Assert.AreEqual(values2[i], current);
                i++;
            }
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void RemoveNullNodeTest()
        {
            threeList.Remove(null);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void RemoveInvalidNodeTest()
        {
            threeList.Remove(new CircularLinkedListNode<int>(4));
        }

        [Test]
        public void RemoveFirstLastTest()
        {
            stringList.RemoveFirst();
            stringList.RemoveLast();
            Assert.AreEqual(1, stringList.Count);
            Assert.AreEqual("bar", stringList.First.Value);
        }

        [Test]
        public void ListSerializationTest()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, threeList);

            stream.Position = 0;
            object deserialized = formatter.Deserialize(stream);

            Assert.IsTrue(deserialized is CircularLinkedList<int>);

            CircularLinkedList<int> dlist = deserialized as CircularLinkedList<int>;

            int[] values = { 2, 3, 4 };
            int i = 0;
            foreach (int value in dlist)
            {
                Assert.AreEqual(values[i], value);
                i++;
            }
            Assert.AreEqual(3, i);
        }

        /* FIXME: disabled pending fix for #75299
        [Test]
        */
        public void EnumeratorSerializationTest()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            CircularLinkedList<int>.Enumerator e = threeList.GetEnumerator();
            formatter.Serialize(stream, e);

            stream.Position = 0;
            object deserialized = formatter.Deserialize(stream);

            Assert.IsTrue(deserialized is CircularLinkedList<int>.Enumerator);

            CircularLinkedList<int>.Enumerator d = (CircularLinkedList<int>.Enumerator) deserialized;

            int[] values = { 2, 3, 4 };
            int i = 0;
            while (d.MoveNext())
            {
                Assert.AreEqual(values[i], d.Current);
                i++;
            }
            Assert.AreEqual(3, i);
        }

        [Test]
        public void ForEachTestEmptyList()
        {
            List<int> result = new List<int>();
            emptyList.ForEach(delegate(int a)
            {
                result.Add(a);
            });
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ForEachTestOneList()
        {
            List<int> result = new List<int>();
            oneList.ForEach(delegate(int a)
            {
                result.Add(a);
            });
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(42, result[0]);
        }

        [Test]
        public void ForEachTestTwoList()
        {
            List<int> result = new List<int>();
            twoList.ForEach(delegate(int a)
            {
                result.Add(a);
            });
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(37, result[0]);
            Assert.AreEqual(51, result[1]);
        }

        [Test]
        public void ForEachTestThreeList()
        {
            List<int> result = new List<int>();
            threeList.ForEach(delegate(int a)
            {
                result.Add(a);
            });
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(3, result[1]);
            Assert.AreEqual(4, result[2]);
        }

        [Test]
        public void ForEachNodeTestEmptyList()
        {
            List<int> result = new List<int>();
            emptyList.ForEachNode(delegate(CircularLinkedListNode<int> a)
            {
                Assert.IsNotNull(a);
                result.Add(a.Value);
            });
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ForEachNodeTestOneList()
        {
            List<int> result = new List<int>();
            oneList.ForEachNode(delegate(CircularLinkedListNode<int> a)
            {
                Assert.IsNotNull(a);
                result.Add(a.Value);
            });
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(42, result[0]);
        }

        [Test]
        public void ForEachNodeTestTwoList()
        {
            List<int> result = new List<int>();
            twoList.ForEachNode(delegate(CircularLinkedListNode<int> a)
            {
                Assert.IsNotNull(a);
                result.Add(a.Value);
            });
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(37, result[0]);
            Assert.AreEqual(51, result[1]);
        }

        [Test]
        public void ForEachNodeTestThreeList()
        {
            List<int> result = new List<int>();
            threeList.ForEachNode(delegate(CircularLinkedListNode<int> a)
            {
                Assert.IsNotNull(a);
                result.Add(a.Value);
            });
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(3, result[1]);
            Assert.AreEqual(4, result[2]);
        }

        [Test]
        public void ForEachPairTestEmptyList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            emptyList.ForEachPair(delegate(int a, int b)
            {
                resultFirst.Add(a);
                resultSecond.Add(b);
            });
            Assert.AreEqual(0, resultFirst.Count);
            Assert.AreEqual(0, resultSecond.Count);
        }

        [Test]
        public void ForEachPairTestOneList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            oneList.ForEachPair(delegate(int a, int b)
            {
                resultFirst.Add(a);
                resultSecond.Add(b);
            });
            Assert.AreEqual(0, resultFirst.Count);
            Assert.AreEqual(0, resultSecond.Count);
        }

        [Test]
        public void ForEachPairTestTwoList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            twoList.ForEachPair(delegate(int a, int b)
            {
                resultFirst.Add(a);
                resultSecond.Add(b);
            });
            Assert.AreEqual(1, resultFirst.Count);
            Assert.AreEqual(1, resultSecond.Count);
            Assert.AreEqual(37, resultFirst[0]);
            Assert.AreEqual(51, resultSecond[0]);
        }

        [Test]
        public void ForEachPairTestThreeList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            threeList.ForEachPair(delegate(int a, int b)
            {
                resultFirst.Add(a);
                resultSecond.Add(b);
            });
            Assert.AreEqual(2, resultFirst.Count);
            Assert.AreEqual(2, resultSecond.Count);
            Assert.AreEqual(2, resultFirst[0]);
            Assert.AreEqual(3, resultSecond[0]);
            Assert.AreEqual(3, resultFirst[1]);
            Assert.AreEqual(4, resultSecond[1]);
        }

        [Test]
        public void ForEachNodePairTestEmptyList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            emptyList.ForEachNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                resultFirst.Add(a.Value);
                resultSecond.Add(b.Value);
            });
            Assert.AreEqual(0, resultFirst.Count);
            Assert.AreEqual(0, resultSecond.Count);
        }

        [Test]
        public void ForEachNodePairTestOneList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            oneList.ForEachNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                resultFirst.Add(a.Value);
                resultSecond.Add(b.Value);
            });
            Assert.AreEqual(0, resultFirst.Count);
            Assert.AreEqual(0, resultSecond.Count);
        }

        [Test]
        public void ForEachNodePairTestTwoList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            twoList.ForEachNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                resultFirst.Add(a.Value);
                resultSecond.Add(b.Value);
            });
            Assert.AreEqual(1, resultFirst.Count);
            Assert.AreEqual(1, resultSecond.Count);
            Assert.AreEqual(37, resultFirst[0]);
            Assert.AreEqual(51, resultSecond[0]);
        }

        [Test]
        public void ForEachNodePairTestThreeList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            threeList.ForEachNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                resultFirst.Add(a.Value);
                resultSecond.Add(b.Value);
            });
            Assert.AreEqual(2, resultFirst.Count);
            Assert.AreEqual(2, resultSecond.Count);
            Assert.AreEqual(2, resultFirst[0]);
            Assert.AreEqual(3, resultSecond[0]);
            Assert.AreEqual(3, resultFirst[1]);
            Assert.AreEqual(4, resultSecond[1]);
        }
    }

    //---------------------------------------------------------------------------------------------------------------/

    [TestFixture]
    public class CircularLinkedListCircularTest
    {
        CircularLinkedList<int> emptyList;
        CircularLinkedList<int> oneList;
        CircularLinkedList<int> twoList;
        CircularLinkedList<int> threeList;
        CircularLinkedList<string> stringList;

        [SetUp]
        public void Setup()
        {
            emptyList = new CircularLinkedList<int>();
            oneList = new CircularLinkedList<int>(new int[] { 42 });
            twoList = new CircularLinkedList<int>(new int[] { 37, 51 });
            threeList = new CircularLinkedList<int>();
            
            // 2 3 4
            threeList.AddLast(3);
            threeList.AddLast(4);
            threeList.AddFirst(2);

            string[] tmpStrings = new string[] { "foo", "bar", "baz" };
            // FIXME workaround for 74953

            List<string> workaround = new List<string>();
            foreach (string s in tmpStrings)
                workaround.Add(s);

            // strings = new CircularLinkedList <string> (tmpStrings);
            stringList = new CircularLinkedList<string>(workaround);
        }

        [Test]
        public void AddedTest()
        {
            int i = 2;
            foreach (int current in threeList)
            {
                Assert.AreEqual(i, current);
                i++;
            }
            Assert.AreEqual(5, i);
        }

        [Test]
        public void CreatedTest()
        {
            string[] values = new string[] { "foo", "bar", "baz" };
            int i = 0;
            foreach (string current in stringList)
            {
                Assert.AreEqual(values[i], current);
                i++;
            }
            Assert.AreEqual(3, i);
        }

        [Test]
        public void CircularNodeTest()
        {
            CircularLinkedListNode<int> node = threeList.First;
            Assert.AreEqual(2, node.Value);
            CircularLinkedListNode<int> previous = node.Previous;
            Assert.AreEqual(4, previous.Value);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(3, node.Value);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(4, node.Value);
            Assert.AreSame(previous, node);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(2, node.Value);
            Assert.AreSame(node, threeList.First);
        }

        [Test]
        public void BecomeLinearTest()
        {
            threeList.IsCircular = false;

            CircularLinkedListNode<int> node = threeList.First;
            Assert.AreEqual(2, node.Value);
            CircularLinkedListNode<int> previous = node.Previous;
            Assert.IsNull(previous);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(3, node.Value);

            node = node.Next;
            Assert.IsNotNull(node);
            Assert.AreEqual(4, node.Value);

            node = node.Next;
            Assert.IsNull(node);
        }

        [Test]
        public void ClearTest()
        {
            threeList.Clear();
            Assert.AreEqual(0, threeList.Count);
        }

        [Test]
        public void ContainsTest()
        {
            Assert.IsTrue(threeList.Contains(3));
            Assert.IsFalse(threeList.Contains(5));
        }

        [Test]
        public void AddBeforeAndAfterTest()
        {
            CircularLinkedListNode<int> node = threeList.Find(3);
            threeList.AddAfter(node, new CircularLinkedListNode<int>(5));
            CircularLinkedListNode<int> sixNode = threeList.AddAfter(node, 6);
            CircularLinkedListNode<int> sevenNode = threeList.AddBefore(node, 7);
            threeList.AddBefore(node, new CircularLinkedListNode<int>(8));

            Assert.AreEqual(6, sixNode.Value);
            Assert.AreEqual(7, sevenNode.Value);

            // 2 7 8 3 6 5 4
            int[] values = new int[] { 2, 7, 8, 3, 6, 5, 4 };
            int i = 0;
            foreach (int current in threeList)
            {
                Assert.AreEqual(values[i], current);
                i++;
            }

            CircularLinkedListNode<int> node2 = threeList.First;
            do
            {
                Assert.AreSame(threeList, node2.List);
                node2 = node2.Next;
            }
            while (node2 != threeList.First);
        }

        [Test]
        public void CopyToTest()
        {
            int[] values = new int[] { 2, 3, 4 };
            int[] output = new int[3];
            threeList.CopyTo(output, 0);
            for (int i = 0; i < 3; i++)
                Assert.AreEqual(values[i], output[i]);
        }

        [Test]
        public void FindPositiveTest()
        {
            threeList.AddFirst(4);

            CircularLinkedListNode<int> head, tail;
            head = threeList.Find(4);
            tail = threeList.FindLast(4);
            Assert.AreEqual(threeList.First, head);
            Assert.AreEqual(threeList.Last, tail);
        }

        [Test]
        public void FindNegativeTest()
        {
            CircularLinkedListNode<int> fiveNode = threeList.Find(5);
            Assert.IsNull(fiveNode);
        }

        [Test]
        public void FindPredicatePositiveTest()
        {
            CircularLinkedListNode<int> found = threeList.Find(delegate(int a)
            {
                return a == 4;
            });
            Assert.AreEqual(threeList.Last, found);
        }

        [Test]
        public void FindPredicateNegativeTest()
        {
            CircularLinkedListNode<int> found = threeList.Find(delegate(int a)
            {
                return a == 5;
            });
            Assert.AreEqual(null, found);
        }

        [Test]
        public void FindPairPositiveTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindPair(4, 2);
            Assert.AreEqual(threeList.Last, pairNode);
        }

        [Test]
        public void FindPairNegativeTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindPair(4, 3);
            Assert.IsNull(pairNode);
        }

        [Test]
        public void FindPredicatePairPositiveTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindPair(delegate(int a, int b)
            {
                return a == 4 && b == 2;
            });
            Assert.AreEqual(threeList.Last, pairNode);
        }

        [Test]
        public void FindPredicatePairNegativeTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindPair(delegate(int a, int b)
            {
                return a == 3 && b == 2;
            });
            Assert.AreEqual(null, pairNode);
        }

        [Test]
        public void FindPredicateNodePairPositiveTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                return a.Value == 4 && b.Value == 2;
            });
            Assert.AreEqual(threeList.Last, pairNode);
        }

        [Test]
        public void FindPredicateNodePairNegativeTest()
        {
            CircularLinkedListNode<int> pairNode = threeList.FindNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                return a.Value == 3 && b.Value == 2;
            });
            Assert.AreEqual(null, pairNode);
        }

        [Test]
        public void RemoveTest()
        {
            Assert.IsTrue(threeList.Remove(3));
            Assert.AreEqual(2, threeList.Count);

            int[] values = { 2, 4 };
            int i = 0;
            foreach (int current in threeList)
            {
                Assert.AreEqual(values[i], current);
                i++;
            }
            Assert.IsFalse(threeList.Remove(5));

            CircularLinkedListNode<string> node = stringList.Find("baz");
            stringList.Remove(node);

            Assert.IsNull(node.List);
            Assert.IsNull(node.Previous);
            Assert.IsNull(node.Next);

            string[] values2 = { "foo", "bar" };
            i = 0;
            foreach (string current in stringList)
            {
                Assert.AreEqual(values2[i], current);
                i++;
            }
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void RemoveNullNodeTest()
        {
            threeList.Remove(null);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void RemoveInvalidNodeTest()
        {
            threeList.Remove(new CircularLinkedListNode<int>(4));
        }

        [Test]
        public void RemoveFirstLastTest()
        {
            stringList.RemoveFirst();
            stringList.RemoveLast();
            Assert.AreEqual(1, stringList.Count);
            Assert.AreEqual("bar", stringList.First.Value);
        }

        [Test]
        public void ListSerializationTest()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, threeList);

            stream.Position = 0;
            object deserialized = formatter.Deserialize(stream);

            Assert.IsTrue(deserialized is CircularLinkedList<int>);

            CircularLinkedList<int> dlist = deserialized as CircularLinkedList<int>;

            int[] values = { 2, 3, 4 };
            int i = 0;
            foreach (int value in dlist)
            {
                Assert.AreEqual(values[i], value);
                i++;
            }
            Assert.AreEqual(3, i);
        }

        /* FIXME: disabled pending fix for #75299
        [Test]
        */
        public void EnumeratorSerializationTest()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            CircularLinkedList<int>.Enumerator e = threeList.GetEnumerator();
            formatter.Serialize(stream, e);

            stream.Position = 0;
            object deserialized = formatter.Deserialize(stream);

            Assert.IsTrue(deserialized is CircularLinkedList<int>.Enumerator);

            CircularLinkedList<int>.Enumerator d = (CircularLinkedList<int>.Enumerator) deserialized;

            int[] values = { 2, 3, 4 };
            int i = 0;
            while (d.MoveNext())
            {
                Assert.AreEqual(values[i], d.Current);
                i++;
            }
            Assert.AreEqual(3, i);
        }

        [Test]
        public void ForEachTestEmptyList()
        {
            List<int> result = new List<int>();
            emptyList.ForEach(delegate(int a)
            {
                result.Add(a);
            });
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ForEachTestOneList()
        {
            List<int> result = new List<int>();
            oneList.ForEach(delegate(int a)
            {
                result.Add(a);
            });
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(42, result[0]);
        }

        [Test]
        public void ForEachTestTwoList()
        {
            List<int> result = new List<int>();
            twoList.ForEach(delegate(int a)
            {
                result.Add(a);
            });
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(37, result[0]);
            Assert.AreEqual(51, result[1]);
        }

        [Test]
        public void ForEachTestThreeList()
        {
            List<int> result = new List<int>();
            threeList.ForEach(delegate(int a)
            {
                result.Add(a);
            });
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(3, result[1]);
            Assert.AreEqual(4, result[2]);
        }

        [Test]
        public void ForEachNodeTestEmptyList()
        {
            List<int> result = new List<int>();
            emptyList.ForEachNode(delegate(CircularLinkedListNode<int> a)
            {
                Assert.IsNotNull(a);
                result.Add(a.Value);
            });
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ForEachNodeTestOneList()
        {
            List<int> result = new List<int>();
            oneList.ForEachNode(delegate(CircularLinkedListNode<int> a)
            {
                Assert.IsNotNull(a);
                result.Add(a.Value);
            });
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(42, result[0]);
        }

        [Test]
        public void ForEachNodeTestTwoList()
        {
            List<int> result = new List<int>();
            twoList.ForEachNode(delegate(CircularLinkedListNode<int> a)
            {
                Assert.IsNotNull(a);
                result.Add(a.Value);
            });
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(37, result[0]);
            Assert.AreEqual(51, result[1]);
        }

        [Test]
        public void ForEachNodeTestThreeList()
        {
            List<int> result = new List<int>();
            threeList.ForEachNode(delegate(CircularLinkedListNode<int> a)
            {
                Assert.IsNotNull(a);
                result.Add(a.Value);
            });
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(3, result[1]);
            Assert.AreEqual(4, result[2]);
        }

        [Test]
        public void ForEachPairTestEmptyList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            emptyList.ForEachPair(delegate(int a, int b)
            {
                resultFirst.Add(a);
                resultSecond.Add(b);
            });
            Assert.AreEqual(0, resultFirst.Count);
            Assert.AreEqual(0, resultSecond.Count);
        }

        [Test]
        public void ForEachPairTestOneList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            oneList.ForEachPair(delegate(int a, int b)
            {
                resultFirst.Add(a);
                resultSecond.Add(b);
            });
            Assert.AreEqual(1, resultFirst.Count);
            Assert.AreEqual(1, resultSecond.Count);
            Assert.AreEqual(42, resultFirst[0]);
            Assert.AreEqual(42, resultSecond[0]);
        }

        [Test]
        public void ForEachPairTestTwoList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            twoList.ForEachPair(delegate(int a, int b)
            {
                resultFirst.Add(a);
                resultSecond.Add(b);
            });
            Assert.AreEqual(2, resultFirst.Count);
            Assert.AreEqual(2, resultSecond.Count);
            Assert.AreEqual(37, resultFirst[0]);
            Assert.AreEqual(51, resultSecond[0]);
            Assert.AreEqual(51, resultFirst[1]);
            Assert.AreEqual(37, resultSecond[1]);
        }

        [Test]
        public void ForEachPairTestThreeList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            threeList.ForEachPair(delegate(int a, int b)
            {
                resultFirst.Add(a);
                resultSecond.Add(b);
            });
            Assert.AreEqual(3, resultFirst.Count);
            Assert.AreEqual(3, resultSecond.Count);
            Assert.AreEqual(2, resultFirst[0]);
            Assert.AreEqual(3, resultSecond[0]);
            Assert.AreEqual(3, resultFirst[1]);
            Assert.AreEqual(4, resultSecond[1]);
            Assert.AreEqual(4, resultFirst[2]);
            Assert.AreEqual(2, resultFirst[0]);
        }

        [Test]
        public void ForEachNodePairTestEmptyList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            emptyList.ForEachNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                resultFirst.Add(a.Value);
                resultSecond.Add(b.Value);
            });
            Assert.AreEqual(0, resultFirst.Count);
            Assert.AreEqual(0, resultSecond.Count);
        }

        [Test]
        public void ForEachNodePairTestOneList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            oneList.ForEachNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                resultFirst.Add(a.Value);
                resultSecond.Add(b.Value);
            });
            Assert.AreEqual(1, resultFirst.Count);
            Assert.AreEqual(1, resultSecond.Count);
            Assert.AreEqual(42, resultFirst[0]);
            Assert.AreEqual(42, resultSecond[0]);
        }

        [Test]
        public void ForEachNodePairTestTwoList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            twoList.ForEachNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                resultFirst.Add(a.Value);
                resultSecond.Add(b.Value);
            });
            Assert.AreEqual(2, resultFirst.Count);
            Assert.AreEqual(2, resultSecond.Count);
            Assert.AreEqual(37, resultFirst[0]);
            Assert.AreEqual(51, resultSecond[0]);
            Assert.AreEqual(51, resultFirst[1]);
            Assert.AreEqual(37, resultSecond[1]);
        }

        [Test]
        public void ForEachNodePairTestThreeList()
        {
            List<int> resultFirst = new List<int>();
            List<int> resultSecond = new List<int>();
            threeList.ForEachNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                Assert.IsNotNull(a);
                Assert.IsNotNull(b);
                resultFirst.Add(a.Value);
                resultSecond.Add(b.Value);
            });
            Assert.AreEqual(3, resultFirst.Count);
            Assert.AreEqual(3, resultSecond.Count);
            Assert.AreEqual(2, resultFirst[0]);
            Assert.AreEqual(3, resultSecond[0]);
            Assert.AreEqual(3, resultFirst[1]);
            Assert.AreEqual(4, resultSecond[1]);
            Assert.AreEqual(4, resultFirst[2]);
            Assert.AreEqual(2, resultSecond[2]);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ForEachModificationTest()
        {
            threeList.ForEach(delegate(int a)
            {
                threeList.Clear();
            });
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ForEachNodeModificationTest()
        {
            threeList.ForEachNode(delegate(CircularLinkedListNode<int> a)
            {
                a.List.Clear();
            });
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ForEachPairModificationTest()
        {
            threeList.ForEachPair(delegate(int a, int b)
            {
                threeList.Clear();
            });
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ForEachNodePairModifcationTest()
        {
            threeList.ForEachNodePair(delegate(CircularLinkedListNode<int> a, CircularLinkedListNode<int> b)
            {
                a.List.Clear();
            });
        }
    }
}
