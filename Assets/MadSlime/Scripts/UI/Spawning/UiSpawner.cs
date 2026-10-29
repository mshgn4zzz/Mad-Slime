using System;
using System.Collections.Generic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MadSlime.UI.Spawning
{
    public class UiSpawner
    {
        private readonly List<Canvas> _canvases = new List<Canvas>();
        private IObjectResolver _resolver;
        private Transform _parent;

        public UiSpawner(IObjectResolver resolver)
        {
            if (resolver == null)
            {
                throw new InvalidOperationException(
                    "UiSpawner: IObjectResolver was not injected. Check that the scene "
                    + "LifetimeScope registers UiSpawner.");
            }

            _resolver = resolver;
        }

        public void SetParent(Transform parent)
        {
            ArgumentGuard.CheckNotNull(parent, nameof(parent));

            _parent = parent;
            _parent.gameObject.SetActive(true);
        }

        public T Spawn<T>(T prefab, UiLayer layer) where T : Component, IShowable
        {
            ArgumentGuard.CheckNotNull(prefab, nameof(prefab));

            T instance;

            if (_parent == null)
            {
                instance = _resolver.Instantiate(prefab);
            }
            else
            {
                instance = _resolver.Instantiate(prefab, _parent, false);
            }

            ApplyLayer(instance, layer);

            return instance;
        }

        public void Release(Component window)
        {
            ArgumentGuard.CheckNotNull(window, nameof(window));

            window.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(window.gameObject);
        }

        public void Show(IShowable showable)
        {
            ArgumentGuard.CheckNotNull(showable, nameof(showable));

            showable.Show();
        }

        private void ApplyLayer(Component showable, UiLayer layer)
        {
            if (showable.TryGetComponent(out Canvas canvas) == false)
            {
                throw new InvalidOperationException(
                    $"{showable.name}: no Canvas on the shown object root. Add a root Canvas so "
                    + $"UiSpawner can apply the {layer} layer.");
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = TakeFreeSortingOrder((int)layer, canvas);
            _canvases.Add(canvas);
        }

        private bool IsReleased(Canvas canvas)
        {
            return canvas == null;
        }

        private int TakeFreeSortingOrder(int startOrder, Canvas ownCanvas)
        {
            _canvases.RemoveAll(IsReleased);

            HashSet<int> occupiedOrders = new HashSet<int>();

            if (_parent != null)
            {
                _parent.GetComponentsInChildren(false, _canvases);
            }

            for (int i = 0; i < _canvases.Count; i++)
            {
                Canvas canvas = _canvases[i];

                if (canvas != ownCanvas && canvas.isActiveAndEnabled)
                {
                    occupiedOrders.Add(canvas.sortingOrder);
                }
            }

            int order = startOrder;

            while (occupiedOrders.Contains(order))
            {
                order++;
            }

            return order;
        }
    }
}
