using MadSlime.Quota.Data;

namespace MadSlime.Quota.Logic
{
    internal class QuotaTypeState
    {
        private QuotaEntry _entry;
        private bool _isHighlighted;
        private bool _isRecalculationRequired;
        private bool _isHighlightChanged;

        public QuotaTypeState(QuotaEntry entry, bool isHighlighted)
        {
            _entry = entry;
            _isHighlighted = isHighlighted;
        }

        public QuotaEntry Entry => _entry;

        public bool IsHighlighted => _isHighlighted;

        public bool IsRecalculationRequired => _isRecalculationRequired;

        public bool IsHighlightChanged => _isHighlightChanged;

        public void RequireRecalculation()
        {
            _isRecalculationRequired = true;
        }

        public void UpdateHighlight(bool isHighlighted)
        {
            _isHighlightChanged = _isHighlighted != isHighlighted;
            _isHighlighted = isHighlighted;
            _isRecalculationRequired = false;
        }

        public void ClearChanges()
        {
            _isHighlightChanged = false;
        }
    }
}
