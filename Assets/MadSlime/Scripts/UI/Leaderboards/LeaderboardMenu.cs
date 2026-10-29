using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Game.Data;
using MadSlime.UI.Leaderboards.StateMachine;
using MadSlime.UI.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.UI.Leaderboards
{
    using Localization = global::MadSlime.Localization.Logic.Localization;
    public class LeaderboardMenu : BaseWindow
    {
        [VContainer.Inject] private Localization _localization;

        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private Button _authButton;
        [SerializeField]
        private TMP_Text _statusText;
        [SerializeField]
        private Button _retryButton;
        [SerializeField]
        private LeaderboardRowView _rowPrefab;

        [SerializeField]
        private ScrollRect _scroll;
        [SerializeField]
        private RectTransform _panel;
        [SerializeField]
        private YandexConfig _config;

        private LeaderboardRowViewFactory _rowFactory;
        private LeaderboardStateMachineFactory _factory;
        private global::MadSlime.Saves.Interfaces.ISavesReadiness _readiness;
        private LeaderboardStateMachine _machine;
        private LeaderboardView _view;
        private CancellationTokenSource _layoutSource;

        protected override void Awake()
        {
            base.Awake();

            if (_factory == null || _config == null || string.IsNullOrEmpty(_config.LeaderboardName)
                || _closeButton == null
                || _authButton == null
                || _retryButton == null
                || _rowFactory == null
                || _scroll == null
                || _panel == null)
            {
                throw new InvalidOperationException($"{name}: leaderboard references are incomplete.");
            }

            _machine = _factory.Create(_config.LeaderboardName);
            _view = new LeaderboardView(
                _scroll,
                _panel,
                _rowFactory,
                _statusText,
                _authButton,
                _retryButton,
                _localization);
        }

        protected override void OnDisable()
        {
            StopLoading();
            _closeButton.onClick.RemoveListener(OnCloseClicked);
            _authButton.onClick.RemoveListener(OnAuthClicked);
            _retryButton.onClick.RemoveListener(OnRefresh);
            _localization.LanguageChanged -= OnLanguageChanged;
            _readiness.Prepared -= OnRefresh;
            _machine.LoadingStarted -= OnLoading;
            _machine.AuthorizationRequired -= OnAuthRequired;
            _machine.SnapshotPrepared -= OnSnapshot;
            _machine.Prepared -= OnReady;
            _machine.EmptyResultReceived -= OnEmpty;
            _machine.Failed -= OnFailed;
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            _machine?.Dispose();
            base.OnDestroy();
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseClicked);
            _authButton.onClick.AddListener(OnAuthClicked);
            _retryButton.onClick.AddListener(OnRefresh);
            _localization.LanguageChanged += OnLanguageChanged;
            _readiness.Prepared += OnRefresh;
            _machine.LoadingStarted += OnLoading;
            _machine.AuthorizationRequired += OnAuthRequired;
            _machine.SnapshotPrepared += OnSnapshot;
            _machine.Prepared += OnReady;
            _machine.EmptyResultReceived += OnEmpty;
            _machine.Failed += OnFailed;
        }

        private void LateUpdate()
        {
            _view.UpdateLayout(_machine.IsReady());
        }

        [Inject]
        public void Construct(
            LeaderboardStateMachineFactory factory,
            global::MadSlime.Saves.Interfaces.ISavesReadiness readiness,
            IObjectResolver resolver)
        {
            _rowFactory = new LeaderboardRowViewFactory(resolver, _rowPrefab);
            _factory = factory;
            _readiness = readiness;
        }

        public void Initialize()
        {
            _view.RememberPanelHeight();
            _machine.Start();
        }

        private async UniTask ScrollAsync(CancellationToken token)
        {
            try
            {
                await _view.ScrollToPlayerAsync(token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
        }

        private void CancelLayout()
        {
            CancellationTokenSource source = _layoutSource;

            _layoutSource = null;
            source?.Cancel();
            source?.Dispose();
        }

        private void StopLoading()
        {
            CancelLayout();
            _machine.Stop();
        }

        protected override void OnClosing()
        {
            StopLoading();
            base.OnClosing();
        }

        private void OnLoading()
        {
            CancelLayout();
            _view.ShowLoading();
        }

        private void OnAuthRequired()
        {
            _view.ShowAuthorizationRequired();
        }

        private void OnReady()
        {
            _view.ShowEntries();
        }

        private void OnEmpty()
        {
            _view.ShowEmpty();
        }

        private void OnFailed()
        {
            _view.ShowError();
        }

        private void OnSnapshot(global::MadSlime.Leaderboards.Data.LeaderboardData snapshot)
        {
            CancelLayout();
            _view.Render(snapshot, _machine.PlayerId);
            _layoutSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            ScrollAsync(_layoutSource.Token).Forget();
        }

        private void OnCloseClicked()
        {
            RequestClose();
        }

        private void OnAuthClicked()
        {
            _machine.RequestAuthorization();
        }

        private void OnRefresh()
        {
            if (IsClosing() == false)
            {
                _machine.Start();
            }
        }

        private void OnLanguageChanged()
        {
            _view.RefreshLanguage();
        }
    }
}
