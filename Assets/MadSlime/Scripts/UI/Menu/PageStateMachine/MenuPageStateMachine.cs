using System;
using MadSlime.StateMachine.Logic;
using MadSlime.UI.Menu.PageStateMachine.States;

namespace MadSlime.UI.Menu.PageStateMachine
{
    public class MenuPageStateMachine : BaseStateMachine<IMenuPageState>, IDisposable
    {
        private readonly MainPageState _main = new MainPageState();
        private readonly SkinsPageState _skins = new SkinsPageState();
        private readonly UpgradesPageState _upgrades = new UpgradesPageState();

        public MenuPageStateMachine()
        {
            RegisterState(_main);
            RegisterState(_skins);
            RegisterState(_upgrades);
            Subscribe();
        }

        public event Action MainRequested;
        public event Action SkinsRequested;
        public event Action UpgradesRequested;
        public event Action Stopped;

        public void Start()
        {
            if (IsRunning() == false)
            {
                SwitchState<MainPageState>();
            }
        }

        public bool IsRunning()
        {
            return CurrentState != null;
        }

        public bool IsMain()
        {
            return CurrentState == _main;
        }

        public bool IsSkins()
        {
            return CurrentState == _skins;
        }

        public bool IsCollection()
        {
            return CurrentState == _skins || CurrentState == _upgrades;
        }

        public void OpenSkins()
        {
            if (IsMain())
            {
                SwitchState<SkinsPageState>();
            }
        }

        public void OpenUpgrades()
        {
            if (IsMain())
            {
                SwitchState<UpgradesPageState>();
            }
        }

        public void NavigateBack()
        {
            if (IsCollection())
            {
                SwitchState<MainPageState>();
            }
        }

        public override void Stop()
        {
            base.Stop();
            Stopped?.Invoke();
        }

        public void Dispose()
        {
            Unsubscribe();
            base.Stop();
        }

        private void Subscribe()
        {
            _main.Entered += OnMain;
            _skins.Entered += OnSkins;
            _upgrades.Entered += OnUpgrades;
        }

        private void Unsubscribe()
        {
            _main.Entered -= OnMain;
            _skins.Entered -= OnSkins;
            _upgrades.Entered -= OnUpgrades;
        }

        private void OnMain()
        {
            MainRequested?.Invoke();
        }

        private void OnSkins()
        {
            SkinsRequested?.Invoke();
        }

        private void OnUpgrades()
        {
            UpgradesRequested?.Invoke();
        }
    }
}
