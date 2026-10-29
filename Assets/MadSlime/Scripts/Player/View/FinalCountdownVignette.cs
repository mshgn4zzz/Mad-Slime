using DG.Tweening;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.Player.View
{
    public class FinalCountdownVignette : MonoBehaviour
    {
        [SerializeField] private Image _vignetteImage;

        [SerializeField, Min(0.05f)] private float _pulseDuration = 0.4f;

        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.8f;

        private Tween _pulseTween;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_vignetteImage, name, nameof(_vignetteImage));

            _vignetteImage.enabled = false;
        }

        private void OnDisable()
        {
            StopPulse();
        }

        public void StartPulse()
        {
            if (_pulseTween != null)
            {
                return;
            }

            _vignetteImage.enabled = true;

            _pulseTween = DOTween.To(ReadIntensity, ApplyIntensity, _maxAlpha, _pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetTarget(this)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        public void StopPulse()
        {
            if (_pulseTween != null)
            {
                _pulseTween.Kill();
                _pulseTween = null;
            }

            if (_vignetteImage == null)
            {
                return;
            }

            _vignetteImage.enabled = false;
            ApplyIntensity(0f);
        }

        private float ReadIntensity()
        {
            return _vignetteImage.color.a;
        }

        private void ApplyIntensity(float intensity)
        {
            _vignetteImage.color = new Color(1f, 1f, 1f, intensity);
        }
    }
}
