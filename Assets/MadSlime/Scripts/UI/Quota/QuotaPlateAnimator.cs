using DG.Tweening;
using System;
using UnityEngine;
namespace UI.Quota
{
    public class QuotaPlateAnimator : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.5f)] private float _popStrength = 0.12f;
        [SerializeField, Min(0.01f)] private float _popDuration = 0.18f;

        [SerializeField, Min(0.01f)] private float _introDuration = 0.25f;
        [SerializeField, Min(0f)] private float _introSlideOffset = 90f;
        [SerializeField] private Ease _introEase = Ease.OutCubic;
        [SerializeField] private Ease _shiftEase = Ease.OutCubic;
        [SerializeField] private Ease _popEase = Ease.OutQuad;
        [SerializeField] private Ease _removeMoveEase = Ease.InCubic;
        [SerializeField] private Ease _removeScaleEase = Ease.InBack;

        private Vector3 _introPosition;
        private Vector3 _baseScale;
        private Tween _popTween;
        private Tween _moveTween;
        private Sequence _removalTween;

        public event Action<QuotaPlateAnimator> RemovalCompleted;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            Cancel();
        }

        private void OnDestroy()
        {
            Cancel();
        }

        public void PrepareIntro()
        {
            Cancel();
            _introPosition = transform.localPosition;
            transform.localPosition = _introPosition + Vector3.left * _introSlideOffset;
        }

        public void PlayIntro()
        {
            Cancel();
            _moveTween = transform.DOLocalMove(_introPosition, _introDuration)
                .SetEase(_introEase)
                .SetUpdate(true)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        public void MoveTo(Vector3 localPosition, float duration)
        {
            _moveTween?.Kill();
            _moveTween = transform.DOLocalMove(localPosition, duration)
                .SetEase(_shiftEase)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public void PlayPop()
        {
            _popTween?.Kill();
            transform.localScale = _baseScale * (1f + _popStrength);
            _popTween = transform.DOScale(_baseScale, _popDuration)
                .SetEase(_popEase)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public void PlayRemoval(float duration, float slideOffset)
        {
            Cancel();
            Vector3 destination = transform.localPosition + Vector3.left * slideOffset;
            _removalTween = DOTween.Sequence();
            _removalTween.Join(transform.DOLocalMove(destination, duration).SetEase(_removeMoveEase));
            _removalTween.Join(transform.DOScale(Vector3.zero, duration).SetEase(_removeScaleEase));
            _removalTween.SetUpdate(true).SetLink(gameObject).OnComplete(OnRemovalCompleted);
        }

        public void Cancel()
        {
            _popTween?.Kill();
            _moveTween?.Kill();
            _removalTween?.Kill();
            _popTween = null;
            _moveTween = null;
            _removalTween = null;
            transform.localScale = _baseScale;
        }

        private void OnRemovalCompleted()
        {
            _removalTween = null;
            RemovalCompleted?.Invoke(this);
        }
    }
}
