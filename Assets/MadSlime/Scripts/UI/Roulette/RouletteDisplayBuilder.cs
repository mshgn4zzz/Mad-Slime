using System.Collections.Generic;
using MadSlime.Roulette.Logic;
using MadSlime.Roulette.Logic.StateMachine;
using UnityEngine;

namespace MadSlime.UI.Roulette
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    public class RouletteDisplayBuilder
    {
        private readonly Localization _localization;

        public RouletteDisplayBuilder(Localization localization)
        {
            _localization = localization;
        }

        public void BuildWheel(RouletteModel model, RouletteView view)
        {
            List<RouletteSectorIcon> icons = new List<RouletteSectorIcon>();

            for (int i = 0; i < model.Slots.Count; i++)
            {
                RouletteSlot slot = model.Slots[i];
                Sprite icon;
                string label = null;

                if (slot.IsHidden)
                {
                    icon = view.SecretIcon;
                }
                else if (slot.Skin != null)
                {
                    icon = slot.Skin.Icon;
                }
                else
                {
                    icon = view.GetCoinIcon(slot.Coins);
                    label = $"×{slot.Coins}";
                }

                icons.Add(new RouletteSectorIcon(icon, label));
            }

            view.Wheel.Build(icons);
        }

        public RouletteDisplayData Build(RouletteStateMachine machine)
        {
            RouletteModel model = machine.Model;
            string title = _localization.Get("skin_roulette_title");
            string spinText = model.GetPrice().ToString();

            if (model.IsAdvertisingAvailable)
            {
                title = _localization.Get("daily_roulette_title");

                int remaining = model.GetFreeRemainSeconds();

                spinText = _localization.Get("roulette_free_ready");

                if (remaining > 0)
                {
                    spinText = FormatRemainingTime(remaining);
                }
            }

            if (machine.IsSaveFailed())
            {
                title = _localization.Get("roulette_save_error");
                spinText = _localization.Get("leaderboard_retry");
            }

            RouletteDisplayData data = new RouletteDisplayData
            {
                Title = title,
                IsSpinVisible = model.IsAdvertisingAvailable || machine.IsSaveFailed(),
                SpinText = spinText,
                IsSpinAvailable = machine.IsSpinAvailable(),
                IsAdvertisementVisible = model.IsAdvertisingAvailable && machine.IsSaveFailed() == false,
                AdText = string.Format(_localization.Get("roulette_ad_spins"), model.GetAdSpinsLeft()),
                IsAdvertisementAvailable = machine.IsAdvertisementAvailable(),
                IsCloseVisible = model.IsAdvertisingAvailable,
                IsClosingAllowed = machine.IsClosingAllowed()
            };

            return data;
        }

        private string FormatRemainingTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return $"{minutes}:{seconds:00}";
        }
    }
}
