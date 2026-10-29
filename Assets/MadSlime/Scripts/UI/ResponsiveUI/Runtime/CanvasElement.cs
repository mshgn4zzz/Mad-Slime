using System;
using UnityEngine;

namespace MadSlime.UI.ResponsiveUI.Runtime
{
    [Serializable]
    public struct CanvasElement
    {
        public RectTransform Target;
        public Transform PortraitParent;
        public Transform LandscapeParent;
        public CanvasPlacement Portrait;
        public CanvasPlacement Landscape;
    }
}
