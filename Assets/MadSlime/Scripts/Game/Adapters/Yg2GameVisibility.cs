using System;
using YG;

namespace Game
{
    public class Yg2GameVisibility : IGameVisibility
    {
        public event Action Hidden
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

        public event Action Shown
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
