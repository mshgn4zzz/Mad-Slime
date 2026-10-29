using MadSlime.Game.Logic;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using VContainer;

namespace MadSlime.UI.HUD
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;

        private Timer _timer;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_timer, name, nameof(_timer));

            ArgumentGuard.CheckAssigned(_timerText, name, nameof(_timerText));
        }

        private void OnEnable()
        {
            _timer.Ticked += OnTimerTicked;
        }

        private void OnDisable()
        {
            _timer.Ticked -= OnTimerTicked;
        }

        [Inject]
        public void Construct(Timer timer)
        {
            _timer = timer;
        }

        private void OnTimerTicked(float remaining)
        {
            float display = Mathf.Max(0f, remaining);

            _timerText.text = $"{display:0.0}";
        }
    }
}
