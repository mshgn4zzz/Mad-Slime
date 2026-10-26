# Прогресс: правки кода по отчёту ревью (26.10.2026)

Порядок: [x] сделано, [~] в работе, [ ] не начато. Компиляция: `/tmp/unity-check/all.sh` — должна быть зелёной после каждого кластера.

## Кластеры правок

- [x] A. Мелкие точечные: AudioMixerController (SFXVolume→SfxVolume, дубли, округление не сохраняется), UIButtonSound (PlayClick→OnClick), фабрики Get→Create, RouletteConfigValidator (Validate-метод, скобки &&/||), PlayerUpgrades (Validate-методы), FillProgressUI (percent→прогресс-доля)
- [x] B. Мёртвый код: сделано AttractConfig._orbitStrength/OrbitStrength, CrawlAnimator.Frequency, ModelDeformer.Compression, Timer.Duration + Timer.Stopped, ZoneLayoutPlanner.Positions, SceneNavigator._previousSceneId/PreviousSceneId.
- [x] C. Магические числа: сделано QuotaGenerator, SavesYG ("Slime"), LeaderboardRowView ("anonymous"), RouletteWinPopup (цвета→[SerializeField]), LocaleEntry/Localization (константы языков).
- [x] D. DTO: краткие свойства { get; } — FillResult, RouletteSlot, RouletteOutcome, RoulettePrize, SaveConfirmationRequest
- [x] E. Дубли: CameraImpulseAnimator, BarFiller, UpgradePurchaseModel, ValidateAssigned, GameplayOrchestrator TeardownSession, MenuNavigation Release<T>, FillSceneOrchestrator ReloadLevel (LoadNext/Restart слиты)
- [x] F. Подписки OnEnable/OnDisable: AudioSettingsPanel, LeaderboardMenu
- [x] G. FSM: GameplayOrchestrator (TryTransitTo везде) + RoulettePresenter (RouletteState: Hidden/Idle/WaitingForAd/ConfirmingSave/Spinning/SaveFailed)
- [x] H. NullRef/гварды: всё сделано — SceneNavigator таймаут+раздельные null-чеки, Saver громкость <1f, FillUIFabric/AdScheduler ShowDoubleReward(onGranted,onNotGranted), RoulettePresenter dwell-попап (destroy-токен + захват prize/onComplete), GenericOverlapDetector Exited null+SetRadius>0, ItemAttractor Definition, FillSceneOrchestrator/LocalizationService OnDisable, Wallet/LeaderboardReporter/SkinApplier/FillResultCalculator/LevelLabelUI/WinMenu/RouletteView Awake, MenuNavigation SkinModel-гвард, LocalizedText ключ в Awake, CameraObstacleDetector/SmellAbilityActivator Awake
^- [x] I. Прочее свободное (включая ItemAttractor/Absorber «притягивание при поглощении» — осознанно пропущено, в отчёте помечено «не уверен», правка грозит регрессом поведения): CameraFollow тряска, AudioMixerController сохранение округления, Pauser поллинг→событие, ItemCollector отступ, ProfilerMarker static ×3, SetRadius консистентность, TierResolver fallback (СДЕЛАНО через GetThreshold+throw), PlayerTier/Rewarder порядок полей, Movement кэш векторов, PlayerScaler/ScaleMultiplier, LevelRange валидация, LevelConfigResolver fallback, LevelPlacementPlanner ошибка, LevelItemsSpawner (dict, O(n²)), LevelGenerator/LevelSurface тема, SpawnZone [Serializable], enumValueIndex ×2, SerializedObject имена-константы ×2, QuotaItemHighlighter null-стиль, LookAtCamera пробел, CameraConfig атрибуты, MusicChannel исключение, UiSpawner исключения, RouletteWinPopup paramName (СДЕЛАНО раздельные ArgumentNullException), Saver поля-среди-методов, FailMenu/WinMenu $""→ToString, QuotaUI раскладка, GridBuilder ReadPixels, скобки: ShapeFiller/GridBuilder/CubeSpawnSchedule/GridCameraFitter, LayoutPreviewDrawer if→switch, tools/verification/run-progression-fill.sh копия проекта, SceneNavigator сообщение null-чека
^- [x] J. Один файл — один класс: UpgradesConfig(+UpgradeEntry+PerkEntry), RouletteConfig(+RouletteSector), LocalizationTable(+LocaleEntry), SkinRarityTable(+RaritySettings), PropSet(+PropVariant), TierTable(+TierEntry)
- [x] K. SRP: Saver РАЗДЕЛЁН — SavesCore + AudioStorage/BalanceStorage/UpgradesStorage/SkinsStorage/RouletteStorage/LanguageStorage/CollectedItemsStorage/ProgressStorage (ProjectLifetimeScope обновлён). FillUIFabric → FillUiPresenter + FillPauseFlow + FillRescueFlow (WindowAction public). SceneNavigator → SceneSessionRegistry + LoadingScreenController. MenuNavigation → MenuNavigation + MenuRouletteFlow. ItemPropFactory → 4 partial-файла.
- [x] L. DIP: SavesTransaction-делегат вместо Action<Action>/Saver — RouletteFactory, RouletteModel, MenuContent, UpgradePurchaseModel, UpgradesPagePresenter, FillSceneOrchestrator, ProjectLifetimeScope, MenuLifetimeScope
- [x] M. Anti-DIP: GameplayOrchestrator — СДЕЛАНО (19 зависимостей: 7 ушли в GameplayActivityController + FinalCountdownEffects, оба Register в GameLifetimeScope). FillSceneOrchestrator 14 → 9 (сейв/награды/лидерборд/прогресс ушли в FillSessionSaver, Register в FillLifetimeScope).
^- [x] N. Namespace по папкам: полная нормализация (модуль=папка, UI.<Sub>, <Модуль>.Editor); git mv Scripts/Input → Scripts/PlayerInput
^- [x] O. Длинные строки >120 символов по всем файлам отчёта
- [x] P. Финал: компиляция зелёная (5/5), критик-сабагент отработал, корректировка внесена:
  - RoulettePresenter: FormatRemainingTime (minutes/seconds до строки); при отмене попапа приза в WinDwell — Debug.LogWarning (приз остаётся выданным, объяснение в логе).
  - GridBuilder: скобки + `== false` в GetFormBounds/EncapsulateCells/FitToPlayArea.
  - PlayerTier/Rewarder: приватные поля убраны из блока свойств.
  - GameplayOrchestrator.FinishSession: переход через TryTransitTo(Finished) (wasRunning снимается до перехода).
  - SceneNavigator: ApplyLoadingSkin (fallback InitialSkinId) на каждом показе экрана загрузки — регресс восстановлен.
  - GameplayActivityController/FinalCountdownEffects: раздельные null-гварды на каждый параметр.
  - AudioStorage: тернарник и комментарии убраны (if/else).
  - FillSceneOrchestrator: мёртвое _isLevelProgressCommitted удалено; ResetProgressAndLoadMenuAsync — EnsureInitialized до try.
  - FillSessionSaver/FillPauseFlow/FillRescueFlow: ArgumentNullException-гварды в конструкторах.
  - Формат: пустые строки (MenuNavigation/MenuPopupStack/FillUiPresenter).
  - .meta: созданы 21 (11 из списка критика + 9 в Saves/Logic + Saves/Logic.meta).

Осознанно не правлено (обоснование):
- LevelRange: валидация через LevelsCatalog.OnValidate (LevelRange — [Serializable]-класс, OnValidate невозможен) + throw в LevelConfigResolver — находка закрыта комбинацией.
- SavesYG: комментарии-оправдание namespace YG (partial-слияние с плагином) и const InitialSkinId оставлены.
- ItemAttractor/ItemAbsorber «притягивание при поглощении» — пропуск осознанный (в отчёте «не уверен», риск регресса).

## Решения по спорным пунктам
- SavesYG: поля не переименовывать (контракт сейвов; отчёт сам это фиксирует).
- СО-компаньоны: сплитить (отчёт главнее AI_RULES-канона).
- Скрипт-тулзы не создавать; одноразовые shell-команды в /tmp — можно.
