using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.GameFSM;
using MadSlime.Saves.Interfaces;
using MadSlime.Skins.Data;

namespace MadSlime.UI.Menu
{
    public class ResetProgressFlow
    {
        private SceneStateMachine _scenes;
        private ISaveConfirmation _saveConfirmation;
        private IPlayerProgressStorage _progress;
        private SkinInventory _inventory;

        public ResetProgressFlow(
            SceneStateMachine scenes,
            ISaveConfirmation saveConfirmation,
            IPlayerProgressStorage progress,
            SkinInventory inventory)
        {
            _scenes = scenes;
            _saveConfirmation = saveConfirmation;
            _progress = progress;
            _inventory = inventory;
        }

        public bool IsRunning { get; private set; }

        public async UniTask RunAsync(CancellationToken cancellationToken)
        {
            if (IsRunning || _scenes.IsTransitioning || _scenes.IsStopped)
            {
                return;
            }

            IsRunning = true;

            await _saveConfirmation.ConfirmSavedAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            _progress.ResetProgress();
            _inventory.Initialize();
            await _scenes.RestartCurrentAsync();
            IsRunning = false;
        }
    }
}
