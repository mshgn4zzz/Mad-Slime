using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.ShapeFill.FillStateMachine.Interfaces;
using MadSlime.ShapeFill.FillStateMachine.States;
using MadSlime.ShapeFill.Logic;
using MadSlime.StateMachine.Interfaces;
using MadSlime.StateMachine.Logic;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.ShapeFill.FillStateMachine.Logic
{
    public class FillStateMachine : BaseStateMachine<IFillState>, ISessionStateMachine, IDisposable
    {
        private FillingState _filling;
        private ResultState _result;
        private RescuingPrepareState _rescuingPrepare;
        private FillSessionData _session;
        private FillSessionSaver _saver;
        private ShapeFillOrchestrator _fill;
        private LevelThemeApplier _theme;
        private GridCameraFitter _camera;
        private FillPlayerPresenter _player;
        private IMusicPlayer _music;
        private Playlist _playlist;
        private MeshRenderer _floor;
        private bool _isPrepared;

        public FillStateMachine(
            FillingState filling,
            ResultState result,
            RescuingPrepareState rescuingPrepare,
            FillSessionData session,
            FillSessionSaver saver,
            ShapeFillOrchestrator fill,
            LevelThemeApplier theme,
            GridCameraFitter camera,
            FillPlayerPresenter player,
            IMusicPlayer music)
        {
            ArgumentGuard.CheckNotNull(filling, nameof(filling));
            ArgumentGuard.CheckNotNull(result, nameof(result));
            ArgumentGuard.CheckNotNull(rescuingPrepare, nameof(rescuingPrepare));
            ArgumentGuard.CheckNotNull(session, nameof(session));
            ArgumentGuard.CheckNotNull(saver, nameof(saver));
            ArgumentGuard.CheckNotNull(fill, nameof(fill));
            ArgumentGuard.CheckNotNull(theme, nameof(theme));
            ArgumentGuard.CheckNotNull(camera, nameof(camera));
            ArgumentGuard.CheckNotNull(player, nameof(player));
            ArgumentGuard.CheckNotNull(music, nameof(music));

            _filling = filling;
            _result = result;
            _rescuingPrepare = rescuingPrepare;
            _session = session;
            _saver = saver;
            _fill = fill;
            _theme = theme;
            _camera = camera;
            _player = player;
            _music = music;

            RegisterState(_filling);
            RegisterState(_result);
            RegisterState(_rescuingPrepare);
            Subscribe();
        }

        public event Action<int> Succeeded;
        public event Action<int, bool> Failed;
        public event Action<Exception> PreparationFailed;

        public void Start()
        {
            if (_isPrepared == false)
            {
                throw new InvalidOperationException("FillStateMachine must be prepared before starting.");
            }

            if (CurrentState != null)
            {
                return;
            }

            _player.Start();
            _music.Play(_playlist);
            SwitchState<FillingState>();
        }

        public void Configure(Playlist playlist, MeshRenderer floor)
        {
            ArgumentGuard.CheckNotNull(playlist, nameof(playlist));
            ArgumentGuard.CheckNotNull(floor, nameof(floor));

            _playlist = playlist;
            _floor = floor;
        }

        public async UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_playlist == null || _floor == null)
            {
                throw new InvalidOperationException("FillStateMachine must be configured before preparation.");
            }

            Stop();
            _theme.Apply(_floor, _fill);
            _fill.Prepare(_floor.bounds);

            FillResult result = _fill.CalculateResult();

            _fill.FitCamera(_camera);
            await _player.PrepareAsync(cancellationToken);
            _session.Prepare(result);
            await _saver.CommitFillAndSave(result, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            _isPrepared = true;
        }

        public override void Stop()
        {
            base.Stop();
            _player.Stop();
            _isPrepared = false;
        }

        public void Accelerate()
        {
            if (CurrentState == _filling)
            {
                _filling.Accelerate();
            }
        }

        public void RequestRescue()
        {
            if (CurrentState == _result)
            {
                _result.RequestRescue();
            }
        }

        public void Dispose()
        {
            Stop();
            Unsubscribe();
        }

        private void Subscribe()
        {
            _filling.Completed += OnFillCompleted;
            _result.RescueRequested += OnRescueRequested;
            _result.Succeeded += OnWin;
            _result.Failed += OnFailed;
            _rescuingPrepare.Prepared += OnRescuePrepared;
            _rescuingPrepare.PreparationFailed += OnPreparationFailed;
        }

        private void Unsubscribe()
        {
            _filling.Completed -= OnFillCompleted;
            _result.RescueRequested -= OnRescueRequested;
            _result.Succeeded -= OnWin;
            _result.Failed -= OnFailed;
            _rescuingPrepare.Prepared -= OnRescuePrepared;
            _rescuingPrepare.PreparationFailed -= OnPreparationFailed;
        }

        private void OnFillCompleted()
        {
            if (CurrentState == _filling)
            {
                SwitchState<ResultState>();
            }
        }

        private void OnRescueRequested()
        {
            if (CurrentState == _result)
            {
                SwitchState<RescuingPrepareState>();
            }
        }

        private void OnRescuePrepared()
        {
            if (CurrentState == _rescuingPrepare)
            {
                SwitchState<FillingState>();
            }
        }

        private void OnWin(int reward)
        {
            Succeeded?.Invoke(reward);
        }

        private void OnFailed(int reward, bool isRescueAvailable)
        {
            Failed?.Invoke(reward, isRescueAvailable);
        }

        private void OnPreparationFailed(Exception exception)
        {
            PreparationFailed?.Invoke(exception);
        }
    }
}
