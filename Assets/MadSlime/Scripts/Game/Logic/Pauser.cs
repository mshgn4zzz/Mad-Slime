using System;
using MadSlime.Ads.Interfaces;
using MadSlime.Game.Interfaces;
using MadSlime.Saves.Interfaces;
using UnityEngine;
using VContainer;

namespace MadSlime.Game.Logic
{
    [DisallowMultipleComponent]
    public class Pauser : MonoBehaviour
    {
        private IAdsService _adsService;
        private IGameVisibility _visibility;
        private ICollectedItemsStorage _collectedItems;
        private int _pauseRequestCount;
        private bool _isVisibilityPaused;
        private bool _isPlatformPaused;

        public event Action StateChanged;

        private void OnEnable()
        {
            _visibility.VisibilitySuspended += OnHidden;
            _visibility.VisibilityRestored += OnShown;
            _adsService.PauseGameChanged += OnAdsPauseGame;
            _isPlatformPaused = _adsService.IsPauseGame;
            ApplyState();
        }

        private void OnDisable()
        {
            _visibility.VisibilitySuspended -= OnHidden;
            _visibility.VisibilityRestored -= OnShown;
            _adsService.PauseGameChanged -= OnAdsPauseGame;
        }

        public bool IsPaused()
        {
            return _pauseRequestCount > 0 || _isPlatformPaused;
        }

        [Inject]
        public void Construct(
            IAdsService adsService,
            IGameVisibility visibility,
            ICollectedItemsStorage collectedItems)
        {
            _adsService = adsService;
            _visibility = visibility;
            _collectedItems = collectedItems;
        }

        public void IncreasePauseRequests()
        {
            _pauseRequestCount++;
            ApplyState();
        }

        public void DecreasePauseRequests()
        {
            if (_pauseRequestCount <= 0)
            {
                return;
            }

            _pauseRequestCount--;
            ApplyState();
        }

        public void ResetToPlay()
        {
            _pauseRequestCount = 0;

            if (_isVisibilityPaused)
            {
                _pauseRequestCount = 1;
            }

            ApplyState();
        }

        private void ApplyState()
        {
            AudioListener.pause = _isVisibilityPaused || _isPlatformPaused;

            if (IsPaused())
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }

            StateChanged?.Invoke();
        }

        private void OnHidden()
        {
            if (_isVisibilityPaused)
            {
                return;
            }

            _isVisibilityPaused = true;
            IncreasePauseRequests();
            _collectedItems.CommitCollectedItems();
        }

        private void OnShown()
        {
            if (_isVisibilityPaused == false)
            {
                return;
            }

            _isVisibilityPaused = false;
            DecreasePauseRequests();
        }

        private void OnAdsPauseGame(bool isPaused)
        {
            _isPlatformPaused = isPaused;
            ApplyState();
        }
    }
}
