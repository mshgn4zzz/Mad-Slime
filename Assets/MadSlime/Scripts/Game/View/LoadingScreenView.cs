using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Shop;
using Skins;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private ShopContent _skinCatalog;
        [SerializeField] private Image _outline;
        [SerializeField] private Image _fill;

        [SerializeField, Min(0.01f)] private float _fillDuration = 0.35f;
        [SerializeField] private Ease _fillEase = Ease.OutSine;

        private float _progress;
        private float _displayedProgress;
        private Tween _fillTween;

        private void Awake()
        {
            if (_skinCatalog == null || _outline == null || _fill == null)
            {
                throw new InvalidOperationException($"{name}: skin catalog and both loading images must be assigned.");
            }

            _fill.fillAmount = 0f;
        }

        public void SetSkin(string skinId)
        {
            SkinItem skin = FindSkin(skinId);

            if (skin == null)
            {
                skin = FindSkin(SkinInventory.InitialSkinId);
            }

            if (skin == null || skin.Icon == null)
            {
                                throw new InvalidOperationException($"{name}: the initial Slime icon must exist in "
                                        + $"the loading skin catalog.");
            }

            _outline.sprite = skin.Icon;
            _fill.sprite = skin.Icon;
        }

        public void SetProgress(float progress)
        {
            float target = Mathf.Clamp01(progress);

            if (target <= _progress)
            {
                return;
            }

            _progress = target;
            _fillTween?.Kill();
            _fillTween = DOTween.To(GetDisplayedProgress, SetDisplayedProgress, _progress, _fillDuration)
                .SetEase(_fillEase)
                .SetUpdate(true);
        }

        public async UniTask CompleteAsync(CancellationToken cancellationToken)
        {
            SetProgress(1f);
            await UniTask.WaitUntil(IsFilled, cancellationToken: cancellationToken);
        }

        private float GetDisplayedProgress()
        {
            return _displayedProgress;
        }

        private void SetDisplayedProgress(float progress)
        {
            _displayedProgress = Mathf.Clamp(progress, _displayedProgress, _progress);
            _fill.fillAmount = _displayedProgress;
        }

        private bool IsFilled()
        {
            return _displayedProgress >= 1f;
        }

        private void OnDestroy()
        {
            _fillTween?.Kill();
        }

        private SkinItem FindSkin(string skinId)
        {
            for (int i = 0; i < _skinCatalog.SkinItems.Count; i++)
            {
                SkinItem skin = _skinCatalog.SkinItems[i];

                if (skin != null && skin.Id == skinId)
                {
                    return skin;
                }
            }

            return null;
        }
    }
}
