using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MadSlime.Shop.Data;
using MadSlime.Skins.Data;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.Game.View
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
            ArgumentGuard.CheckAssigned(_skinCatalog, name, nameof(_skinCatalog));
            ArgumentGuard.CheckAssigned(_outline, name, nameof(_outline));
            ArgumentGuard.CheckAssigned(_fill, name, nameof(_fill));

            _fill.fillAmount = 0f;
        }

        private void OnDestroy()
        {
            _fillTween?.Kill();
        }

        public void SetSkin(string skinId)
        {
            SkinItem skin = FindSkin(skinId);

            if (skin == null)
            {
                skin = FindSkin(SkinInventory.InitialSkinId);
            }

            ArgumentGuard.CheckAssigned(skin, name, nameof(skin));
            ArgumentGuard.CheckAssigned(skin.Icon, name, nameof(skin.Icon));

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
