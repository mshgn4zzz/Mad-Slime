using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Leaderboards.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Leaderboards
{
    using Localization = global::MadSlime.Localization.Logic.Localization;
    public class LeaderboardView
    {
        private readonly Localization _localization;

        private readonly ScrollRect _scroll;
        private readonly RectTransform _panel;
        private readonly LeaderboardRowViewFactory _rowFactory;
        private readonly TMP_Text _statusText;
        private readonly Button _authButton;
        private readonly Button _retryButton;
        private readonly List<LeaderboardRowView> _rows = new List<LeaderboardRowView>();
        private LeaderboardRowView _ownRow;
        private Vector2Int _screenSize;
        private float _maximumPanelHeight;
        private string _statusKey;

        public LeaderboardView(
            ScrollRect scroll,
            RectTransform panel,
            LeaderboardRowViewFactory factory,
            TMP_Text status,
            Button auth,
            Button retry,
            Localization localization)
        {
            _localization = localization;
            _scroll = scroll;
            _panel = panel;
            _rowFactory = factory;
            _statusText = status;
            _authButton = auth;
            _retryButton = retry;
        }

        public void ShowLoading()
        {
            ShowStatus("leaderboard_loading", false, false);
        }

        public void ShowAuthorizationRequired()
        {
            ClearRows();
            ShowStatus("leaderboard_login_hint", true, false);
        }

        public void ShowEntries()
        {
            ShowStatus(null, false, false);
        }

        public void ShowEmpty()
        {
            ShowStatus("leaderboard_empty", false, true);
        }

        public void ShowError()
        {
            ShowStatus("leaderboard_error", false, true);
        }

        public void RefreshLanguage()
        {
            if (_statusText == null)
            {
                return;
            }

            _statusText.gameObject.SetActive(_statusKey != null);

            if (_statusKey == null)
            {
                _statusText.text = string.Empty;
            }
            else
            {
                _statusText.text = _localization.Get(_statusKey);
            }
        }

        public void UpdateLayout(bool isReady)
        {
            if (_panel == null || _scroll == null)
            {
                return;
            }

            if (_screenSize.x != Screen.width || _screenSize.y != Screen.height)
            {
                RememberPanelHeight();
            }

            if (isReady == false || _rows.Count == 0)
            {
                return;
            }

            float contentHeight = _scroll.content.rect.height;

            if (contentHeight <= 0f)
            {
                return;
            }

            float height = Mathf.Min(
                _maximumPanelHeight,
                _panel.rect.height + contentHeight - _scroll.viewport.rect.height);

            if (Mathf.Abs(height - _panel.rect.height) > 1f)
            {
                _panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
        }

        public void RememberPanelHeight()
        {
            _screenSize = new Vector2Int(Screen.width, Screen.height);
            _maximumPanelHeight = _panel.rect.height;
        }

        public void Render(LeaderboardData snapshot, string playerId)
        {
            ClearRows();

            for (int i = 0; i < snapshot.Players.Length; i++)
            {
                LeaderboardEntryData entry = snapshot.Players[i];
                bool isOwn = entry.Id == playerId;

                AddRow(entry, isOwn);
            }

            if (_ownRow == null && snapshot.CurrentPlayer.HasValue)
            {
                AddRow(snapshot.CurrentPlayer.Value, true);
            }
        }

        public async UniTask ScrollToPlayerAsync(CancellationToken cancellationToken)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scroll.content);
            _scroll.StopMovement();
            _scroll.verticalNormalizedPosition = 1f;

            if (_ownRow == null)
            {
                return;
            }

            RectTransform content = _scroll.content;
            RectTransform viewport = _scroll.viewport;
            Vector3 viewportTop = content.InverseTransformPoint(
                viewport.TransformPoint(new Vector3(0f, viewport.rect.yMax, 0f)));
            Vector3 viewportBottom = content.InverseTransformPoint(
                viewport.TransformPoint(new Vector3(0f, viewport.rect.yMin, 0f)));
            float viewportHeight = viewportTop.y - viewportBottom.y;
            float scrollableHeight = content.rect.height - viewportHeight;

            if (scrollableHeight <= 0f)
            {
                return;
            }

            RectTransform row = _ownRow.RectTransform;
            Vector3 rowBottom = content.InverseTransformPoint(
                row.TransformPoint(new Vector3(row.rect.center.x, row.rect.yMin, 0f)));
            float targetOffset = content.rect.yMax - rowBottom.y - viewportHeight;
            float clampedOffset = Mathf.Clamp(targetOffset, 0f, scrollableHeight);

            _scroll.verticalNormalizedPosition = 1f - (clampedOffset / scrollableHeight);
        }

        public void ClearRows()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                _rows[i].gameObject.SetActive(false);
                Object.Destroy(_rows[i].gameObject);
            }

            _rows.Clear();
            _ownRow = null;
        }

        private void ShowStatus(string key, bool isAuth, bool isRetry)
        {
            _statusKey = key;
            _authButton.gameObject.SetActive(isAuth);
            _retryButton.gameObject.SetActive(isRetry);
            _retryButton.interactable = isRetry;
            _scroll.gameObject.SetActive(isAuth == false && _rows.Count > 0);
            RefreshLanguage();
        }

        private void AddRow(LeaderboardEntryData entry, bool isOwn)
        {
            LeaderboardRowView row = _rowFactory.Create(_scroll.content, entry, isOwn);

            _rows.Add(row);

            if (isOwn)
            {
                _ownRow = row;
            }
        }
    }
}
