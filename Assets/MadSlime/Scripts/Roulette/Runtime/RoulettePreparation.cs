using System;
using Audio;
using Game;
using Saves;
using Shop;
using UnityEngine;
using VContainer;

namespace Roulette
{
    public class RoulettePreparation : MonoBehaviour
    {
        [SerializeField] private RoulettePresenter _dailyRoulette;
        [SerializeField] private RoulettePresenter _skinRoulette;

        private RouletteConfig _config;
        private ShopContent _content;
        private RouletteFactory _factory;
        private AdScheduler _ads;
        private IUISoundPlayer _sound;
        private ISaveConfirmation _saveConfirmation;

        [Inject]
        public void Construct(RouletteConfig config, ShopContent content, RouletteFactory factory,
            AdScheduler ads, IUISoundPlayer sound, ISaveConfirmation saveConfirmation)
        {
            _config = config;
            _content = content;
            _factory = factory;
            _ads = ads;
            _sound = sound;
            _saveConfirmation = saveConfirmation;
        }

        public void Prepare()
        {
            if (_dailyRoulette == null || _skinRoulette == null)
            {
                throw new InvalidOperationException($"{name}: assign both roulette presenters.");
            }

            int dailySectorCount = _dailyRoulette.View.Wheel.SectorCount;
            int skinSectorCount = _skinRoulette.View.Wheel.SectorCount;
            RouletteConfigValidator validator = new RouletteConfigValidator();
            validator.Validate(_config, _content.SkinItems, dailySectorCount, skinSectorCount);
            _dailyRoulette.Setup(_factory.CreateDaily(dailySectorCount), _ads, _sound, _saveConfirmation);
            _skinRoulette.Setup(_factory.CreateSkins(_content.SkinItems, skinSectorCount), _ads, _sound, _saveConfirmation);
        }
    }
}
