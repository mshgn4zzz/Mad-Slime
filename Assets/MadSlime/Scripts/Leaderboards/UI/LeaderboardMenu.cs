using Core;
using Game;
using Saves;
using Scriptables;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI
{
    public class LeaderboardMenu : BaseWindow
    {
        private const int TopCount = 10;
        private const int AroundCount = 1;

        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _authButton;
        [SerializeField] private TMP_Text _entriesText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Image[] _rowImages;
        [SerializeField] private TMP_Text[] _rankTexts;
        [SerializeField] private TMP_Text[] _nameTexts;
        [SerializeField] private TMP_Text[] _scoreTexts;
        [SerializeField] private Color _rowColor = new Color(0f, 0f, 0f, 0.12f);
        [SerializeField] private Color _ownRowColor = new Color(0.12f, 0.5f, 0.8f, 0.6f);
        [SerializeField] private YandexConfig _config;

        private ISavesReadiness _savesReadiness;
        private ILeaderboardService _leaderboardService;

        [Inject]
        public void Construct(ISavesReadiness savesReadiness, ILeaderboardService leaderboardService)
        {
            _savesReadiness = savesReadiness;
            _leaderboardService = leaderboardService;
        }

        public void Initialize()
        {
            if (_closeButton == null)
            {
                throw new InvalidOperationException(
                    $"{name}: CloseButton is not assigned. Drag a Button into the _closeButton field.");
            }

            if (_authButton == null)
            {
                throw new InvalidOperationException(
                    $"{name}: AuthButton is not assigned. Drag a Button into the _authButton field.");
            }

            if (_entriesText == null)
            {
                throw new InvalidOperationException(
                    $"{name}: EntriesText is not assigned. Drag a TMP_Text into the _entriesText field.");
            }

            if (_retryButton == null || _rowImages == null || _rowImages.Length == 0
                || _rankTexts == null || _nameTexts == null || _scoreTexts == null
                || _rankTexts.Length != _rowImages.Length || _nameTexts.Length != _rowImages.Length
                || _scoreTexts.Length != _rowImages.Length)
            {
                throw new InvalidOperationException($"{name}: assign the retry button and matching leaderboard row arrays.");
            }

            for (int i = 0; i < _rowImages.Length; i++)
            {
                if (_rowImages[i] == null || _rankTexts[i] == null || _nameTexts[i] == null || _scoreTexts[i] == null)
                {
                    throw new InvalidOperationException($"{name}: leaderboard row {i} is incomplete.");
                }
            }

            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: YandexConfig is not assigned. Drag the YandexConfig asset into the _config field.");
            }

            if (_savesReadiness == null || _leaderboardService == null)
            {
                throw new InvalidOperationException(
                    $"{name}: dependencies were not injected. The window prefab must be shown through the DI container (UiSpawner.Spawn).");
            }

            _closeButton.onClick.AddListener(Close);
            _authButton.onClick.AddListener(OnAuthClicked);
            _retryButton.onClick.AddListener(OnRetryClicked);
            _leaderboardService.EntriesReceived += OnLeaderboardReceived;
            _leaderboardService.EntriesFailed += OnLeaderboardFailed;
            _savesReadiness.Ready += OnSdkDataReceived;

            RefreshAuthView();
            RequestLeaderboard();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _closeButton?.onClick.RemoveListener(Close);
            _authButton?.onClick.RemoveListener(OnAuthClicked);
            _retryButton?.onClick.RemoveListener(OnRetryClicked);
            if (_leaderboardService == null)
            {
                return;
            }

            _leaderboardService.EntriesReceived -= OnLeaderboardReceived;
            _leaderboardService.EntriesFailed -= OnLeaderboardFailed;
            _savesReadiness.Ready -= OnSdkDataReceived;
        }

        private void RequestLeaderboard()
        {
            ClearRows();
            _entriesText.enabled = true;
            _retryButton.gameObject.SetActive(false);
            _entriesText.text = Localization.Get("leaderboard_loading");

            if (_savesReadiness.IsReady == false)
            {
                return;
            }

            _leaderboardService.RequestEntries(_config.LeaderboardName, TopCount, AroundCount);
        }

        private void OnLeaderboardFailed()
        {
            ClearRows();
            _entriesText.enabled = true;
            _entriesText.text = Localization.Get("leaderboard_error");
            _retryButton.gameObject.SetActive(true);
        }

        private void OnLeaderboardReceived(LeaderboardData snapshot)
        {
            if (snapshot.TechnoName != _config.LeaderboardName)
            {
                return;
            }

            if (snapshot.Players.Length == 0)
            {
                ClearRows();
                _entriesText.enabled = true;
                _entriesText.text = Localization.Get("leaderboard_empty");
                _retryButton.gameObject.SetActive(true);
                return;
            }

            ClearRows();
            _entriesText.enabled = false;
            _retryButton.gameObject.SetActive(false);
            int rowIndex = 0;
            bool ownRowInTop = false;

            for (int entryIndex = 0; entryIndex < snapshot.Players.Length && rowIndex < _rowImages.Length; entryIndex++)
            {
                LeaderboardEntryData entry = snapshot.Players[entryIndex];
                bool isOwn = _leaderboardService.IsAuthorized == true && entry.Id == _leaderboardService.PlayerId;
                RenderRow(rowIndex, entry, isOwn);
                ownRowInTop = ownRowInTop || isOwn;
                rowIndex++;
            }

            if (ownRowInTop == false && _leaderboardService.IsAuthorized == true
                && snapshot.CurrentPlayer.HasValue == true && rowIndex < _rowImages.Length)
            {
                LeaderboardEntryData entry = snapshot.CurrentPlayer.Value;
                if (entry.Rank <= 0)
                {
                    return;
                }

                entry.Name = _leaderboardService.PlayerName;
                RenderRow(rowIndex, entry, true);
            }
        }

        private void ClearRows()
        {
            for (int i = 0; i < _rowImages.Length; i++)
            {
                _rowImages[i].gameObject.SetActive(false);
            }
        }

        private void RenderRow(int index, LeaderboardEntryData entry, bool isOwn)
        {
            _rankTexts[index].text = entry.Rank.ToString();
            _nameTexts[index].richText = false;
            _nameTexts[index].overflowMode = TextOverflowModes.Ellipsis;
            _nameTexts[index].text = entry.Name;
            _scoreTexts[index].text = entry.Score.ToString();
            _rowImages[index].color = _rowColor;

            if (isOwn == true)
            {
                _rowImages[index].color = _ownRowColor;
            }

            _rowImages[index].gameObject.SetActive(true);
        }

        private void OnRetryClicked()
        {
            RequestLeaderboard();
        }

        private void OnSdkDataReceived()
        {
            RefreshAuthView();

            RequestLeaderboard();
        }

        private void OnAuthClicked()
        {
            _leaderboardService.OpenAuthDialog();
            RefreshAuthView();
        }

        private void RefreshAuthView()
        {
            _authButton.gameObject.SetActive(_leaderboardService.IsAuthorized == false);
        }

        private void Close()
        {
            RequestClose();
        }
    }
}
