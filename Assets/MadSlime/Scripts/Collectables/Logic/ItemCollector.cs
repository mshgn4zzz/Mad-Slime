using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Items.View;
using MadSlime.Player.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Collectables.Logic
{
    public class ItemCollector : MonoBehaviour
    {
        private HashSet<Item> _itemsInAbsorption = new HashSet<Item>();

        private CollectAvailability _collectAvailability;
        private ItemDetector _detector;
        private Absorber _absorber;
        private bool _isRunning;
        private CancellationTokenSource _absorptionSource;

        public event Action<Item> ItemCollected;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_collectAvailability, name, nameof(_collectAvailability));

            ArgumentGuard.CheckAssigned(_detector, name, nameof(_detector));

            ArgumentGuard.CheckAssigned(_absorber, name, nameof(_absorber));
        }

        private void OnEnable()
        {
            _detector.Detected += OnItemDetected;
        }

        private void OnDisable()
        {
            _detector.Detected -= OnItemDetected;

            StopCollecting();
        }

        [Inject]
        public void Construct(CollectAvailability collectAvailability, ItemDetector detector, Absorber absorber)
        {
            _collectAvailability = collectAvailability;
            _detector = detector;
            _absorber = absorber;
        }

        public void StartCollecting()
        {
            if (_absorptionSource == null)
            {
                _absorptionSource = CancellationTokenSource.CreateLinkedTokenSource(
                    this.GetCancellationTokenOnDestroy());
            }

            _isRunning = true;
        }

        public void PauseCollecting()
        {
            _isRunning = false;
        }

        public void StopCollecting()
        {
            _isRunning = false;

            if (_absorptionSource != null)
            {
                _absorptionSource.Cancel();
                _absorptionSource.Dispose();
                _absorptionSource = null;
            }

            foreach (Item item in _itemsInAbsorption)
            {
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                }
            }

            _itemsInAbsorption.Clear();
        }

        private async UniTaskVoid AbsorbAsync(Item item, CancellationToken cancellationToken)
        {
            try
            {
                await _absorber.AbsorbAsync(item.transform, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _itemsInAbsorption.Remove(item);
                return;
            }

            _itemsInAbsorption.Remove(item);

            item.gameObject.SetActive(false);
        }

        private void OnItemDetected(Item item)
        {
            if (_isRunning == false)
            {
                return;
            }

            ArgumentGuard.CheckAssigned(item.Definition, item.name, nameof(item.Definition));

            if (_collectAvailability.IsCollectable(item.Definition.Tier) == false)
            {
                return;
            }

            if (_itemsInAbsorption.Contains(item))
            {
                return;
            }

            _itemsInAbsorption.Add(item);

            if (item.TryGetComponent(out ItemBody body) == false)
            {
                throw new InvalidOperationException($"{item.name}: ItemBody is missing.");
            }

            body.Capture();

            CancellationToken cancellationToken = _absorptionSource.Token;

            ItemCollected?.Invoke(item);

            if (cancellationToken.IsCancellationRequested == false)
            {
                AbsorbAsync(item, cancellationToken).Forget();
            }
        }
    }
}
