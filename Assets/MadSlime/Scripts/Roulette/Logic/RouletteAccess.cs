using System;
using Game;

namespace Roulette
{
    public class RouletteAccess
    {
        private Func<int> _price;
        private Func<int> _freeRemainSeconds;
        private Func<int> _adSpinsLeft;
        private Func<bool, bool> _canSpin;
        private Action<bool> _pay;
        private bool _hasAdvertising;

        public bool HasAdvertising => _hasAdvertising;

        public RouletteAccess(
            Func<int> price,
            Func<int> freeRemainSeconds,
            Func<int> adSpinsLeft,
            Func<bool, bool> canSpin,
            Action<bool> pay,
            bool hasAdvertising)
        {
            ArgumentGuard.CheckNotNull(price, nameof(price));
            ArgumentGuard.CheckNotNull(freeRemainSeconds, nameof(freeRemainSeconds));
            ArgumentGuard.CheckNotNull(adSpinsLeft, nameof(adSpinsLeft));
            ArgumentGuard.CheckNotNull(canSpin, nameof(canSpin));
            ArgumentGuard.CheckNotNull(pay, nameof(pay));

            _price = price;
            _freeRemainSeconds = freeRemainSeconds;
            _adSpinsLeft = adSpinsLeft;
            _canSpin = canSpin;
            _pay = pay;
            _hasAdvertising = hasAdvertising;
        }

        public int GetPrice()
        {
            return _price();
        }

        public int GetFreeRemainSeconds()
        {
            return _freeRemainSeconds();
        }

        public int GetAdSpinsLeft()
        {
            return _adSpinsLeft();
        }

        public bool CanSpin(bool advertisementGranted)
        {
            return _canSpin(advertisementGranted);
        }

        public void Pay(bool advertisementGranted)
        {
            _pay(advertisementGranted);
        }
    }
}
