using System;
using System.Collections.Generic;
using SaintsField;
using UnityEngine;

namespace Runtime {
    [Serializable]
    internal class Keyword {
        [field: SerializeField] internal string Name { get; private set; }
        [field: SerializeField] internal List<Keyword> Children { get; private set; }
    }
}
