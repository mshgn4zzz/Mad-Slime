using System;
using DG.Tweening;
using UnityEngine;

namespace UI.Animations
{
    public class UiScaleAnimator : MonoBehaviour
    {
        [Tooltip("Контейнер контента окна (панель, но не затемнение): он скейлится при показе и скрытии.")]
        [SerializeField] private RectTransform _content;

        [Tooltip("Длительность показа окна (с).")]
        [SerializeField, Min(0.01f)] private float _showDuration = 0.25f;

        [Tooltip("Длительность скрытия окна (с).")]
        [SerializeField, Min(0.01f)] private float _hideDuration = 0.2f;

        [Tooltip("Масштаб, с которого окно выезжает при показе.")]
        [SerializeField, Range(0.01f, 1f)] private float _showFromScale = 0.6f;

        [Tooltip("Изинг выезда.")]
        [SerializeField] private Ease _showEase = Ease.OutBack;

        [Tooltip("Изинг ухода.")]
        [SerializeField] private Ease _hideEase = Ease.InBack;

        private Vector3 _contentBaseScale;
        private CanvasGroup _canvasGroup;
        private Tween _currentTween;
        private float _currentFactor = 1f;

        public event Action ShowCompleted;

        public event Action HideCompleted;

        private void Awake()
        {
            if (_content == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Content is not assigned. Drag the window's content container (the panel, not the fade dim) into the _content field.");
            }

            _contentBaseScale = _content.localScale;

            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void OnDisable()
        {
            KillTween();

            ApplyFactor(1f);
            ApplyAlpha(1f);
            SetRaycastable(true);
        }

        public void PlayShow()
        {
            KillTween();

            ApplyFactor(_showFromScale);
            ApplyAlpha(0f);
            SetRaycastable(true);

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            sequence.Append(DOTween.To(ReadFactor, ApplyFactor, 1f, _showDuration).SetEase(_showEase));
            sequence.Join(DOTween.To(ReadAlpha, ApplyAlpha, 1f, _showDuration).SetEase(Ease.OutQuad));
            sequence.OnComplete(OnShowCompleted);

            _currentTween = sequence;
        }

        public void PlayHide()
        {
            KillTween();

            SetRaycastable(false);

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);
            sequence.SetLink(gameObject);
            sequence.Append(DOTween.To(ReadFactor, ApplyFactor, 0f, _hideDuration).SetEase(_hideEase));
            sequence.Join(DOTween.To(ReadAlpha, ApplyAlpha, 0f, _hideDuration).SetEase(_hideEase));
            sequence.OnComplete(OnHideCompleted);

            _currentTween = sequence;
        }

        private void KillTween()
        {
            if (_currentTween == null)
            {
                return;
            }

            _currentTween.Kill();
            _currentTween = null;
        }

        private float ReadFactor()
        {
            return _currentFactor;
        }

        private void ApplyFactor(float factor)
        {
            _currentFactor = factor;

            _content.localScale = _contentBaseScale * factor;
        }

        private float ReadAlpha()
        {
            return _canvasGroup.alpha;
        }

        private void ApplyAlpha(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }

        private void SetRaycastable(bool isRaycastable)
        {
            _canvasGroup.interactable = isRaycastable;
            _canvasGroup.blocksRaycasts = isRaycastable;
        }

        private void OnShowCompleted()
        {
            _currentTween = null;

            ShowCompleted?.Invoke();
        }

        private void OnHideCompleted()
        {
            _currentTween = null;

            HideCompleted?.Invoke();
        }
    }
}
