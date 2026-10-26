using UnityEngine;
using UnityEngine.UI;
using Game;
namespace UI.Common
{
    public class BarFiller
    {
        private Image _bar;

        public BarFiller(Image bar)
        {
            ArgumentGuard.CheckNotNull(bar, nameof(bar));

            _bar = bar;
        }

        public void SetFill(float fillAmount)
        {
            float clamped = Mathf.Clamp01(fillAmount);
            _bar.fillAmount = clamped;

            if (_bar.type == Image.Type.Sliced)
            {
                RectTransform fill = _bar.rectTransform;
                fill.anchorMax = new Vector2(clamped, 1f);
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;
                _bar.enabled = clamped > 0f;
            }
        }
    }
}
