using System;
using Collectables;
using PlayerInput;
using Quota;
using UnityEngine;

namespace Game
{
    public class PlayingState : IGameplayState
    {
        private Timer _timer;
        private QuotaCounter _quotaCounter;
        private ItemCollector _itemCollector;
        private GameplayActivityController _activity;
        private FinalCountdownEffects _effects;
        private Pauser _pauser;
        private IGameplayReporter _reporter;
        private GameplayPlayerSetup _playerSetup;
        private PlayerInputReader _inputReader;
        private float _finalSeconds;
        private bool _isEntered;
        private bool _isCompletionRequested;
        private bool _isCountdownStarted;
        private int _pauseRequests;

        public event Action Started;

        public event Action CompletionRequested;

        public PlayingState(
            Timer timer,
            QuotaCounter quotaCounter,
            ItemCollector itemCollector,
            GameplayActivityController activity,
            FinalCountdownEffects effects,
            Pauser pauser,
            IGameplayReporter reporter,
            GameplayPlayerSetup playerSetup,
            PlayerInputReader inputReader)
        {
            _timer = timer;
            _quotaCounter = quotaCounter;
            _itemCollector = itemCollector;
            _activity = activity;
            _effects = effects;
            _pauser = pauser;
            _reporter = reporter;
            _playerSetup = playerSetup;
            _inputReader = inputReader;
        }

        public void Configure(float finalSeconds)
        {
            _finalSeconds = finalSeconds;
        }

        public void Enter()
        {
            if (_isEntered)
            {
                return;
            }

            _isEntered = true;
            _isCompletionRequested = false;
            _isCountdownStarted = false;
            _timer.Ticked += OnTimerTicked;
            _timer.Finished += OnCompleted;
            _quotaCounter.QuotaCompleted += OnCompleted;
            _pauser.StateChanged += OnPauseStateChanged;
            _quotaCounter.Attach(_itemCollector);
            _reporter.ReportStart();
            _inputReader.enabled = true;
            OnPauseStateChanged();
            _timer.StartCount();
            Started?.Invoke();
        }

        public void Exit()
        {
            if (_isEntered == false)
            {
                return;
            }

            _isEntered = false;
            _timer.Ticked -= OnTimerTicked;
            _timer.Finished -= OnCompleted;
            _quotaCounter.QuotaCompleted -= OnCompleted;
            _pauser.StateChanged -= OnPauseStateChanged;
            _inputReader.enabled = false;
            _activity.SetRunning(false);
            _timer.Stop();
            _itemCollector.StopCollecting();
            _quotaCounter.Detach();
            _effects.Stop();
            _playerSetup.StopSession();

            while (_pauseRequests > 0)
            {
                ResumeByRequest();
            }

            _reporter.ReportStop();
        }

        public void PauseByRequest()
        {
            _pauseRequests++;
            _pauser.RequestPause();
        }

        public void ResumeByRequest()
        {
            if (_pauseRequests > 0)
            {
                _pauseRequests--;
                _pauser.RequestResume();
            }
        }

        private void OnPauseStateChanged()
        {
            _activity.SetRunning(_isEntered && _pauser.IsPaused() == false);
        }

        private void OnTimerTicked(float remainingSeconds)
        {
            if (_isEntered == false)
            {
                return;
            }

            float urgency = 0f;

            if (_finalSeconds > 0f)
            {
                urgency = 1f - Mathf.Clamp01(remainingSeconds / _finalSeconds);
            }

            _effects.SetUrgency(urgency);

            if (_isCountdownStarted == false && remainingSeconds <= _finalSeconds)
            {
                _isCountdownStarted = true;
                _effects.Start();
            }
        }

        private void OnCompleted()
        {
            if (_isEntered && _isCompletionRequested == false)
            {
                _isCompletionRequested = true;
                CompletionRequested?.Invoke();
            }
        }
    }
}
