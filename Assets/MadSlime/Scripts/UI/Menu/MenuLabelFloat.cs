using UnityEngine;

namespace MadSlime.UI.Menu
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class MenuLabelFloat : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _swing = 10f;
        [SerializeField, Min(0.1f)] private float _period = 4.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("_useUnscaledTime")]
        [SerializeField] private bool _isUnscaledTimeUsed = true;

        private RectTransform _rect;
        private Vector2 _restPosition;
        private float _elapsed;

        private void OnEnable()
        {
            _rect = (RectTransform)transform;
            _restPosition = _rect.anchoredPosition;
            _elapsed = 0f;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            if (_isUnscaledTimeUsed)
            {
                deltaTime = Time.unscaledDeltaTime;
            }

            _elapsed += deltaTime;

            float offset = Mathf.Sin(_elapsed * (Mathf.PI * 2f / _period)) * _swing;

            _rect.anchoredPosition = _restPosition + (Vector2.up * offset);
        }

        private void OnDisable()
        {
            if (_rect != null)
            {
                _rect.anchoredPosition = _restPosition;
            }
        }
    }
}
