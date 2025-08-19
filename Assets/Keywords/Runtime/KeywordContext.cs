using System.Collections.Generic;
using System.Linq;
using SaintsField;
using UnityEngine;

namespace Runtime {
    [CreateAssetMenu(fileName = "New Keyword Context", menuName = "Keywords/Keyword Context")]
    public class KeywordContext : ScriptableObject {
        [field: SerializeField] internal List<Keyword> Keywords { get; private set; } = new List<Keyword>();

        internal bool Contains(string keyword) {
            Stack<Keyword> stack = new Stack<Keyword>();
            foreach (Keyword word in this.Keywords) {
                stack.Push(word);
            }
            
            Stack<string> path = new Stack<string>();
            while (stack.TryPop(out Keyword curr)) {
                path.Push(curr.Name);
                string label = string.Join(".", path);
                if (label == keyword) {
                    return true;
                }
                
                foreach (Keyword child in curr.Children) {
                    stack.Push(child);
                }
                
                path.Pop();
            }
            
            return false;
        }

        internal AdvancedDropdownList<string> Denumerate() {
            Dictionary<string, AdvancedDropdownList<string>> sections =
                    new Dictionary<string, AdvancedDropdownList<string>>();
            Dictionary<Keyword, string> paths = new Dictionary<Keyword, string>();

            Stack<(Keyword word, string path, string parent)> stack =
                    new Stack<(Keyword word, string path, string parent)>();
            for (int i = this.Keywords.Count - 1; i >= 0; i -= 1) {
                stack.Push((this.Keywords[i], this.Keywords[i].Name, string.Empty));
            }
            
            while (stack.TryPop(out (Keyword word, string path, string parent) curr)) {
                paths.Add(curr.word, curr.path);
                
                if (curr.word.Children.Count == 0) {
                    sections.Add(curr.path, new AdvancedDropdownList<string>(curr.word.Name, curr.path));
                } else {
                    sections.Add(curr.path, new AdvancedDropdownList<string>(curr.word.Name));
                    foreach (Keyword child in curr.word.Children) {
                        stack.Push((child, curr.path + "." + child.Name, curr.path));
                    }
                }

                if (!string.IsNullOrEmpty(curr.parent)) {
                    sections[curr.parent].Add(sections[curr.path]);
                }
            }

            return new AdvancedDropdownList<string>(this.name, this.Keywords.Select(word => sections[paths[word]]));
        }
    }
}
