using System;
using System.Threading;
using Audio;
using Cysharp.Threading.Tasks;
using Game;
using UnityEngine;

namespace ShapeFill
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
            ArgumentGuard.CheckNotNull(music, nameof(music));

            _filling = filling;
            _result = result;
            _rescuingPrepare = rescuingPrepare;
            _session = session;
            _saver = saver;
            _fill = fill;
            _theme = theme;
            _camera = camera;
            _music = music;

            RegisterState(_filling);
            RegisterState(_result);
            RegisterState(_rescuingPrepare);
            _filling.Completed += OnFillCompleted;
            _result.RescueRequested += OnRescueRequested;
            _result.Win += OnWin;
            _result.Failed += OnFailed;
            _rescuingPrepare.Prepared += OnRescuePrepared;
            _rescuingPrepare.PreparationFailed += OnPreparationFailed;
        }

        public event Action<int> Win;
        public event Action<int, bool> Failed;
        public event Action<Exception> PreparationFailed;

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
            _session.Prepare(result);
            await _saver.CommitFillAndSave(result, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            _isPrepared = true;
        }

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

            _music.Play(_playlist);
            SwitchState<FillingState>();
        }

        public override void Stop()
        {
            base.Stop();
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
            _filling.Completed -= OnFillCompleted;
            _result.RescueRequested -= OnRescueRequested;
            _result.Win -= OnWin;
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
            Win?.Invoke(reward);
        }

        private void OnFailed(int reward, bool canRescue)
        {
            Failed?.Invoke(reward, canRescue);
        }

        private void OnPreparationFailed(Exception exception)
        {
            PreparationFailed?.Invoke(exception);
        }
    }
}
