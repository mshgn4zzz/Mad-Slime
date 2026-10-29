using System;
using UnityEngine;

namespace MadSlime.UI.ResponsiveUI.Runtime
{
    [Serializable]
    public struct CanvasVisibility
    {
        public GameObject Target;
        [UnityEngine.Serialization.FormerlySerializedAs("Portrait")]
        public bool IsPortraitVisible;
        [UnityEngine.Serialization.FormerlySerializedAs("Landscape")]
        public bool IsLandscapeVisible;
    }
}
