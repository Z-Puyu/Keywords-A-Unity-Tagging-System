using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Keywords.TrieUtils;
using SaintsField;
using UnityEngine;

namespace Runtime {
    [DisallowMultipleComponent]
    public sealed class KeywordRegistry : MonoBehaviour, ICollection<string> {
        [field: SerializeField] private KeywordContext KeywordContext { get; set; }
        
        [field: SerializeField, AdvancedDropdown(nameof(this.GetKeywords))] 
        private List<string> PreRegisteredKeywords { get; set; } = new List<string>();
        
        private TrieSet<string, char> TrieSet { get; set; } = new TrieSet<string, char>('.');
        
        public int Count => this.TrieSet.Count;
        public bool IsReadOnly => false;

        private AdvancedDropdownList<string> GetKeywords() {
            return this.KeywordContext.Denumerate();
        }
        
        private void Awake() {
            foreach (string keyword in this.PreRegisteredKeywords.Distinct()) {
                this.TrieSet.Add(keyword);
            }
        }

        public void Clear() {
            this.TrieSet.Clear();
        }

        /// <summary>
        /// Determines whether a given keyword exists as a prefix in the registered keywords.
        /// </summary>
        /// <param name="keyword">The keyword to search for as a prefix.</param>
        /// <returns>True if the specified keyword exists as a prefix; otherwise, false.</returns>
        public bool Contains(string keyword) {
            if (this.TrieSet.ContainsPrefix(keyword)) {
                return true;
            }

            if (!this.Defines(keyword)) {
                Debug.LogWarning($"Trying to search for undefined keyword {keyword} in {this.gameObject.name}!", this);
            }
            
            return false;
        }

        public void CopyTo(string[] array, int arrayIndex) {
            this.TrieSet.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Determines whether a given keyword exists exactly as it is in the registered keywords.
        /// </summary>
        /// <param name="keyword">The keyword to search for an exact match in the registry.</param>
        /// <returns>True if the specified keyword exists exactly; otherwise, false.</returns>
        public bool ContainsExactly(string keyword) {
            if (this.TrieSet.Contains(keyword)) {
                return true;
            }

            if (!this.Defines(keyword)) {
                Debug.LogWarning($"Trying to search for undefined keyword {keyword} in {this.gameObject.name}!", this);
            }
            
            return false;
        }

        /// <summary>
        /// Determines whether any of the specified keywords exist in the registered keywords.
        /// </summary>
        /// <param name="keywords">A collection of keywords to search for in the registry.</param>
        /// <returns>True if at least one of the specified keywords exists in the registry; otherwise, false.</returns>
        public bool ContainsAny(IEnumerable<string> keywords) {
            return keywords.Any(this.Contains);
        }

        /// <summary>
        /// Determines whether all the specified keywords exist in the registered keywords.
        /// </summary>
        /// <param name="keywords">A collection of keywords to search for in the registry.</param>
        /// <returns>True if all the specified keywords exist in the registry; otherwise, false.</returns>
        public bool ContainsAll(IEnumerable<string> keywords) {
            return keywords.All(this.Contains);
        }

        /// <summary>
        /// Determines whether any of the specified keywords exist exactly as they are in the registered keywords.
        /// </summary>
        /// <param name="keywords">A collection of keywords to check for exact matches in the registry.</param>
        /// <returns>True if at least one of the specified keywords exists exactly; otherwise, false.</returns>
        public bool ContainsExactlyAny(IEnumerable<string> keywords) {
            return keywords.Any(this.ContainsExactly);
        }

        /// <summary>
        /// Determines whether all the specified keywords exist exactly as they are in the registered keywords.
        /// </summary>
        /// <param name="keywords">A collection of keywords to check for exact matches in the registry.</param>
        /// <returns>True if all the specified keywords exist exactly; otherwise, false.</returns>
        public bool ContainsExactlyAll(IEnumerable<string> keywords) {
            return keywords.All(this.ContainsExactly);
        }

        /// <summary>
        /// Adds a specified keyword to the registry if it is defined in the associated keyword context.
        /// </summary>
        /// <param name="keyword">The keyword to add to the registry.</param>
        public void Add(string keyword) {
            if (!this.Defines(keyword)) {
                Debug.LogError($"Trying to add undefined keyword {keyword} to {this.gameObject.name}!", this);
                return;
            }
            
            this.TrieSet.Add(keyword);
        }

        /// <summary>
        /// Removes the specified keyword from the registry if it exists.
        /// </summary>
        /// <param name="keyword">The keyword to be removed from the registry.</param>
        /// <returns>True if the keyword was successfully removed; otherwise, false.</returns>
        public bool Remove(string keyword) {
            if (this.TrieSet.Remove(keyword)) {
                return true;
            }

            if (!this.Defines(keyword)) {
                Debug.LogWarning($"Trying to remove undefined keyword {keyword} from {this.gameObject.name}!", this);
            }
            
            return false;
        }

        /// <summary>
        /// Determines if the specified keyword is defined in the keyword context.
        /// </summary>
        /// <param name="keyword">The keyword to check for definition.</param>
        /// <returns>True if the keyword is defined in the keyword context; otherwise, false.</returns>
        public bool Defines(string keyword) {
            return this.KeywordContext.Contains(keyword);
        }

        private void OnDestroy() {
            this.TrieSet.Clear();
        }

        public IEnumerator<string> GetEnumerator() {
            return this.TrieSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}
