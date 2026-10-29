using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Saves.Adapters;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using YG;

namespace MadSlime.Saves.Logic
{
    public class SavesCore : ISavesReadiness, ISaveConfirmation
    {
        private Yg2SavesAccess _access;
        private bool _isTransactionRunning;

        public SavesCore(Yg2SavesAccess access)
        {
            ArgumentGuard.CheckNotNull(access, nameof(access));

            _access = access;
        }

        public event Action Prepared
        {
            add
            {
                _access.Prepared += value;
            }
            remove
            {
                _access.Prepared -= value;
            }
        }

        public bool IsReady => _access.IsReady;

        public UniTask ConfirmSavedAsync(CancellationToken cancellationToken)
        {
            return _access.ConfirmSavedAsync(cancellationToken);
        }

        public void ExecuteTransaction(Action operation)
        {
            ArgumentGuard.CheckNotNull(operation, nameof(operation));

            if (_isTransactionRunning)
            {
                throw new InvalidOperationException("SavesCore: nested transactions are not supported.");
            }

            SavesYG originalData = LoadData();
            string snapshot = JsonUtility.ToJson(originalData);

            _isTransactionRunning = true;

            try
            {
                operation.Invoke();
                _access.Save();
            }
            catch
            {
                JsonUtility.FromJsonOverwrite(snapshot, originalData);
                throw;
            }
            finally
            {
                _isTransactionRunning = false;
            }
        }

        public SavesYG LoadData()
        {
            if (_access.IsReady == false)
            {
                throw new InvalidOperationException(
                    "SavesCore: the save data is not ready yet. Wait for the Ready event before "
                    + "reading or writing saves.");
            }

            return _access.Data;
        }

        public void SaveChanges()
        {
            if (_isTransactionRunning == false)
            {
                _access.Save();
            }
        }
    }
}
