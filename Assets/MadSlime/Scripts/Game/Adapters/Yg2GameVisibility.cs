using System;
using MadSlime.Game.Interfaces;
using YG;

namespace MadSlime.Game.Adapters
{
    public class Yg2GameVisibility : IGameVisibility
    {
        public event Action VisibilitySuspended
        {
            add
            {
                YG2.onHideWindowGame += value;
            }
            remove
            {
                YG2.onHideWindowGame -= value;
            }
        }

        public event Action VisibilityRestored
        {
            add
            {
                YG2.onShowWindowGame += value;
            }
            remove
            {
                YG2.onShowWindowGame -= value;
            }
        }
    }
}
