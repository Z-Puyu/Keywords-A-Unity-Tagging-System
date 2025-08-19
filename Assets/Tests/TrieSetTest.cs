using System;
using System.Collections.Generic;
using System.Linq;
using Keywords.TrieUtils;
using NUnit.Framework;

namespace Tests {
    public class TrieSetTest {
        [Test]
        public void TrieSetInsertStrings_WithoutSeparator_SplitByCharacter() {
            TrieSet<string, char> set = new TrieSet<string, char>();
            HashSet<string> testCases = new HashSet<string>(1000);
            HashSet<string> prefixes = new HashSet<string>();
            for (int i = 0; i < 1000; i += 1) {
                testCases.Add(Guid.NewGuid().ToString());
            }

            foreach (string testCase in testCases) {
                set.Add(testCase);
                for (int i = 0; i < testCase.Length; i += 1) {
                    prefixes.Add(testCase[..(i + 1)]);
                }
            }

            Assert.AreEqual(testCases.Count, set.Count);
            foreach (string prefix in prefixes) {
                Assert.IsTrue(set.ContainsPrefix(prefix));
                if (testCases.Contains(prefix)) {
                    Assert.IsTrue(set.Contains(prefix));
                } else {
                    Assert.IsFalse(set.Contains(prefix));
                }
            }
        }
        
        [Test]
        public void TrieSetInsertStrings_WithSeparator_SplitByCharacter() {
            TrieSet<string, char> set = new TrieSet<string, char>('-');
            HashSet<string> testCases = new HashSet<string>(1000);
            HashSet<string> prefixes = new HashSet<string>();
            for (int i = 0; i < 1000; i += 1) {
                testCases.Add(Guid.NewGuid().ToString());
            }

            foreach (string testCase in testCases) {
                set.Add(testCase);
                string[] tokens = testCase.Split('-');
                for (int i = 0; i < tokens.Length; i += 1) {
                    prefixes.Add(string.Join("-", tokens[..(i + 1)]));
                }
            }

            Assert.AreEqual(testCases.Count, set.Count);
            foreach (string prefix in prefixes) {
                Assert.IsTrue(set.ContainsPrefix(prefix));
                if (testCases.Contains(prefix)) {
                    Assert.IsTrue(set.Contains(prefix));
                } else {
                    Assert.IsFalse(set.Contains(prefix));
                }
            }
        }
        
        [Test]
        public void TrieSetRemoveStrings_WithoutSeparator_StringRemoved() {
            TrieSet<string, char> set = new TrieSet<string, char>();
            HashSet<string> testCases = new HashSet<string>(1000);
            HashSet<string> removed = new HashSet<string>(500);
            for (int i = 0; i < 1000; i += 1) {
                testCases.Add(Guid.NewGuid().ToString());
            }

            foreach (string testCase in testCases) {
                set.Add(testCase);
            }

            for (int i = 0; i < 500; i += 1) {
                string toRemove = testCases.ElementAt(UnityEngine.Random.Range(0, testCases.Count));
                testCases.Remove(toRemove);
                set.Remove(toRemove);
                removed.Add(toRemove);
            }

            Assert.AreEqual(testCases.Count, set.Count);
            foreach (string test in testCases) {
                for (int i = 0; i < test.Length; i += 1) {
                    string prefix = test[..(i + 1)];
                    Assert.IsTrue(set.ContainsPrefix(prefix));
                    if (testCases.Contains(prefix)) {
                        Assert.IsTrue(set.Contains(prefix));
                    } else {
                        Assert.IsFalse(set.Contains(prefix));
                    }
                }
            }
            
            foreach (string test in removed) {
                Assert.IsFalse(set.Contains(test));
            }
        }
        
        [Test]
        public void TrieSetRemoveStrings_WithSeparator_StringRemoved() {
            TrieSet<string, char> set = new TrieSet<string, char>('-');
            HashSet<string> testCases = new HashSet<string>(1000);
            HashSet<string> removed = new HashSet<string>(500);
            for (int i = 0; i < 1000; i += 1) {
                testCases.Add(Guid.NewGuid().ToString());
            }

            foreach (string testCase in testCases) {
                set.Add(testCase);
            }

            for (int i = 0; i < 500; i += 1) {
                string toRemove = testCases.ElementAt(UnityEngine.Random.Range(0, testCases.Count));
                testCases.Remove(toRemove);
                set.Remove(toRemove);
                removed.Add(toRemove);
            }

            Assert.AreEqual(testCases.Count, set.Count);
            foreach (string test in testCases) {
                string[] tokens = test.Split('-');
                for (int i = 0; i < tokens.Length; i += 1) {
                    string prefix = string.Join("-", tokens[..(i + 1)]);
                    Assert.IsTrue(set.ContainsPrefix(prefix));
                    if (testCases.Contains(prefix)) {
                        Assert.IsTrue(set.Contains(prefix));
                    } else {
                        Assert.IsFalse(set.Contains(prefix));
                    }
                }
            }
            
            foreach (string test in removed) {
                Assert.IsFalse(set.Contains(test));
            }
        }

        [Test]
        public void TrieSetInsertSequences_WithoutSeparator_SplitByCharacter() {
            TrieSet<string[], string> set = new TrieSet<string[], string>();
            HashSet<string[]> testCases = new HashSet<string[]>(1000);
            HashSet<string[]> prefixes = new HashSet<string[]>();
            for (int i = 0; i < 1000; i += 1) {
                string[] testCase = new string[10];
                for (int j = 0; j < 10; j += 1) {
                    testCase[j] = Guid.NewGuid().ToString();
                }

                testCases.Add(testCase);
            }

            foreach (string[] testCase in testCases) {
                set.Add(testCase);
                for (int i = 0; i < testCase.Length; i += 1) {
                    prefixes.Add(testCase[..(i + 1)]);
                }
            }


            Assert.AreEqual(testCases.Count, set.Count);
            foreach (string[] prefix in prefixes) {
                Assert.IsTrue(set.ContainsPrefix(prefix));
                if (testCases.Any(str => prefix.SequenceEqual(str))) {
                    Assert.IsTrue(set.Contains(prefix));
                } else {
                    Assert.IsFalse(set.Contains(prefix), $"Contains {string.Join(',', prefix.Select(token => $"[{token}]"))}");
                }
            }
        }
    }
}
