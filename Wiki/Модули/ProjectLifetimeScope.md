# ProjectLifetimeScope

**Слой:** DI · **Код:** `Assets/MadSlime/Scripts/DI/ProjectLifetimeScope.cs` · префаб `Assets/MadSlime/Resources/Prefabs/DI/ProjectScope.prefab`

Корневой скоуп VContainer. В сценах его нет: на префаб указывает `RootLifetimeScope` в `VContainerSettings.asset`, поэтому он автоспавнится из Resources в каждой сцене и живёт в DontDestroyOnLoad. Родитель всех [[Скоупы сцен | сценических скоупов]].

## Что регистрирует

- **Сейв-слой** (единый конструктором): `Yg2SavesAccess` → `Saver` → девять интерфейсов-хранилищ (`IBalanceStorage`, `ILevelStorage`, `IAudioStorage`, `IUpgradesStorage`, `ISkinStorage`, `IRouletteStorage`, `ILanguageStorage`, `ICollectedItemsStorage`) + `ISavesReadiness` + `ISaveConfirmation` + транзакция `Action<Action>` — см. [[Интерфейсы Core (шов YG2)]], [[YG2-адаптеры]], [[SavesYG]]
- **Компоненты** (ссылки на префабе, fail-fast проверки): [[Скины | SkinInventory]], [[PlayerUpgrades]], SaveConfirmation, [[LeaderboardReporter | Yg2LeaderboardService → ILeaderboardService]], [[LocalizationService]], `AudioPlayer` (один класс на три шва: `IMusicPlayer` + `IGameSoundPlayer` + `IUISoundPlayer`), AudioMixerController, [[SceneNavigator]]
- **SO-ассеты** (RegisterInstance): LevelsCatalog ([[Конфиги уровней]]), [[TierTable]], LayoutsLibrary ([[Конфиги уровней | генерация зон]])
- **Квота:** `QuotaBoard` + `QuotaCounter` ([[QuotaGenerator]]), `LevelConfigResolver`
- **Шов платформы** (`new Yg2...()` в конфигураторе): `IAdsService → Yg2AdsService`, `ILanguageProvider → Yg2LanguageProvider`, `IGameVisibility → Yg2GameVisibility`, `IGameplayReporter → Yg2GameplayReporter`

## Точка входа

Сессионного бootstrap-класса нет: после билда контейнера сценические скоупы в `RegisterBuildCallback` зовут `SceneNavigator.EnsureInitialized()`, а сценические оркестраторы регистрируют сессии ([[SceneNavigator]], [[Скоупы сцен]]).

## Нюансы

- `new Yg2...()` зашиты в коде конфигуратора — смена платформы = правка DI-кода
- Сценические скоупы цепляются к нему неявно (parentReference в сценах пуст) — связь существует только на уровне VContainer-конвенций
- Скоуп держит ~10 сериализованных ссылок — правка префаба ProjectScope бьёт по всем сценам
