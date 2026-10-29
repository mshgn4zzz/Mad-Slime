using System.Collections.Generic;
using MadSlime.Collectables.Logic;
using MadSlime.Items.Data;
using MadSlime.Items.View;
using MadSlime.Player.Logic;
using MadSlime.Quota.Data;
using MadSlime.Upgrades.Data;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Quota.Logic
{
    public class QuotaItemHighlighter : MonoBehaviour
    {
        private UpgradesConfig _config;

        private QuotaItemDetector _detector;
        private QuotaBoard _board;
        private CollectAvailability _collectAvailability;
        private PlayerTier _playerTier;
        private Dictionary<ItemDefinition, QuotaTypeState> _quotaTypes =
            new Dictionary<ItemDefinition, QuotaTypeState>();

        private Dictionary<Item, QuotaTrackedItem> _items = new Dictionary<Item, QuotaTrackedItem>();
        private HashSet<Item> _pendingItems = new HashSet<Item>();
        private bool _isRecalculationRequired;

        public Color HighlightColor => _config.HighlightColor;

        public float OutlineWidth => _config.OutlineWidth;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));
        }

        private void OnEnable()
        {
            foreach (QuotaEntry entry in _board.Entries)
            {
                _quotaTypes.Add(entry.Definition, new QuotaTypeState(entry, IsHighlightable(entry)));
            }

            _detector.Detected += OnDetected;
            _detector.Exited += OnExited;
            _board.QuotaChanged += OnQuotaChanged;
            _playerTier.TierChanged += OnTierChanged;
        }

        private void OnDisable()
        {
            _detector.Detected -= OnDetected;
            _detector.Exited -= OnExited;
            _board.QuotaChanged -= OnQuotaChanged;
            _playerTier.TierChanged -= OnTierChanged;

            foreach (QuotaTrackedItem item in _items.Values)
            {
                if (item.Animator != null)
                {
                    item.Animator.SetHighlighted(false);
                }
            }

            _items.Clear();
            _pendingItems.Clear();
            _quotaTypes.Clear();
            _isRecalculationRequired = false;
        }

        private void LateUpdate()
        {
            if (_isRecalculationRequired == false && _pendingItems.Count == 0)
            {
                return;
            }

            EvaluateTypes();
            ApplyChanges();
            ClearChanges();
        }

        [Inject]
        public void Construct(
            QuotaItemDetector detector,
            QuotaBoard board,
            CollectAvailability collectAvailability,
            PlayerTier playerTier,
            UpgradesConfig config)
        {
            _config = config;
            _detector = detector;
            _board = board;
            _collectAvailability = collectAvailability;
            _playerTier = playerTier;
        }

        private void EvaluateTypes()
        {
            foreach (QuotaTypeState type in _quotaTypes.Values)
            {
                if (type.IsRecalculationRequired == false)
                {
                    continue;
                }

                bool isHighlighted = IsHighlightable(type.Entry);

                type.UpdateHighlight(isHighlighted);
            }
        }

        private void ApplyChanges()
        {
            foreach (KeyValuePair<Item, QuotaTrackedItem> pair in _items)
            {
                QuotaTrackedItem item = pair.Value;

                if (item.Type.IsHighlightChanged == false && _pendingItems.Contains(pair.Key) == false)
                {
                    continue;
                }

                if (item.Animator == null)
                {
                    continue;
                }

                bool isHighlighted = item.Type.IsHighlighted && item.Collider != null
                    && item.Collider.enabled && item.Animator.isActiveAndEnabled;

                item.Animator.SetHighlighted(isHighlighted);
            }
        }

        private void ClearChanges()
        {
            foreach (QuotaTypeState type in _quotaTypes.Values)
            {
                type.ClearChanges();
            }

            _pendingItems.Clear();
            _isRecalculationRequired = false;
        }

        private bool IsHighlightable(QuotaEntry entry)
        {
            return entry.GetRemaining() > 0 && _collectAvailability.IsCollectable(entry.Definition.Tier);
        }

        private void OnDetected(Item item)
        {
            if (item == null || _items.ContainsKey(item)
                || _quotaTypes.TryGetValue(item.Definition, out QuotaTypeState type) == false)
            {
                return;
            }

            if (item.TryGetComponent(out GhostAnimator animator) == false
                || item.TryGetComponent(out ItemBody body) == false)
            {
                return;
            }

            _items.Add(item, new QuotaTrackedItem(animator, body.Collider, type));
            _pendingItems.Add(item);
        }

        private void OnExited(Item item)
        {
            if (item == null || _items.TryGetValue(item, out QuotaTrackedItem tracked) == false)
            {
                return;
            }

            _items.Remove(item);
            _pendingItems.Remove(item);

            if (tracked.Animator != null)
            {
                tracked.Animator.SetHighlighted(false);
            }
        }

        private void OnQuotaChanged(int remaining, QuotaEntry entry)
        {
            if (remaining > 0 || _quotaTypes.TryGetValue(entry.Definition, out QuotaTypeState type) == false)
            {
                return;
            }

            type.RequireRecalculation();
            _isRecalculationRequired = true;
        }

        private void OnTierChanged(SizeTier previousTier, SizeTier currentTier)
        {
            foreach (QuotaTypeState type in _quotaTypes.Values)
            {
                type.RequireRecalculation();
            }

            _isRecalculationRequired = true;
        }
    }
}
