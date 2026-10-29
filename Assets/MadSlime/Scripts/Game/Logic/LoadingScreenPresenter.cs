using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Skins.Data;
using GameplayMachine = MadSlime.Game.GameplayStateMachine.Logic.GameplayStateMachine;

namespace MadSlime.Game.Logic
{
    public class LoadingScreenPresenter
    {
        private LoadingScreenController _screen;
        private SkinInventory _inventory;
        private GameStartup _startup;
        private GameplayMachine _preparing;
        private bool _isVisible;

        public LoadingScreenPresenter(LoadingScreenController screen, SkinInventory inventory, GameStartup startup)
        {
            _screen = screen;
            _inventory = inventory;
            _startup = startup;
        }

        public void Show()
        {
            if (_isVisible)
            {
                return;
            }

            _screen.Show();
            _isVisible = true;
            _startup.Initialized += OnStartupInitialized;
            OnStartupInitialized();
        }

        public void ObservePreparation(GameplayMachine machine)
        {
            ReleasePreparation();
            _preparing = machine;
            _preparing.PreparationProgressChanged += OnProgressChanged;
        }

        public UniTask CompleteAsync(CancellationToken cancellationToken)
        {
            return _screen.CompleteAsync(cancellationToken);
        }

        public void Hide()
        {
            ReleasePreparation();
            _startup.Initialized -= OnStartupInitialized;
            _isVisible = false;
            _screen.Hide();
        }

        private void ReleasePreparation()
        {
            if (_preparing != null)
            {
                _preparing.PreparationProgressChanged -= OnProgressChanged;
                _preparing = null;
            }
        }

        private void OnStartupInitialized()
        {
            if (_startup.IsInitialized)
            {
                _screen.SetSkin(_inventory.SelectedSkinId);
            }
            else
            {
                _screen.SetSkin(SkinInventory.InitialSkinId);
            }
        }

        private void OnProgressChanged(float progress)
        {
            if (_isVisible)
            {
                _screen.SetProgress(progress);
            }
        }
    }
}
