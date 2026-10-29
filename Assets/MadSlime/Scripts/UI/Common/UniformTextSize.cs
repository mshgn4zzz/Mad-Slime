using MadSlime.Validation;
using TMPro;
using UnityEngine;

namespace MadSlime.UI.Common
{
    [ExecuteAlways]
    public class UniformTextSize : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] _texts;

        private string[] _contents;
        private Vector2[] _sizes;
        private bool[] _visibility;
        private float[] _maximumSizes;
        private bool _isRecalculationRequired;

        private void Awake()
        {
            InitializeCache();
        }

        private void OnEnable()
        {
            _isRecalculationRequired = true;
        }

        private void OnValidate()
        {
            _isRecalculationRequired = true;
        }

        private void LateUpdate()
        {
            if (_texts == null || _contents == null || _contents.Length != _texts.Length)
            {
                InitializeCache();
                _isRecalculationRequired = true;
            }

            if (IsRefreshRequired() == false)
            {
                return;
            }

            float sharedSize = float.MaxValue;

            foreach (TMP_Text text in _texts)
            {
                if (text.isActiveAndEnabled == false)
                {
                    continue;
                }

                text.enableAutoSizing = true;
                text.ForceMeshUpdate();
                sharedSize = Mathf.Min(sharedSize, text.fontSize);
            }

            if (sharedSize < float.MaxValue)
            {
                foreach (TMP_Text text in _texts)
                {
                    text.enableAutoSizing = false;
                    text.fontSize = sharedSize;
                }
            }

            RememberState();
        }

        private void InitializeCache()
        {
            ArgumentGuard.CheckNotEmpty(_texts, name, nameof(_texts));

            foreach (TMP_Text text in _texts)
            {
                ArgumentGuard.CheckAssigned(text, name, nameof(text));
            }

            _contents = new string[_texts.Length];
            _sizes = new Vector2[_texts.Length];
            _visibility = new bool[_texts.Length];
            _maximumSizes = new float[_texts.Length];
        }

        private bool IsRefreshRequired()
        {
            if (_isRecalculationRequired)
            {
                return true;
            }

            for (int i = 0; i < _texts.Length; i++)
            {
                TMP_Text text = _texts[i];

                if (_contents[i] != text.text || _sizes[i] != text.rectTransform.rect.size
                    || _visibility[i] != text.isActiveAndEnabled || _maximumSizes[i] != text.fontSizeMax)
                {
                    return true;
                }
            }

            return false;
        }

        private void RememberState()
        {
            for (int i = 0; i < _texts.Length; i++)
            {
                TMP_Text text = _texts[i];

                _contents[i] = text.text;
                _sizes[i] = text.rectTransform.rect.size;
                _visibility[i] = text.isActiveAndEnabled;
                _maximumSizes[i] = text.fontSizeMax;
            }

            _isRecalculationRequired = false;
        }
    }
}
