using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.UI.Common
{
    public class WindowParticles : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] _systems;

        private void Awake()
        {
            ArgumentGuard.CheckNotEmpty(_systems, name, nameof(_systems));

            foreach (ParticleSystem system in _systems)
            {
                ArgumentGuard.CheckAssigned(system, name, nameof(system));
            }
        }

        private void OnEnable()
        {
            foreach (ParticleSystem system in _systems)
            {
                ParticleSystem.MainModule main = system.main;

                main.useUnscaledTime = true;
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Play(true);
            }
        }

        private void OnDisable()
        {
            foreach (ParticleSystem system in _systems)
            {
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
