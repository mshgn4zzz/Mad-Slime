using System;
using System.Collections.Generic;
using MadSlime.StateMachine.Interfaces;
using MadSlime.Validation;

namespace MadSlime.StateMachine.Logic
{
    public class BaseStateMachine<TState> where TState : IState
    {
        private Dictionary<Type, TState> _states = new Dictionary<Type, TState>();
        private TState _currentSceneState;

        protected TState CurrentState => _currentSceneState;

        public virtual void Stop()
        {
            TState state = _currentSceneState;

            _currentSceneState = default;
            state?.Exit();
        }

        protected void RegisterState(TState state)
        {
            ArgumentGuard.CheckNotNull(state, nameof(state));

            _states.Add(state.GetType(), state);
        }

        protected void SwitchState<TTarget>() where TTarget : TState
        {
            if (_currentSceneState != null && typeof(TTarget) == _currentSceneState.GetType())
            {
                return;
            }

            if (_states.TryGetValue(typeof(TTarget), out TState state) == false)
            {
                throw new InvalidOperationException($"State {typeof(TTarget).Name} is not registered.");
            }

            SwitchTo(state);
        }

        protected void SwitchTo(TState state)
        {
            if (ReferenceEquals(_currentSceneState, state))
            {
                return;
            }

            TState previousState = _currentSceneState;

            _currentSceneState = default;
            previousState?.Exit();
            _currentSceneState = state;
            _currentSceneState.Enter();
        }
    }
}
