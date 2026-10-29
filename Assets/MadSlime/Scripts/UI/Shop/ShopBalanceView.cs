using MadSlime.Economy.Logic;
using MadSlime.Validation;
using TMPro;
using UnityEngine;

namespace MadSlime.UI.Shop
{
    public class ShopBalanceView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _moneyText;

        private Wallet _wallet;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_moneyText, name, nameof(_moneyText));
        }

        private void OnEnable()
        {
            if (_wallet == null)
            {
                return;
            }

            _wallet.BalanceChanged -= OnBalanceChanged;
            _wallet.BalanceChanged += OnBalanceChanged;
            OnBalanceChanged(_wallet.Balance, _wallet.Balance);
        }

        private void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.BalanceChanged -= OnBalanceChanged;
            }
        }

        public void Setup(Wallet wallet)
        {
            ArgumentGuard.CheckNotNull(wallet, nameof(wallet));

            OnDisable();
            _wallet = wallet;

            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnBalanceChanged(int previousBalance, int currentBalance)
        {
            _moneyText.text = currentBalance.ToString();
        }
    }
}
