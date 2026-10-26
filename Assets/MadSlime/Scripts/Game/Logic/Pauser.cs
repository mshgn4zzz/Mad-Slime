using Ads;
using Saves;
using System;
using UnityEngine;
using VContainer;
namespace Game
{
    [DisallowMultipleComponent]
    public class Pauser : MonoBehaviour
    {
        private IAdsService _adsService;
        private IGameVisibility _visibility;
        private ICollectedItemsStorage _collectedItems;
        private int _pauseRequestCount;
        private bool _hasVisibilityPause;
        private bool _isPlatformPaused;

        public event Action StateChanged;

        public bool IsPaused()
        {
            return _pauseRequestCount > 0 || _isPlatformPaused;
        }

        [Inject]
        public void Construct(IAdsService adsService, IGameVisibility visibility, ICollectedItemsStorage collectedItems)
        {
            _adsService = adsService;
            _visibility = visibility;
            _collectedItems = collectedItems;
        }

        private void OnEnable()
        {
            _visibility.Hidden += OnHidden;
            _visibility.Shown += OnShown;
            _adsService.PauseGameChanged += OnAdsPauseGame;
            _isPlatformPaused = _adsService.IsPauseGame;
            ApplyState();
        }

        private void OnDisable()
        {
            _visibility.Hidden -= OnHidden;
            _visibility.Shown -= OnShown;
            _adsService.PauseGameChanged -= OnAdsPauseGame;
        }

        public void RequestPause()
        {
            _pauseRequestCount++;
            ApplyState();
        }

        public void RequestResume()
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

            if (_hasVisibilityPause == true)
            {
                _pauseRequestCount = 1;
            }

            ApplyState();
        }

        private void OnHidden()
        {
            if (_hasVisibilityPause == true)
            {
                return;
            }

            _hasVisibilityPause = true;
            RequestPause();
            _collectedItems.CommitCollectedItems();
        }

        private void OnShown()
        {
            if (_hasVisibilityPause == false)
            {
                return;
            }

            _hasVisibilityPause = false;
            RequestResume();
        }

        private void OnAdsPauseGame(bool isPaused)
        {
            _isPlatformPaused = isPaused;
            ApplyState();
        }

        private void ApplyState()
        {
            AudioListener.pause = _hasVisibilityPause || _isPlatformPaused;

            if (IsPaused() == true)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }

            StateChanged?.Invoke();
        }
    }
}
