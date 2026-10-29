using System;
using UnityEngine;
using VContainer;
using CountdownMachine = MadSlime.Game.Logic.TimerStateMachine.TimerStateMachine;

namespace MadSlime.Game.Logic
{
    public class Timer : MonoBehaviour
    {
        private CountdownMachine _machine;

        public event Action Finished;
        public event Action<float> Ticked;

        private void OnEnable()
        {
            _machine.Ticked += OnTicked;
            _machine.Finished += OnFinished;
        }

        private void OnDisable()
        {
            _machine.StopCount();
            _machine.Ticked -= OnTicked;
            _machine.Finished -= OnFinished;
        }

        private void Update()
        {
            _machine.Tick(Time.deltaTime);
        }

        [Inject]
        public void Construct(CountdownMachine machine)
        {
            _machine = machine;
        }

        public void Setup(float duration)
        {
            _machine.Configure(duration);
        }

        public void StartCount()
        {
            _machine.StartCount();
        }

        public void Stop()
        {
            _machine.StopCount();
        }

        private void OnTicked(float remaining)
        {
            Ticked?.Invoke(remaining);
        }

        private void OnFinished()
        {
            Finished?.Invoke();
        }
    }
}
