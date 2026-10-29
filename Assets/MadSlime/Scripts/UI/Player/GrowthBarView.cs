using DG.Tweening;
using MadSlime.Items.Data;
using MadSlime.Player.Logic;
using MadSlime.UI.Common;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.UI.Player
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    public class GrowthBarView : MonoBehaviour
    {
        [VContainer.Inject] private Localization _localization;

        [SerializeField] private Image _progressBar;
        [SerializeField] private TMP_Text _tierText;
        [SerializeField, Min(0.01f)] private float _fillSmoothDuration = 0.25f;

        private PlayerTier _playerTier;
        private TierResolver _tierResolver;
        private Tween _fillTween;
        private BarFiller _barFiller;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_playerTier, name, nameof(_playerTier));

            ArgumentGuard.CheckAssigned(_tierResolver, name, nameof(_tierResolver));

            ArgumentGuard.CheckAssigned(_progressBar, name, nameof(_progressBar));

            ArgumentGuard.CheckAssigned(_tierText, name, nameof(_tierText));

            _barFiller = new BarFiller(_progressBar);
        }

        private void OnEnable()
        {
            _playerTier.MassChanged += OnMassChanged;
            _playerTier.TierChanged += OnTierChanged;
        }

        private void Start()
        {
            Refresh(_playerTier.Mass);
        }

        private void OnDisable()
        {
            _playerTier.MassChanged -= OnMassChanged;
            _playerTier.TierChanged -= OnTierChanged;

            if (_fillTween != null)
            {
                _fillTween.Kill();
                _fillTween = null;
            }
        }

        [Inject]
        public void Construct(PlayerTier playerTier, TierResolver tierResolver)
        {
            _playerTier = playerTier;
            _tierResolver = tierResolver;
        }

        private void Refresh(int mass)
        {
            float targetFill = _tierResolver.GetTierProgress(mass);

            _tierText.text = _localization.Get($"tier_{_playerTier.CurrentTier}");

            if (_fillTween != null)
            {
                _fillTween.Kill();
            }

            _fillTween = DOTween.To(ReadFillAmount, ApplyFillAmount, targetFill, _fillSmoothDuration)
                .SetEase(Ease.OutQuad)
                .SetTarget(this);
        }

        private float ReadFillAmount()
        {
            return _progressBar.fillAmount;
        }

        private void ApplyFillAmount(float fillAmount)
        {
            _barFiller.SetFill(fillAmount);
        }

        private void OnMassChanged(int previousMass, int currentMass)
        {
            Refresh(currentMass);
        }

        private void OnTierChanged(SizeTier previousTier, SizeTier currentTier)
        {
            Refresh(_playerTier.Mass);
        }
    }
}
