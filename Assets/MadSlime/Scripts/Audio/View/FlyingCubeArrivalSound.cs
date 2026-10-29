using System;
using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.ShapeFill.Logic;
using MadSlime.ShapeFill.View;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Audio.View
{
    [RequireComponent(typeof(ShapeFiller))]
    public class FlyingCubeArrivalSound : MonoBehaviour
    {
        [SerializeField] private SfxClip _sfxClip;
        [SerializeField, Range(0.5f, 2f)] private float _minPitch = 0.9f;
        [SerializeField, Range(0.5f, 2f)] private float _maxPitch = 1.35f;
        [SerializeField, Min(0f)] private float _minInterval = 0.03f;

        private IGameSoundPlayer _soundPlayer;
        private ShapeFiller _filler;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_soundPlayer, name, nameof(_soundPlayer));

            ArgumentGuard.CheckAssigned(_sfxClip, name, nameof(_sfxClip));

            ArgumentGuard.CheckOrder(_minPitch, _maxPitch, name, nameof(_minPitch));

            if (TryGetComponent(out _filler) == false)
            {
                throw new InvalidOperationException($"{name}: ShapeFiller is required.");
            }
        }

        private void OnEnable()
        {
            _filler.CubeArrived += OnFillerCubeArrived;
        }

        private void OnDisable()
        {
            _filler.CubeArrived -= OnFillerCubeArrived;
        }

        [Inject]
        public void Construct(IGameSoundPlayer soundPlayer)
        {
            _soundPlayer = soundPlayer;
        }

        private void OnFillerCubeArrived(CubeFlightAnimator cube)
        {
            float pitch = Mathf.Lerp(_minPitch, _maxPitch, _filler.GetFillFraction());

            _soundPlayer.Play(_sfxClip, pitch, _minInterval);
        }
    }
}
