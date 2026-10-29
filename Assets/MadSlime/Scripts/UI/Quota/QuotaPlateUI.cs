using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Quota
{
    [RequireComponent(typeof(QuotaPlateAnimator))]
    public class QuotaPlateUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _text;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_icon, name, nameof(_icon));
            ArgumentGuard.CheckAssigned(_text, name, nameof(_text));
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
