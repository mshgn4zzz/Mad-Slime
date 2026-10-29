using System;
using UnityEngine;

namespace MadSlime.UI.ResponsiveUI.Runtime
{
    [Serializable]
    public struct CanvasPlacement
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 Pivot;
        public Vector2 Position;
        public Vector2 Size;
        public float Scale;

        public void Apply(RectTransform target)
        {
            target.anchorMin = AnchorMin;
            target.anchorMax = AnchorMax;
            target.pivot = Pivot;
            target.sizeDelta = Size;
            target.anchoredPosition = Position;

            float scale = 1f;

            if (Scale > 0f)
            {
                scale = Scale;
            }

            target.localScale = Vector3.one * scale;
        }
    }
}
