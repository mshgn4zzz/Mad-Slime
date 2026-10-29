using System;
using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.Audio.View
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [SerializeField] private SfxClip _sfxClip;

        private Button _button;
        private IUISoundPlayer _soundPlayer;

        private void Awake()
        {
            if (TryGetComponent(out _button) == false)
            {
                throw new InvalidOperationException(
                    $"{name}: Button component is missing. UIButtonSound requires the Button "
                    + $"component on the same object.");
            }

            ArgumentGuard.CheckAssigned(_soundPlayer, name, nameof(_soundPlayer));

            ArgumentGuard.CheckAssigned(_sfxClip, name, nameof(_sfxClip));
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }

        [Inject]
        public void Construct(IUISoundPlayer soundPlayer)
        {
            _soundPlayer = soundPlayer;
        }

        private void OnClick()
        {
            _soundPlayer.Play(_sfxClip);
        }
    }
}
