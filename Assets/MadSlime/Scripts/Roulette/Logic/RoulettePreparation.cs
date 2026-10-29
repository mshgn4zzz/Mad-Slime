using System;
using MadSlime.Ads.Logic;
using MadSlime.Audio.Interfaces;
using MadSlime.Roulette.Data;
using MadSlime.Roulette.Logic.StateMachine;
using MadSlime.Saves.Interfaces;
using MadSlime.Shop.Data;
using MadSlime.UI.Menu;
using MadSlime.UI.Roulette;
using MadSlime.UI.Spawning;
using MadSlime.Validation;

namespace MadSlime.Roulette.Logic
{
    public class RoulettePreparation
    {
        private readonly RouletteView _dailyRoulettePrefab;
        private readonly RouletteView _skinRoulettePrefab;

        private readonly RouletteConfig _config;
        private readonly ShopContent _content;
        private readonly RouletteFactory _factory;
        private readonly AdScheduler _ads;
        private readonly IUISoundPlayer _sound;
        private readonly ISaveConfirmation _saveConfirmation;
        private readonly UiSpawner _spawner;
        private RouletteModel _dailyModel;

        private RouletteModel _skinModel;

        public RoulettePreparation(
            RouletteView dailyRoulettePrefab,
            RouletteView skinRoulettePrefab,
            RouletteConfig config,
            ShopContent content,
            RouletteFactory factory,
            AdScheduler ads,
            IUISoundPlayer sound,
            ISaveConfirmation saveConfirmation,
            UiSpawner spawner)
        {
            ArgumentGuard.CheckNotNull(dailyRoulettePrefab, nameof(dailyRoulettePrefab));
            ArgumentGuard.CheckNotNull(skinRoulettePrefab, nameof(skinRoulettePrefab));
            _dailyRoulettePrefab = dailyRoulettePrefab;
            _skinRoulettePrefab = skinRoulettePrefab;
            _config = config;
            _content = content;
            _factory = factory;
            _ads = ads;
            _sound = sound;
            _saveConfirmation = saveConfirmation;
            _spawner = spawner;
        }

        public RouletteModel SkinModel => _skinModel;

        public void Prepare()
        {
            ArgumentGuard.CheckAssigned(
                _dailyRoulettePrefab, nameof(RoulettePreparation), nameof(_dailyRoulettePrefab));
            ArgumentGuard.CheckAssigned(_skinRoulettePrefab, nameof(RoulettePreparation), nameof(_skinRoulettePrefab));

            int dailySectorCount = _dailyRoulettePrefab.Wheel.SectorCount;
            int skinSectorCount = _skinRoulettePrefab.Wheel.SectorCount;
            RouletteConfigValidator validator = new RouletteConfigValidator();

            validator.Validate(_config, _content.SkinItems, skinSectorCount);
            _dailyModel = _factory.CreateDaily(dailySectorCount);
            _skinModel = _factory.CreateSkins(_content.SkinItems, skinSectorCount);
            _dailyModel.RefreshSlots();
            SkinModel.RefreshSlots();
        }

        public RoulettePresenter SpawnDaily(MenuPopupStack popups)
        {
            return Spawn(_dailyRoulettePrefab, _dailyModel, popups, false);
        }

        public RoulettePresenter SpawnSkins(MenuPopupStack popups)
        {
            return Spawn(_skinRoulettePrefab, SkinModel, popups, true);
        }

        private RoulettePresenter Spawn(
            RouletteView prefab,
            RouletteModel model,
            MenuPopupStack popups,
            bool isClosingAfterPrize)
        {
            if (model == null)
            {
                throw new InvalidOperationException(
                    "RoulettePreparation: prepare roulette models before opening a popup.");
            }

            RouletteView view = _spawner.Spawn(prefab, UiLayer.Popup);

            if (view.TryGetComponent(out RoulettePresenter presenter) == false)
            {
                throw new InvalidOperationException($"{view.name}: RoulettePresenter is required.");
            }

            presenter.Setup(
                new RouletteStateMachine(model, _saveConfirmation, isClosingAfterPrize),
                _ads,
                _sound,
                _spawner,
                popups);
            return presenter;
        }
    }
}
