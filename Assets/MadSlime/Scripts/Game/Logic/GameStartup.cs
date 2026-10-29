using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Saves.Interfaces;
using MadSlime.Skins.Data;

namespace MadSlime.Game.Logic
{
    public class GameStartup
    {
        private ISavesReadiness _readiness;
        private SkinInventory _inventory;

        public GameStartup(ISavesReadiness readiness, SkinInventory inventory)
        {
            _readiness = readiness;
            _inventory = inventory;
        }

        public event Action Initialized;

        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
            {
                return;
            }

            await UniTask.WaitUntil(() => _readiness.IsReady, cancellationToken: cancellationToken);
            _inventory.Initialize();
            IsInitialized = true;
            Initialized?.Invoke();
        }
    }
}
