using System;
using MadSlime.Game.View;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Game.Logic
{
    public class LoadingScreenFactory
    {
        public LoadingScreenView Create(Canvas prefab, Transform parent)
        {
            ArgumentGuard.CheckNotNull(prefab, nameof(prefab));

            Canvas instance = UnityEngine.Object.Instantiate(prefab, parent);

            if (instance.TryGetComponent(out LoadingScreenView view))
            {
                return view;
            }

            UnityEngine.Object.Destroy(instance.gameObject);
            throw new InvalidOperationException("The loading prefab must contain LoadingScreenView.");
        }
    }
}
