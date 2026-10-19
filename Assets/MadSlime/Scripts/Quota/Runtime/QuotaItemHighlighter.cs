using Collectables;
using Items;
using Player;
using Quota;
using Skills;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Game
{
    public class QuotaItemHighlighter : MonoBehaviour
    {
        [SerializeField] private Color _highlightColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField, Min(0f)] private float _outlineWidth = 4f;

        private QuotaItemDetector _detector;
        private QuotaBoard _board;
        private CollectAvailability _collectAvailability;
        private PlayerTier _playerTier;
        private HashSet<Item> _items = new HashSet<Item>();

        [Inject]
        public void Construct(QuotaItemDetector detector, QuotaBoard board, CollectAvailability collectAvailability,
            PlayerTier playerTier)
        {
            _detector = detector;
            _board = board;
            _collectAvailability = collectAvailability;
            _playerTier = playerTier;
        }

        private void OnEnable()
        {
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

            foreach (Item item in _items)
            {
                SetHighlighted(item, false);
            }

            _items.Clear();
        }

        private void OnDetected(Item item)
        {
            _items.Add(item);
            SetHighlighted(item, IsHighlightable(item));
        }

        private void OnExited(Item item)
        {
            _items.Remove(item);
            SetHighlighted(item, false);
        }

        private void OnQuotaChanged(int remaining, QuotaEntry entry)
        {
            RefreshAll();
        }

        private void OnTierChanged(SizeTier previousTier, SizeTier currentTier)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            foreach (Item item in _items)
            {
                if (item != null)
                {
                    SetHighlighted(item, IsHighlightable(item));
                }
            }
        }

        private bool IsHighlightable(Item item)
        {
            return _board.IsQuotaItem(item.Definition) && _collectAvailability.CanCollect(item.Definition.Tier);
        }

        private void SetHighlighted(Item item, bool isHighlighted)
        {
            if (item != null && item.TryGetComponent(out GhostAnimator animator))
            {
                animator.SetHighlighted(isHighlighted, _highlightColor, _outlineWidth);
            }
        }
    }
}
