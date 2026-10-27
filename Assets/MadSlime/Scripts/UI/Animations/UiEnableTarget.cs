using System;
using UnityEngine;
namespace UI.Animations
{
    [Serializable]
    public class UiEnableTarget
    {
        [SerializeField] private ReactiveUIView _view;

        [SerializeField, Min(0f)] private float _delay;

        public ReactiveUIView View => _view;
        public float Delay => _delay;
    }
}
