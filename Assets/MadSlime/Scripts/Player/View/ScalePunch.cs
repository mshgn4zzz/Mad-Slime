using DG.Tweening;
using MadSlime.Player.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Player.View
{
    public class ScalePunch : MonoBehaviour
    {
        private const float FullProgress = 1f;
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private ModelDeformer _deformer;

        private float _progress;
        private Tween _squashTween;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));

            ArgumentGuard.CheckNotEmpty(_config.SquashCurve, name, nameof(_config.SquashCurve));

            ArgumentGuard.CheckAssigned(_deformer, name, nameof(_deformer));
        }

        private void OnDisable()
        {
            KillTween();

            _progress = 0f;

            if (_deformer != null)
            {
                _deformer.SetCollectCompression(0f);
            }
        }

        public void Punch()
        {
            KillTween();

            _squashTween = DOTween.To(ReadProgress, ApplyProgress, FullProgress, _config.SquashDuration)
                .SetEase(Ease.Linear)
                .SetTarget(this)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(OnSquashCompleted);
        }

        private float ReadProgress()
        {
            return _progress;
        }

        private void ApplyProgress(float progress)
        {
            _progress = progress;

            float compression = 1f - Mathf.Clamp01(_config.SquashCurve.Evaluate(progress));

            _deformer.SetCollectCompression(compression);
        }

        private void KillTween()
        {
            if (_squashTween == null)
            {
                return;
            }

            _squashTween.Kill();
            _squashTween = null;
        }

        private void OnSquashCompleted()
        {
            _progress = 0f;
            _deformer.SetCollectCompression(0f);
        }
    }
}
