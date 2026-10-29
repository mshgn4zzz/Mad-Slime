using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Validation;
using YG;

namespace MadSlime.Saves.Adapters
{
    public class Yg2SavesAccess
    {
        private SaveConfirmation _confirmation;
        private UniTask _latestSave;
        private bool _isSaved;

        public Yg2SavesAccess(SaveConfirmation confirmation)
        {
            ArgumentGuard.CheckNotNull(confirmation, nameof(confirmation));

            _confirmation = confirmation;
        }

        public event Action Prepared
        {
            add
            {
                YG2.onGetSDKData += value;
            }
            remove
            {
                YG2.onGetSDKData -= value;
            }
        }

        public bool IsReady => YG2.isSDKEnabled;

        public SavesYG Data => YG2.saves;

        public UniTask ConfirmSavedAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsReady == false)
            {
                throw new InvalidOperationException("Cannot confirm a save before SDK readiness.");
            }

            if (_isSaved == false || _latestSave.Status == UniTaskStatus.Faulted
                || _latestSave.Status == UniTaskStatus.Canceled)
            {
                Save();
            }

            return _latestSave.AttachExternalCancellation(cancellationToken);
        }

        public void Save()
        {
            if (YG2.isSDKEnabled == false)
            {
                throw new InvalidOperationException(
                    "Yg2SavesAccess: cannot save before the SDK data is ready.");
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            YG2.saves.idSave++;

            if (YG2.infoYG.Storage.saveLocal)
            {
                YG.Insides.YGInsides.SaveLocal();
            }

            _latestSave = UniTask.CompletedTask;

            if (YG2.infoYG.Storage.saveCloud)
            {
                string json = UnityEngine.JsonUtility.ToJson(YG2.saves);
                _latestSave = _confirmation.ConfirmAsync(json, CancellationToken.None).Preserve();
                _latestSave.Forget();
            }
#else

            YG2.SaveProgress();
            _latestSave = UniTask.CompletedTask;
#endif
            _isSaved = true;
        }
    }
}
