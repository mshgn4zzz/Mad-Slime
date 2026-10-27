using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Economy;
using Game;
using Saves;
using UnityEngine;

namespace ShapeFill
{
    public class FillSessionSaver
    {
        private ILevelStorage _levelStorage;
        private Rewarder _rewarder;
        private SavesTransaction _transaction;
        private ISaveConfirmation _saveConfirmation;
        private bool _isLevelProgressCommitted;

        public FillSessionSaver(
            ILevelStorage levelStorage,
            Rewarder rewarder,
            SavesTransaction transaction,
            ISaveConfirmation saveConfirmation)
        {
            ArgumentGuard.CheckNotNull(levelStorage, nameof(levelStorage));

            ArgumentGuard.CheckNotNull(rewarder, nameof(rewarder));

            ArgumentGuard.CheckNotNull(transaction, nameof(transaction));

            ArgumentGuard.CheckNotNull(saveConfirmation, nameof(saveConfirmation));

            _levelStorage = levelStorage;
            _rewarder = rewarder;
            _transaction = transaction;
            _saveConfirmation = saveConfirmation;
        }

        public UniTask CommitFillAndSave(FillResult fillResult, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentGuard.CheckNotNull(fillResult, nameof(fillResult));

            _isLevelProgressCommitted = false;
            _rewarder.ResetLevelResult();
            _transaction(() => CommitFill(fillResult));

            return _saveConfirmation.ConfirmSavedAsync(cancellationToken);
        }

        public UniTask CommitRescueAndSave(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _transaction(CommitRescue);
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
