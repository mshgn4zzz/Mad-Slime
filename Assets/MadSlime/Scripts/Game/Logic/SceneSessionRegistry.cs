using Cysharp.Threading.Tasks;
using System;
using System.Threading;
namespace Game
{
    public class SceneSessionRegistry
    {
        private Func<CancellationToken, UniTask> _pendingPreparation;
        private Action _pendingSceneStart;
        private Action _pendingSceneFinish;
        private CancellationToken _pendingSessionToken;
        private Action _currentSessionStart;
        private Action _currentSessionFinish;

        public bool HasPendingSession => _pendingPreparation != null;

        public Func<CancellationToken, UniTask> PendingPreparation => _pendingPreparation;

        public CancellationToken PendingSessionToken => _pendingSessionToken;

        public Action PendingSceneStart => _pendingSceneStart;

        public void RegisterPending(Func<CancellationToken, UniTask> preparation, Action sessionStart,
            Action sessionFinish, CancellationToken sessionToken)
        {
            _pendingPreparation = preparation;
            _pendingSceneStart = sessionStart;
            _pendingSceneFinish = sessionFinish;
            _pendingSessionToken = sessionToken;
        }

        public void Unregister(Action sessionStart)
        {
            if (_currentSessionStart == sessionStart)
            {
                _currentSessionStart = null;
                _currentSessionFinish = null;
            }

            if (_pendingSceneStart == sessionStart)
            {
                ClearPending();
            }
        }

        public void ClearPending()
        {
            _pendingPreparation = null;
            _pendingSceneStart = null;
            _pendingSceneFinish = null;
        }

        public void FinishCurrentSession()
        {
            _currentSessionFinish?.Invoke();
        }

        public void SetCurrentSession(Action sessionStart, Action sessionFinish)
        {
            _currentSessionStart = sessionStart;
            _currentSessionFinish = sessionFinish;
        }

        public void PromotePendingToCurrent()
        {
            Action pendingStart = _pendingSceneStart;
            Action pendingFinish = _pendingSceneFinish;
            ClearPending();
            SetCurrentSession(pendingStart, pendingFinish);
        }
    }
}
