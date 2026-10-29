using System;
using MadSlime.Leaderboards.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Leaderboards
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    public class LeaderboardRowView : MonoBehaviour
    {
        private const string AnonymousNameMarker = "anonymous";
        [VContainer.Inject] private Localization _localization;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private Color _rowColor = new Color(0f, 0f, 0f, 0.12f);
        [SerializeField] private Color _ownRowColor = new Color(0.12f, 0.5f, 0.8f, 0.6f);

        private LeaderboardEntryData _entry;

        public RectTransform RectTransform => _rectTransform;

        private void OnEnable()
        {
            _localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            _localization.LanguageChanged -= OnLanguageChanged;
        }

        public void Initialize(LeaderboardEntryData entry, bool isOwn)
        {
            if (_rectTransform == null || _background == null || _rankText == null
                || _nameText == null || _scoreText == null)
            {
                throw new InvalidOperationException($"{name}: leaderboard row references are incomplete.");
            }

            _entry = entry;
            _rankText.text = entry.Rank.ToString();
            _nameText.richText = false;
            _nameText.overflowMode = TextOverflowModes.Ellipsis;
            OnLanguageChanged();
            _scoreText.text = entry.Score.ToString();
            _background.color = _rowColor;

            if (isOwn)
            {
                _background.color = _ownRowColor;
            }
        }

        private void OnLanguageChanged()
        {
            if (string.IsNullOrEmpty(_entry.Name) || _entry.Name == AnonymousNameMarker)
            {
                string anonymousName = _localization.Get("leaderboard_anonymous");

                _nameText.text = $"{anonymousName} #{_entry.Id}";
                return;
            }

            _nameText.text = _entry.Name;
        }
    }
}
