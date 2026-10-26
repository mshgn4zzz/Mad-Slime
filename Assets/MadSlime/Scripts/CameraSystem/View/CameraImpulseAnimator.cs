using DG.Tweening;
using UnityEngine;
namespace CameraSystem
{
    public abstract class CameraImpulseAnimator : MonoBehaviour
    {
        private float _impulse;
        private Tween _recoverTween;

        protected float Impulse => _impulse;

        protected void PlayRecoverFrom(float newImpulse, float recoverDuration)
        {
            KillRecoverTween();

            _impulse = newImpulse;

            _recoverTween = DOTween.To(ReadImpulse, ApplyImpulse, 0f, recoverDuration)
                .SetEase(Ease.OutQuad)
                .SetTarget(this)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        protected void KillRecoverTween()
        {
            if (_recoverTween == null)
            {
                return;
            }

            _recoverTween.Kill();
            _recoverTween = null;
        }

        private float ReadImpulse()
        {
            return _impulse;
        }

        private void ApplyImpulse(float impulse)
        {
            _impulse = impulse;
        }
    }
}
