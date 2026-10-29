using System;
using System.Collections.Generic;
using DG.Tweening;
using MadSlime.Roulette.Data;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MadSlime.UI.Roulette
{
    public class RouletteWheel : MonoBehaviour
    {
        private const float FullCircleDegrees = 360f;
        private const float SettleSwingSeconds = 0.2f;
        private const float WinPunchStrength = 0.12f;
        private const float MaxWinPunchDurationSeconds = 0.5f;
        private const int WinPunchVibrato = 4;
        private const float WinPunchElasticity = 0.5f;

        [SerializeField]
        private RectTransform _spinContainer;
        [SerializeField]
        private RectTransform _arrow;
        [SerializeField]
        private List<Image> _sectorIcons = new List<Image>();
        [SerializeField]
        private List<TMP_Text> _sectorLabels = new List<TMP_Text>();

        private RouletteConfig _config;
        private float[] _sectorAngles;
        private float _arrowAngle;
        private float _averageSectorAngle;
        private float _rotation;
        private int _lastTickIndex;
        private int _pendingTargetIndex;
        private bool _isSpinning;
        private bool _isBuilt;
        private RectTransform _punchedIcon;
        private Vector3 _iconScaleBeforePunch;

        public event Action Ticked;

        public event Action Completed;

        public int SectorCount => _sectorIcons.Count;

        private void Update()
        {
            if (_isBuilt == false || _isSpinning)
            {
                return;
            }

            if (_rotation <= -FullCircleDegrees)
            {
                _rotation %= FullCircleDegrees;
                _lastTickIndex = Mathf.FloorToInt((_arrowAngle - _rotation) / _averageSectorAngle);
            }

            ApplyRotation(_rotation - (_config.IdleRotationSpeed * Time.unscaledDeltaTime));
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Setup(RouletteConfig config)
        {
            _config = config;
        }

        public void Build(IReadOnlyList<RouletteSectorIcon> sectors)
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: RouletteWheel.Setup was not called. Pass the RouletteConfig before building.");
            }

            ArgumentGuard.CheckAssigned(_spinContainer, name, nameof(_spinContainer));
            ArgumentGuard.CheckAssigned(_arrow, name, nameof(_arrow));

            ArgumentGuard.CheckNotEmpty(sectors, name, nameof(sectors));

            ArgumentGuard.CheckNotEmpty(_sectorIcons, name, nameof(_sectorIcons));

            if (_sectorLabels.Count != _sectorIcons.Count)
            {
                throw new InvalidOperationException(
                    $"{name}: label count {_sectorLabels.Count} must match icon slot count {_sectorIcons.Count}.");
            }

            _sectorAngles = new float[_sectorIcons.Count];

            for (int i = 0; i < _sectorIcons.Count; i++)
            {
                Image sectorIcon = _sectorIcons[i];
                TMP_Text sectorLabel = _sectorLabels[i];

                ArgumentGuard.CheckAssigned(sectorIcon, name, nameof(sectorIcon));

                ArgumentGuard.CheckAssigned(sectorLabel, name, nameof(sectorLabel));

                Vector2 slotOffset = sectorIcon.rectTransform.anchoredPosition;

                ArgumentGuard.CheckPositive(slotOffset.sqrMagnitude, name, nameof(slotOffset.sqrMagnitude));

                _sectorAngles[i] = Mathf.Atan2(slotOffset.y, slotOffset.x) * Mathf.Rad2Deg;
            }

            Vector2 arrowOffset = _arrow.anchoredPosition;

            ArgumentGuard.CheckPositive(arrowOffset.sqrMagnitude, name, nameof(arrowOffset.sqrMagnitude));

            _arrowAngle = Mathf.Atan2(arrowOffset.y, arrowOffset.x) * Mathf.Rad2Deg;
            _averageSectorAngle = FullCircleDegrees / _sectorAngles.Length;

            if (sectors.Count != _sectorIcons.Count)
            {
                throw new InvalidOperationException($"{name}: rewards and wheel sectors must have the same count.");
            }

            for (int i = 0; i < _sectorIcons.Count; i++)
            {
                RouletteSectorIcon sector = sectors[i];

                ArgumentGuard.CheckAssigned(sector.Icon, name, nameof(sector.Icon));

                _sectorIcons[i].sprite = sector.Icon;
                _sectorIcons[i].preserveAspect = true;
                ApplySectorLabel(_sectorLabels[i], sector.Label);
            }

            ApplyRotation(0f);
            _lastTickIndex = 0;
            _isBuilt = true;
        }

        public void Spin(int targetIndex)
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: RouletteWheel.Setup was not called. Pass the RouletteConfig before spinning.");
            }

            if (_isBuilt == false)
            {
                throw new InvalidOperationException(
                    $"{name}: RouletteWheel.Spin was called before Build.");
            }

            if (_isSpinning)
            {
                throw new InvalidOperationException(
                    $"{name}: RouletteWheel.Spin was called while already spinning.");
            }

            ArgumentGuard.CheckRange(targetIndex, 0, _sectorAngles.Length - 1, nameof(targetIndex));

            Stop();
            _isSpinning = true;
            _pendingTargetIndex = targetIndex;

            int turns = Random.Range(_config.MinTurns, _config.MaxTurns + 1);
            float targetAngle = GetPositiveRemainder(_arrowAngle - _sectorAngles[targetIndex], FullCircleDegrees);
            float windBackRotation = _rotation + _config.WindBackDegrees;
            float delta = GetPositiveRemainder(windBackRotation - targetAngle, FullCircleDegrees);
            float targetRotation = windBackRotation - ((turns * FullCircleDegrees) + delta);

            Sequence sequence = DOTween.Sequence();

            sequence.SetTarget(this);
            sequence.SetUpdate(true);
            sequence.SetLink(gameObject, LinkBehaviour.KillOnDisable);

            sequence.Append(DOTween.To(ReadRotation, ApplyRotation, windBackRotation, _config.WindBackDuration)
                .SetEase(Ease.InOutQuad));

            float maxDeviation = _config.SettleMaxDeviationDegrees;

            if (maxDeviation > 0f)
            {
                float deviation = Random.Range(0f, maxDeviation);
                float settleRotation = targetRotation - deviation;

                if (Random.Range(0, 2) == 0)
                {
                    settleRotation = targetRotation + deviation;
                }

                sequence.Append(DOTween.To(ReadRotation, ApplyRotation, settleRotation, _config.SpinDuration)
                    .SetEase(EvaluateSpinEase));
                sequence.Append(DOTween.To(ReadRotation, ApplyRotation, targetRotation, SettleSwingSeconds)
                    .SetEase(Ease.OutQuad));
            }
            else
            {
                sequence.Append(DOTween.To(ReadRotation, ApplyRotation, targetRotation, _config.SpinDuration)
                    .SetEase(EvaluateSpinEase));
            }

            sequence.OnComplete(OnSpinCompleted);
        }

        public void Stop()
        {
            DOTween.Kill(this);

            if (_punchedIcon != null)
            {
                _punchedIcon.localScale = _iconScaleBeforePunch;
                _punchedIcon = null;
            }

            _isSpinning = false;
        }

        private void ApplySectorLabel(TMP_Text label, string labelText)
        {
            if (string.IsNullOrEmpty(labelText) == false)
            {
                label.text = labelText;
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }
        }

        private void ApplyRotation(float rotation)
        {
            _rotation = rotation;
            _spinContainer.localRotation = Quaternion.Euler(0f, 0f, _rotation);
            EmitTick();
        }

        private float ReadRotation()
        {
            return _rotation;
        }

        private float EvaluateSpinEase(float time, float duration, float overshootOrAmplitude, float period)
        {
            float normalized = time / duration;

            return 1f - Mathf.Pow(1f - normalized, _config.SpinEasePower);
        }

        private void EmitTick()
        {
            int tickIndex = Mathf.FloorToInt((_arrowAngle - _rotation) / _averageSectorAngle);

            if (tickIndex == _lastTickIndex)
            {
                return;
            }

            _lastTickIndex = tickIndex;
            Ticked?.Invoke();
        }

        private void PunchWinningIcon()
        {
            float punchDuration = Mathf.Min(_config.WinDwellSeconds, MaxWinPunchDurationSeconds);

            _punchedIcon = _sectorIcons[_pendingTargetIndex].rectTransform;
            _iconScaleBeforePunch = _punchedIcon.localScale;

            _punchedIcon
                .DOPunchScale(
                new Vector3(WinPunchStrength, WinPunchStrength, 0f),
                punchDuration,
                WinPunchVibrato,
                WinPunchElasticity)
                .SetTarget(this)
                .SetUpdate(true)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        private float GetPositiveRemainder(float value, float modulus)
        {
            float remainder = value % modulus;

            if (remainder < 0f)
            {
                remainder += modulus;
            }

            return remainder;
        }

        private void OnSpinCompleted()
        {
            float targetAngle = GetPositiveRemainder(
                _arrowAngle - _sectorAngles[_pendingTargetIndex],
                FullCircleDegrees);

            _rotation = targetAngle
                + (FullCircleDegrees * Mathf.RoundToInt((_rotation - targetAngle) / FullCircleDegrees));

            ApplyRotation(_rotation);
            PunchWinningIcon();
            Completed?.Invoke();
        }
    }
}
