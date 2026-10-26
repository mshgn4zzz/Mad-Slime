using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using YG;
namespace Saves
{
    public class SavesCore : ISavesReadiness, ISaveConfirmation
    {
        private Yg2SavesAccess _access;
        private bool _isTransactionRunning;

        public event Action Ready
        {
            add
            {
                _access.Ready += value;
            }
            remove
            {
                _access.Ready -= value;
            }
        }

        public bool IsReady => _access.IsReady;

        public SavesCore(Yg2SavesAccess access)
        {
            if (access == null)
            {
                throw new ArgumentNullException(nameof(access),
                    "SavesCore requires a Yg2SavesAccess instance.");
            }

            _access = access;
        }

        public UniTask ConfirmSavedAsync(CancellationToken cancellationToken)
        {
            return _access.ConfirmSavedAsync(cancellationToken);
        }

        public void ExecuteTransaction(Action operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (_isTransactionRunning == true)
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
