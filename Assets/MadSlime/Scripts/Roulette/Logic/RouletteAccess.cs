using System;
using MadSlime.Validation;

namespace MadSlime.Roulette.Logic
{
    public class RouletteAccess
    {
        private Func<int> _price;
        private Func<int> _freeRemainSeconds;
        private Func<int> _adSpinsLeft;
        private Func<bool, bool> _canSpin;
        private Action<bool> _pay;
        private bool _isAdvertisingAvailable;

        public RouletteAccess(
            Func<int> price,
            Func<int> freeRemainSeconds,
            Func<int> adSpinsLeft,
            Func<bool, bool> canSpin,
            Action<bool> pay,
            bool isAdvertisingAvailable)
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
            _isAdvertisingAvailable = isAdvertisingAvailable;
        }

        public bool IsAdvertisingAvailable => _isAdvertisingAvailable;

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

        public bool IsSpinAvailable(bool isAdvertisementGranted)
        {
            return _canSpin(isAdvertisementGranted);
        }

        public void Pay(bool isAdvertisementGranted)
        {
            _pay(isAdvertisementGranted);
        }
    }
}
