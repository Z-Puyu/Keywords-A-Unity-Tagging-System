using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Keywords.TrieUtils {
    /// <summary>
    /// Represents a generic dictionary-like data structure implemented as a Trie.
    /// Allows efficient storage and retrieval of key-value pairs where keys are sequences of elements.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary. Must implement <see cref="IEnumerable{T}"/>.</typeparam>
    /// <typeparam name="T">The type of elements in the key sequences.</typeparam>
    /// <typeparam name="V">The type of values stored in the dictionary.</typeparam>
    public class TrieDictionary<K, T, V> : IDictionary<K, V> where K : IEnumerable<T> {
        private sealed class Entry {
            public Dictionary<T, Entry> Children { get; } = new Dictionary<T, Entry>();
            public bool IsEndOfKey { get; private set; }
            public bool IsEndOfToken { get; set; }
            public bool IsLeaf => this.Children.Count == 0;
            public K Key { get; private set; }
            public V Value { get; private set; }

            public void Empty() {
                this.IsEndOfKey = false;
                this.IsEndOfToken = false;
                this.Children.Clear();
                this.Key = default;
                this.Value = default;
            }

            public void Put(K key, V value) {
                this.Key = key;
                this.Value = value;
                this.IsEndOfKey = true;
                this.IsEndOfToken = true;
            }

            public void EraseEntry() {
                this.Key = default;
                this.Value = default;
                this.IsEndOfKey = false;
            }
        }

        private Entry Root { get; } = new Entry();
        private T Separator { get; }
        private bool HasSeparator { get; }

        public V this[K key] {
            get {
                if (this.TryGetValue(key, out V value)) {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set => this.Add(key, value);
        }

        public int Count { get; private set; }
        public bool IsReadOnly => false;
        public ICollection<K> Keys => this.Select(kv => kv.Key).ToHashSet();
        public ICollection<V> Values => this.Select(kv => kv.Value).ToHashSet();

        public TrieDictionary() {
            this.HasSeparator = false;
        }

        /// <summary>
        /// Create a TrieDictionary with a pre-defined separator token.
        /// Elements in prefix sequences of a key are produced by splitting at separators.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.
        /// Must implement <see cref="IEnumerable{T}"/>.</typeparam>
        /// <typeparam name="T">The type of elements in the key sequences.</typeparam>
        /// <typeparam name="V">The type of values stored in the dictionary.</typeparam>
        public TrieDictionary(T separator) {
            this.Separator = separator;
            this.HasSeparator = true;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
            return enumerate(this.Root).GetEnumerator();
            
            IEnumerable<KeyValuePair<K, V>> enumerate(Entry node) {
                if (node.IsEndOfKey) {
                    yield return new KeyValuePair<K, V>(node.Key, node.Value);
                }

                foreach (Entry child in node.Children.Values) {
                    foreach (KeyValuePair<K, V> kv in enumerate(child)) {
                        yield return kv;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public void Add(KeyValuePair<K, V> item) {
            this.Add(item.Key, item.Value);
        }

        public void Clear() {
            this.Root.Empty();
            this.Count = 0;
        }

        public bool Contains(KeyValuePair<K, V> item) {
            return this.TryGetValue(item.Key, out V value) && EqualityComparer<V>.Default.Equals(value, item.Value);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
            foreach (KeyValuePair<K, V> kv in this) {
                if (arrayIndex >= array.Length) {
                    break;
                }
                
                array[arrayIndex] = kv;
                arrayIndex += 1;
            }
        }

        public bool Remove(KeyValuePair<K, V> item) {
            if (this.TryGetValue(item.Key, out V value) && EqualityComparer<V>.Default.Equals(value, item.Value)) {
                return this.Remove(item.Key);
            }
            
            return false;
        }

        public void Add(K key, V value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            Entry curr = this.Root;
            foreach (T element in key) {
                if (!this.HasSeparator) {
                    curr.IsEndOfToken = true;
                } else if (EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    curr.IsEndOfToken = true;
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Entry next)) {
                    next = new Entry();
                    curr.Children.Add(element, next);
                }

                curr = next;
            }

            if (!curr.IsEndOfKey) {
                this.Count += 1;
            }
            
            curr.Put(key, value);
        }

        /// <summary>
        /// Determines whether the TrieDictionary contains a specified key.
        /// Note that this is different from <see cref="ContainsKeyPrefix"/>.
        /// A proper prefix of an existing key is not considered as present!
        /// </summary>
        /// <param name="key">The key to locate in the TrieDictionary.</param>
        /// <returns>True if the specified key exists in the TrieDictionary; otherwise, false.</returns>
        public bool ContainsKey(K key) {
            Entry curr = this.Root;
            foreach (T element in key) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }
                
                if (!curr.Children.TryGetValue(element, out Entry next)) {
                    return false;
                }
                
                curr = next;
            }

            return curr.IsEndOfKey;
        }

        public bool Remove(K key) {
            return removeFrom(this.Root, key.GetEnumerator());

            bool removeFrom(Entry node, IEnumerator<T> it) {
                if (!it.MoveNext()) {
                    if (!node.IsEndOfKey) {
                        return false;
                    }

                    node.EraseEntry();
                    this.Count -= 1;
                    return true;
                }

                T token = it.Current;
                if (token == null || (this.HasSeparator && EqualityComparer<T>.Default.Equals(token, this.Separator))) {
                    return removeFrom(node, it);
                }

                if (!node.Children.TryGetValue(token, out Entry next)) {
                    return false;
                }

                bool hasRemoved = removeFrom(next, it);
                if (next.IsLeaf) {
                    node.Children.Remove(token);
                }

                return hasRemoved;
            }
        }

        public bool TryGetValue(K key, out V value) {
            Entry curr = this.Root;
            foreach (T element in key) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Entry next)) {
                    value = default;
                    return false;
                }

                curr = next;
            }

            if (curr.IsEndOfKey) {
                value = curr.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Determines whether the TrieDictionary contains a specified key prefix.
        /// Unlike <see cref="ContainsKey"/>, this method checks if any key in the TrieDictionary
        /// starts with the given prefix, even if the prefix itself is not a full key.
        /// </summary>
        /// <param name="prefix">The prefix to locate in the TrieDictionary.</param>
        /// <returns>True if the specified prefix matches the start of any key in the TrieDictionary;
        /// otherwise, false.</returns>
        public bool ContainsKeyPrefix(IEnumerable<T> prefix) {
            if (prefix == null) {
                return false;
            }

            Entry curr = this.Root;
            foreach (T element in prefix) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Entry next)) {
                    return false;
                }

                curr = next;
            }

            return curr.IsEndOfToken;
        }
    }
}
