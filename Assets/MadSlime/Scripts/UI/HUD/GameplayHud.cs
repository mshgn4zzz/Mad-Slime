using Game;
using System;
using System.Collections.Generic;
using UI.Animations;
using UnityEngine;
using VContainer;
namespace UI.HUD
{
    public class GameplayHud : MonoBehaviour
    {
        [SerializeField] private UiEnableTarget[] _sceneStartTargets;
        [SerializeField] private UiEnableTarget[] _sessionStartTargets;
        [SerializeField] private AnimationScheduler _sceneScheduler;
        [SerializeField] private AnimationScheduler _gameplayScheduler;

        private GameplayOrchestrator _session;
        private List<ReactiveUIView> _views = new List<ReactiveUIView>();
        private List<Action> _sceneAnimations = new List<Action>();
        private List<Action> _gameplayAnimations = new List<Action>();
        private List<float> _sceneDelays = new List<float>();
        private List<float> _gameplayDelays = new List<float>();
        private bool _isGameplayShown;

        [Inject]
        public void Construct(GameplayOrchestrator session)
        {
            _session = session;
        }

        private void Awake()
        {
            if (_sceneScheduler == null || _gameplayScheduler == null)
            {
                throw new InvalidOperationException($"{name}: assign both animation schedulers.");
            }

            RegisterTargets(_sceneStartTargets, _sceneAnimations, _sceneDelays);
            RegisterTargets(_sessionStartTargets, _gameplayAnimations, _gameplayDelays);
        }

        private void OnEnable()
        {
            _session.SceneSessionPrepared += OnPrepared;
            _session.SceneSessionStarted += OnStarted;
            _session.GameplayStarted += OnGameplayStarted;
            _session.SessionFinished += OnFinished;

            foreach (ReactiveUIView view in _views)
            {
                view.DisableCompleted += OnTargetHidden;
            }
        }

        private void OnDisable()
        {
            _session.SceneSessionPrepared -= OnPrepared;
            _session.SceneSessionStarted -= OnStarted;
            _session.GameplayStarted -= OnGameplayStarted;
            _session.SessionFinished -= OnFinished;
            CancelSequences();

            foreach (ReactiveUIView view in _views)
            {
                view.DisableCompleted -= OnTargetHidden;
            }
        }

        private void OnPrepared()
        {
            CancelSequences();
            _isGameplayShown = false;

            foreach (ReactiveUIView view in _views)
            {
                view.ResetState(false);
                view.gameObject.SetActive(false);
            }
        }

        private void OnStarted()
        {
            _sceneScheduler.Play(_sceneAnimations, _sceneDelays);
        }

        private void OnGameplayStarted()
        {
            if (_isGameplayShown == true)
            {
                return;
            }

            _isGameplayShown = true;
            _gameplayScheduler.Play(_gameplayAnimations, _gameplayDelays);
        }

        private void OnFinished()
        {
            CancelSequences();

            foreach (ReactiveUIView view in _views)
            {
                view.Disable();
            }
        }

        private void OnTargetHidden(ReactiveUIView view)
        {
            view.gameObject.SetActive(false);
        }

        private void RegisterTargets(UiEnableTarget[] targets, List<Action> animations, List<float> delays)
        {
            if (targets == null)
            {
                throw new InvalidOperationException($"{name}: HUD target list is required.");
            }

            for (int i = 0; i < targets.Length; i++)
            {
                ReactiveUIView view = targets[i].View;

                if (view == null)
                {
                    throw new InvalidOperationException($"{name}: HUD target {i} requires ReactiveUIView.");
                }

                animations.Add(view.Enable);
                delays.Add(targets[i].Delay);

                if (_views.Contains(view) == false)
                {
                    _views.Add(view);
                }
            }
        }

        private void CancelSequences()
        {
            _sceneScheduler.Cancel();
            _gameplayScheduler.Cancel();
        }
    }
}
