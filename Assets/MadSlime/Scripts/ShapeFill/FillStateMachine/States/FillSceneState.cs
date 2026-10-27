using System;
using Audio;
using Game;
using UnityEngine;
using VContainer;

namespace ShapeFill
{
    public class FillSceneState : SceneState<FillStateMachine>
    {
        [SerializeField] private Playlist _musicPlaylist;
        [SerializeField] private MeshRenderer _floorRenderer;
        [SerializeField, Min(0f)] private float _winWindowDelay = 1f;

        private FillExitFlow _exitFlow;
        private Pauser _pauser;
        private int _pauseRequests;

        public event Action<int> Win;
        public event Action<int, bool> Failed;
        public event Action<Exception> PreparationFailed;

        public float ResultWindowDelay => _winWindowDelay;

        [Inject]
        public void Construct(
            FillStateMachine machine,
            SceneNavigator navigator,
            SceneStateMachine sceneMachine,
            FillExitFlow exitFlow,
            Pauser pauser)
        {
            ArgumentGuard.CheckNotNull(exitFlow, nameof(exitFlow));
            ArgumentGuard.CheckNotNull(pauser, nameof(pauser));

            Initialize(machine, navigator, sceneMachine);
            _exitFlow = exitFlow;
            _pauser = pauser;
        }

        public void ReloadLevel()
        {
            if (IsFinished == false)
            {
                _exitFlow.Exit(SceneId.Game);
            }
        }

        public void ExitToMenu()
        {
            if (IsFinished == false)
            {
                _exitFlow.Exit(SceneId.Menu);
            }
        }

        public void RescueFill()
        {
            Machine.RequestRescue();
        }

        public void Accelerate()
        {
            Machine.Accelerate();
        }

        public void PauseByRequest()
        {
            if (IsFinished)
            {
                return;
            }

            _pauseRequests++;
            _pauser.RequestPause();
        }

        public void ResumeByRequest()
        {
            if (_pauseRequests == 0)
            {
                return;
            }

            _pauseRequests--;
            _pauser.RequestResume();
        }

        protected override void ValidateConfiguration()
        {
            if (_musicPlaylist == null || _floorRenderer == null)
            {
                throw new InvalidOperationException($"{name}: assign the fill playlist and floor renderer.");
            }

            if (float.IsNaN(_winWindowDelay) || float.IsInfinity(_winWindowDelay) || _winWindowDelay < 0f)
            {
                throw new InvalidOperationException($"{name}: result window delay must be finite and nonnegative.");
            }
        }

        protected override void ConfigureMachine()
        {
            Machine.Configure(_musicPlaylist, _floorRenderer);
        }

        protected override void SubscribeMachineEvents()
        {
            Machine.Win += OnWin;
            Machine.Failed += OnFailed;
            Machine.PreparationFailed += OnPreparationFailed;
        }

        protected override void UnsubscribeMachineEvents()
        {
            Machine.Win -= OnWin;
            Machine.Failed -= OnFailed;
            Machine.PreparationFailed -= OnPreparationFailed;
        }

        protected override void OnStopped()
        {
            while (_pauseRequests > 0)
            {
                ResumeByRequest();
            }
        }

        private void OnWin(int reward)
        {
            Win?.Invoke(reward);
        }

        private void OnFailed(int reward, bool canRescue)
        {
            Failed?.Invoke(reward, canRescue);
        }

        private void OnPreparationFailed(Exception exception)
        {
            PreparationFailed?.Invoke(exception);
        }
    }
}
