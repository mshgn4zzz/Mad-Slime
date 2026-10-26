using Cysharp.Threading.Tasks;
using Economy;
using Game;
using Leaderboards;
using Saves;
using System;
using System.Threading;
using UnityEngine;
namespace ShapeFill
{
    public class FillSessionSaver
    {
        private ILevelStorage _levelStorage;
        private Rewarder _rewarder;
        private LeaderboardReporter _leaderboardReporter;
        private SavesTransaction _transaction;
        private ISaveConfirmation _saveConfirmation;
        private bool _isLevelProgressCommitted;

        public FillSessionSaver(ILevelStorage levelStorage, Rewarder rewarder,
            LeaderboardReporter leaderboardReporter, SavesTransaction transaction,
            ISaveConfirmation saveConfirmation)
        {
            ArgumentGuard.CheckNotNull(levelStorage, nameof(levelStorage));

            ArgumentGuard.CheckNotNull(rewarder, nameof(rewarder));

            ArgumentGuard.CheckNotNull(leaderboardReporter, nameof(leaderboardReporter));

            ArgumentGuard.CheckNotNull(transaction, nameof(transaction));

            ArgumentGuard.CheckNotNull(saveConfirmation, nameof(saveConfirmation));

            _levelStorage = levelStorage;
            _rewarder = rewarder;
            _leaderboardReporter = leaderboardReporter;
            _transaction = transaction;
            _saveConfirmation = saveConfirmation;
        }

        public UniTask CommitFillAndSave(FillResult fillResult, CancellationToken cancellationToken)
        {
            _rewarder.ResetLevelResult();
            _transaction(() => CommitFill(fillResult));

            if (_rewarder.IsWin == true)
            {
                _leaderboardReporter.Report();
            }

            return _saveConfirmation.ConfirmSavedAsync(cancellationToken);
        }

        public UniTask CommitRescueAndSave(CancellationToken cancellationToken)
        {
            _transaction(CommitRescue);
            _leaderboardReporter.Report();
            return _saveConfirmation.ConfirmSavedAsync(cancellationToken);
        }

        private void CommitFill(FillResult fillResult)
        {
            _rewarder.CommitLevelResult(fillResult.Percent);

            if (_rewarder.IsWin == true)
            {
                CommitLevelCompletion();
            }
        }

        private void CommitRescue()
        {
            _rewarder.CommitRescueTopUp();
            CommitLevelCompletion();
        }

        private void CommitLevelCompletion()
        {
            if (_isLevelProgressCommitted == true)
            {
                return;
            }

            _isLevelProgressCommitted = true;
            int nextLevel = _levelStorage.CurrentLevel + 1;
            int maxLevel = Mathf.Max(_levelStorage.MaxLevel, nextLevel);
            _levelStorage.SetLevelProgress(nextLevel, maxLevel);
        }
    }
}
