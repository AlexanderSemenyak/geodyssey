using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility.Collections
{
    using NUnit.Framework;

    [TestFixture]
    public class SplayDictionaryTests
    {
        private SplayDictionary<int, string> dictionary;

        [Test]
        public void DefaultConstructor()
        {
            dictionary = new SplayDictionary<int, string>();
            Assert.IsTrue(dictionary.IsEmpty);
            Assert.AreEqual(0, dictionary.Count);
        }

        [Test]
        public void ConstructFromEnumerable()
        {
            var array = new [] { new KeyValuePair<int, string>(41, "forty-one"),
                                 new KeyValuePair<int, string>(92, "ninety-two"),
                                 new KeyValuePair<int, string>(71, "seventy-one"),
                                 new KeyValuePair<int, string>(12, "twelve"),
                                 new KeyValuePair<int, string>(20, "twenty") };
            dictionary = new SplayDictionary<int, string>(array);
            Assert.AreEqual(array.Length, dictionary.Count);
            foreach (KeyValuePair<int, string> kv in array)
            {
                Assert.IsTrue(dictionary.Contains(kv));
            }
        }

        [Test]
        public void EmptinessTest()
        {
            dictionary = new SplayDictionary<int, string>();
            dictionary.Add(new KeyValuePair<int, string>(42, "forty-two"));
            Assert.IsFalse(dictionary.IsEmpty);
        }

        [Test]
        public void EnumerationTest()
        {
            var array = new[] { new KeyValuePair<int, string>(64, "sixty-four"),
                                new KeyValuePair<int, string>(3,  "three"),
                                new KeyValuePair<int, string>(16, "sixteen"),
                                new KeyValuePair<int, string>(73, "seventy-three"),
                                new KeyValuePair<int, string>(67, "sixty-seven") };
            dictionary = new SplayDictionary<int, string>(array);
            IEnumerator<KeyValuePair<int, string>> e = dictionary.GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(new KeyValuePair<int, string>(3, "three"), e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(new KeyValuePair<int, string>(16, "sixteen"), e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(new KeyValuePair<int, string>(64, "sixty-four"), e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(new KeyValuePair<int, string>(67, "sixty-seven"), e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual(new KeyValuePair<int, string>(73, "seventy-three"), e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [Test]
        public void EnumerationEmptyTest()
        {
            dictionary = new SplayDictionary<int, string>();
            IEnumerator<KeyValuePair<int, string>> e = dictionary.GetEnumerator();
            Assert.IsFalse(e.MoveNext());
        }

        [Test]
        public void ClearTest()
        {
            var array = new[] { new KeyValuePair<int, string>(100, "one hundred"),
                                new KeyValuePair<int, string>(29,  "twenty-nine"),
                                new KeyValuePair<int, string>(77, "seventy-seven"),
                                new KeyValuePair<int, string>(59, "fifty-nine"),
                                new KeyValuePair<int, string>(61, "sixty-one") };
            dictionary = new SplayDictionary<int, string>(array);
            Assert.IsFalse(dictionary.IsEmpty);
            dictionary.Clear();
            Assert.IsTrue(dictionary.IsEmpty);
            Assert.AreEqual(0, dictionary.Count);
        }

        [Test]
        public void CountTest()
        {
            dictionary = new SplayDictionary<int, string>();
            dictionary.Add(5, "five");
            Assert.AreEqual(1, dictionary.Count);
            dictionary.Add(3, "three");
            Assert.AreEqual(2, dictionary.Count);
            dictionary.Remove(5);
            Assert.AreEqual(1, dictionary.Count);
            dictionary.Add(7, "seven");
            Assert.AreEqual(2, dictionary.Count);
            dictionary.Add(9, "nine");
            Assert.AreEqual(3, dictionary.Count);
            dictionary.Add(5 , "five");
            Assert.AreEqual(4, dictionary.Count);
            dictionary.Remove(7);
            Assert.AreEqual(3, dictionary.Count);
        }

        [Test]
        public void RootKeyTest()
        {
            var array = new[]
                            {
                                new KeyValuePair<int, string>(5, "five"),
                                new KeyValuePair<int, string>(3, "three"),
                                new KeyValuePair<int, string>(7, "seven")
                            };
            dictionary = new SplayDictionary<int, string>(array);
            Assert.IsTrue(dictionary.ContainsKey(5));
            Assert.AreEqual(5, dictionary.Root.Key);
            Assert.AreEqual(3, dictionary.Left.Key);
            Assert.AreEqual(7, dictionary.Right.Key);
            Assert.AreEqual(3, dictionary.Minimum.Key);
            Assert.AreEqual(7, dictionary.Maximum.Key);
        }

        [Test]
        public void RootValueTest()
        {
            var array = new[]
                            {
                                new KeyValuePair<int, string>(5, "five"),
                                new KeyValuePair<int, string>(3, "three"),
                                new KeyValuePair<int, string>(7, "seven")
                            };
            dictionary = new SplayDictionary<int, string>(array);
            Assert.IsTrue(dictionary.ContainsKey(5));
            Assert.AreEqual("five", dictionary.Root.Value);
            Assert.AreEqual("three", dictionary.Left.Value);
            Assert.AreEqual("seven", dictionary.Right.Value);
            Assert.AreEqual("three", dictionary.Minimum.Value);
            Assert.AreEqual("seven", dictionary.Maximum.Value);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void MinimumFailTest()
        {
            dictionary = new SplayDictionary<int, string>();
            var dummy = dictionary.Minimum;
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void MaximumFailTest()
        {
            dictionary = new SplayDictionary<int, string>();
            var dummy = dictionary.Maximum;
        }

        [Test]
        public void TryGetValuePositiveTest()
        {
            var array = new[] { new KeyValuePair<int, string>(64, "sixty-four"),
                                new KeyValuePair<int, string>(3,  "three"),
                                new KeyValuePair<int, string>(16, "sixteen"),
                                new KeyValuePair<int, string>(73, "seventy-three"),
                                new KeyValuePair<int, string>(67, "sixty-seven") };
            dictionary = new SplayDictionary<int, string>(array);
            string result;
            bool success = dictionary.TryGetValue(16, out result);
            Assert.IsTrue(success);
            Assert.AreEqual("sixteen", result);
        }

        [Test]
        public void TryGetValueNegativeTest()
        {
            var array = new[] { new KeyValuePair<int, string>(64, "sixty-four"),
                                new KeyValuePair<int, string>(3,  "three"),
                                new KeyValuePair<int, string>(16, "sixteen"),
                                new KeyValuePair<int, string>(73, "seventy-three"),
                                new KeyValuePair<int, string>(67, "sixty-seven") };
            dictionary = new SplayDictionary<int, string>(array);
            string result;
            bool success = dictionary.TryGetValue(15, out result);
            Assert.IsFalse(success);
        }

        [Test]
        public void FindLargestBelowTest()
        {
            var array = new[] { new KeyValuePair<int, string>(5,  "five"),
                                new KeyValuePair<int, string>(11, "eleven"),
                                new KeyValuePair<int, string>(8,  "eight"),
                                new KeyValuePair<int, string>(13, "thirteen"),
                                new KeyValuePair<int, string>(3,  "three"),
                                new KeyValuePair<int, string>(7,  "seven"),
                                new KeyValuePair<int, string>(12, "twelve") };
            dictionary = new SplayDictionary<int, string>(array);
            Assert.AreEqual(new KeyValuePair<int, string>(8, "eight"), dictionary.LargestBelow(10));
        }

        [Test]
        public void FindSmallestAboveTest()
        {
            var array = new[] { new KeyValuePair<int, string>(5,  "five"),
                                new KeyValuePair<int, string>(11, "eleven"),
                                new KeyValuePair<int, string>(8,  "eight"),
                                new KeyValuePair<int, string>(13, "thirteen"),
                                new KeyValuePair<int, string>(3,  "three"),
                                new KeyValuePair<int, string>(7,  "seven"),
                                new KeyValuePair<int, string>(12, "twelve") };
            dictionary = new SplayDictionary<int, string>(array);
            Assert.AreEqual(new KeyValuePair<int, string>(11, "eleven"), dictionary.SmallestAbove(8));
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void FindLargestBelowTestFail()
        {
            var array = new[] { new KeyValuePair<int, string>(5,  "five"),
                                new KeyValuePair<int, string>(11, "eleven"),
                                new KeyValuePair<int, string>(8,  "eight"),
                                new KeyValuePair<int, string>(13, "thirteen"),
                                new KeyValuePair<int, string>(3,  "three"),
                                new KeyValuePair<int, string>(7,  "seven"),
                                new KeyValuePair<int, string>(12, "twelve") };
            dictionary = new SplayDictionary<int, string>(array);
            dictionary.LargestBelow(3);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void FindSmallestAboveTestFail()
        {
            var array = new[] { new KeyValuePair<int, string>(5,  "five"),
                                new KeyValuePair<int, string>(11, "eleven"),
                                new KeyValuePair<int, string>(8,  "eight"),
                                new KeyValuePair<int, string>(13, "thirteen"),
                                new KeyValuePair<int, string>(3,  "three"),
                                new KeyValuePair<int, string>(7,  "seven"),
                                new KeyValuePair<int, string>(12, "twelve") };
            dictionary = new SplayDictionary<int, string>(array);
            dictionary.SmallestAbove(13);
        }

        [Test]
        public void RemoveFailTest()
        {
            var array = new[] { new KeyValuePair<int, string>(77, "seventy-seven"),
                                new KeyValuePair<int, string>(85, "eighty-five"),
                                new KeyValuePair<int, string>(66, "sixty-six"),
                                new KeyValuePair<int, string>(56, "fifty-six"),
                                new KeyValuePair<int, string>(74, "seventy-four"),
                                new KeyValuePair<int, string>(94, "ninety-four"),
                                new KeyValuePair<int, string>(27, "twenty-seven"),
                                new KeyValuePair<int, string>(61, "sixty-one"),
                                new KeyValuePair<int, string>(48, "forty-eight"),
                                new KeyValuePair<int, string>(27, "fifty-seven") };
            dictionary = new SplayDictionary<int, string>(array);
            Assert.IsFalse(dictionary.Remove(34));
        }

        [Test]
        public void WeissTest()
        {
            dictionary = new SplayDictionary<int, string>();
            const int NUMS = 40000;
            const int GAP = 307;

            for (int i = GAP; i != 0; i = (i + GAP) % NUMS)
            {
                dictionary.Add(i, i.ToString());
            }

            for (int i = 1; i < NUMS; i += 2)
            {
                dictionary.Remove(i);
            }

            Assert.AreEqual(new KeyValuePair<int, string>(2, "2"), dictionary.Minimum);
            Assert.AreEqual(new KeyValuePair<int, string>(NUMS - 2, (NUMS - 2).ToString()), dictionary.Maximum);

            for (int i = 2; i < NUMS; i += 2)
            {
                Assert.IsTrue(dictionary.ContainsKey(i));
            }

            for (int i = 1; i < NUMS; i += 2)
            {
                Assert.IsFalse(dictionary.ContainsKey(i));
            }
        }

        [Test]
        public void ItemTest()
        {
            dictionary = new SplayDictionary<int, string>();
            dictionary[1] = "one";
            Assert.AreEqual(1, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey(1));
            Assert.AreEqual("one", dictionary[1]);

            dictionary[2] = "two";
            Assert.AreEqual(2, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey(1));
            Assert.IsTrue(dictionary.ContainsKey(2));
            Assert.AreEqual("one", dictionary[1]);
            Assert.AreEqual("two", dictionary[2]);

            dictionary[2] = "TWO";
            Assert.AreEqual(2, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey(1));
            Assert.IsTrue(dictionary.ContainsKey(2));
            Assert.AreEqual("one", dictionary[1]);
            Assert.AreEqual("TWO", dictionary[2]);

            dictionary[3] = "three";
            Assert.AreEqual(3, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey(1));
            Assert.IsTrue(dictionary.ContainsKey(2));
            Assert.IsTrue(dictionary.ContainsKey(3));
            Assert.AreEqual("one", dictionary[1]);
            Assert.AreEqual("TWO", dictionary[2]);
            Assert.AreEqual("three", dictionary[3]);

            dictionary[3] = "three";
            Assert.AreEqual(3, dictionary.Count);
            Assert.IsTrue(dictionary.ContainsKey(1));
            Assert.IsTrue(dictionary.ContainsKey(2));
            Assert.IsTrue(dictionary.ContainsKey(3));
            Assert.AreEqual("one", dictionary[1]);
            Assert.AreEqual("TWO", dictionary[2]);
            Assert.AreEqual("three", dictionary[3]);
        }
    }
}
