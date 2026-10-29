using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.GameFSM.Interfaces;
using MadSlime.StateMachine.Interfaces;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.GameFSM
{
    public abstract class SceneState<TMachine> : MonoBehaviour, ISceneState
        where TMachine : ISessionStateMachine
    {
        private SceneStateMachine _sceneMachine;
        private bool _isEntered;

        public event Action SceneSessionPrepared;
        public event Action SceneSessionStarted;
        public event Action Finished;

        public bool IsFinished { get; private set; }
        protected TMachine Machine { get; private set; }

        protected virtual void Awake()
        {
            ArgumentGuard.CheckAssigned(Machine, name, nameof(Machine));
            ArgumentGuard.CheckAssigned(_sceneMachine, name, nameof(_sceneMachine));

            ValidateConfiguration();
        }

        protected virtual void Start()
        {
            _sceneMachine.RegisterScene(this, gameObject.scene, this.GetCancellationTokenOnDestroy());
        }

        public async UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsFinished = false;
            ConfigureMachine();
            await Machine.PrepareAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            SceneSessionPrepared?.Invoke();
        }

        public virtual void Enter()
        {
            _isEntered = true;
            Subscribe();
            Machine.Start();
            SceneSessionStarted?.Invoke();
        }

        public virtual void Exit()
        {
            if (IsFinished)
            {
                return;
            }

            IsFinished = true;

            bool isPreviouslyEntered = _isEntered;

            _isEntered = false;

            try
            {
                Machine.Stop();
            }
            finally
            {
                if (isPreviouslyEntered)
                {
                    Unsubscribe();
                }
            }

            Finished?.Invoke();
        }

        protected void Initialize(TMachine machine, SceneStateMachine sceneMachine)
        {
            ArgumentGuard.CheckNotNull(machine, nameof(machine));
            ArgumentGuard.CheckNotNull(sceneMachine, nameof(sceneMachine));
            Machine = machine;
            _sceneMachine = sceneMachine;
        }

        protected bool IsTransitionAllowed()
        {
            return _isEntered && IsFinished == false &&
                   _sceneMachine.IsStopped == false && _sceneMachine.IsTransitioning == false;
        }

        protected void RequestTransition<TTarget>() where TTarget : ISceneState
        {
            if (IsTransitionAllowed())
            {
                _sceneMachine.SwitchStateAsync<TTarget>()
                    .Forget(_sceneMachine.HandleTransitionException);
            }
        }

        protected abstract void ValidateConfiguration();

        protected abstract void ConfigureMachine();

        protected virtual void Subscribe()
        {
        }

        protected virtual void Unsubscribe()
        {
        }
    }
}
