using System;
using System.Collections;
using System.Collections.Generic;

namespace Keywords.TrieUtils {
    /// <summary>
    /// A set abstract data type implemented using a trie.
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="T">The token type, i.e. the type of the element stored at each trie node.</typeparam>
    public sealed class TrieSet<K, T> : ICollection<K> where K : IEnumerable<T> {
        private sealed class Node {
            public Dictionary<T, Node> Children { get; } = new Dictionary<T, Node>();
            public bool IsEndOfKey { get; private set; }
            public bool IsEndOfToken { get; set; }
            public bool IsLeaf => this.Children.Count == 0;
            public K Value { get; private set; }

            public void Empty() {
                this.IsEndOfKey = false;
                this.IsEndOfToken = false;
                this.Children.Clear();
            }

            public void Put(K value) {
                this.Value = value;
                this.IsEndOfKey = true;
                this.IsEndOfToken = true;
            }

            public void EraseKey() {
                this.Value = default;
                this.IsEndOfKey = false;
            }
        }

        private Node Root { get; } = new Node();
        private T Separator { get; }
        private bool HasSeparator { get; }
        public int Count { get; private set; }
        public bool IsReadOnly => false;

        public TrieSet() {
            this.HasSeparator = false;
        }

        /// <summary>
        /// Create a TrieSet with a pre-defined separator token.
        /// Elements in prefix sequences of a key are produced by splitting at separators.
        /// </summary>
        /// <typeparam name="K">The type of keys stored in the trie set,
        /// must implement <see cref="IEnumerable{T}"/>.</typeparam>
        /// <typeparam name="T">The type of elements used to form the keys.</typeparam>
        public TrieSet(T separator) {
            this.Separator = separator;
            this.HasSeparator = true;
        }

        public IEnumerator<K> GetEnumerator() {
            return enumerate(this.Root).GetEnumerator();
            
            IEnumerable<K> enumerate(Node node) {
                if (node.IsEndOfKey) {
                    yield return node.Value;
                }

                foreach (Node child in node.Children.Values) {
                    foreach (K key in enumerate(child)) {
                        yield return key;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public void Add(K key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            Node curr = this.Root;
            foreach (T element in key) {
                if (!this.HasSeparator) {
                    curr.IsEndOfToken = true;
                } else if (EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    curr.IsEndOfToken = true;
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Node next)) {
                    next = new Node(); 
                    curr.Children.Add(element, next);
                }

                curr = next;
            }

            if (curr.IsEndOfKey) {
                return; // The key already exists.
            }

            curr.Put(key);
            this.Count += 1;
        }

        public void Clear() {
            this.Root.Empty();
            this.Count = 0;
        }

        /// <summary>
        /// Checks whether the specified key exists in the trie set.
        /// Note that this is different from <see cref="ContainsPrefix"/>.
        /// A proper prefix of an existing key is not considered as present!
        /// </summary>
        /// <param name="key">The key to search for in the trie set.</param>
        /// <returns>True if the key exists in the trie set; otherwise, false.</returns>
        public bool Contains(K key) {
            if (key == null) {
                return false;
            }

            Node curr = this.Root;
            foreach (T element in key) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Node next)) {
                    return false;
                }

                curr = next;
            }

            return curr.IsEndOfKey;
        }

        /// <summary>
        /// Determines whether the trie set contains any keys with the given prefix.
        /// Unlike <see cref="Contains"/>, this method checks if the specified prefix matches the beginning
        /// of at least one key in the trie set.
        /// </summary>
        /// <param name="prefix">The sequence of elements representing the prefix to search for in the trie set.</param>
        /// <returns>True if the prefix exists at the start of any key in the trie set; otherwise, false.</returns>
        public bool ContainsPrefix(IEnumerable<T> prefix) {
            if (prefix == null) {
                return false;
            }

            Node curr = this.Root;
            foreach (T element in prefix) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Node next)) {
                    return false;
                }

                curr = next;
            }

            return curr.IsEndOfToken;
        }

        public void CopyTo(K[] array, int arrayIndex) {
            foreach (K key in this) {
                if (arrayIndex >= array.Length) {
                    break;
                }

                array[arrayIndex] = key;
                arrayIndex += 1;
            }
        }

        public bool Remove(K key) {
            return key != null && removeFrom(this.Root, key.GetEnumerator());

            bool removeFrom(Node node, IEnumerator<T> it) {
                if (!it.MoveNext()) {
                    if (!node.IsEndOfKey) {
                        return false;
                    }

                    node.EraseKey();
                    this.Count -= 1;
                    return true;
                }

                T token = it.Current;
                if (token == null || (this.HasSeparator && EqualityComparer<T>.Default.Equals(token, this.Separator))) {
                    return removeFrom(node, it); // skip separator
                }

                if (!node.Children.TryGetValue(token, out Node next)) {
                    return false;
                }

                bool hasRemoved = removeFrom(next, it);
                if (next.IsLeaf) {
                    node.Children.Remove(token);
                }

                return hasRemoved;
            }
        }
    }
}
