using System;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Quota
{
    [RequireComponent(typeof(QuotaPlateAnimator))]
    public class QuotaPlateUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _text;

        private void Awake()
        {
            if (_icon == null || _text == null)
            {
                throw new InvalidOperationException(
                    $"{name}: assign the quota plate references (_icon, _text).");
            }
        }

        public void Setup(Sprite icon, int remaining)
        {
            _icon.sprite = icon;
            UpdateCount(remaining);
        }

        public void UpdateCount(int remaining)
        {
            _text.text = remaining.ToString();
        }
    }
}
