# Интерфейсы Core (шов YG2)

**Слой:** Core · **Код:** `Saves/Interfaces/`, `Ads/Interfaces/`, `Leaderboards/Interfaces/`, `Localization/Interfaces/`, `Game/Interfaces/`

Единственное место, где игра касается Яндекс-платформы абстрактно. Код `Gameplay`/`UI` YG2 не видит вообще — прямые `using YG` вне `Adapters/` и `Saves/` считаются ошибкой ревью.

## Зачем

Изоляция платформы: asmdef'ы не могут ссылаться на Assembly-CSharp, где живёт YG2-плагин. Игра говорит с платформой только через эти контракты, реализации подменяются в одном месте — [[ProjectLifetimeScope]].

## Состав

**Сейв-шов** (вместо старого божественного `ISavesAccess` — узкие хранилища; реализует все [[Saver]]):

| Интерфейс | Отвечает за |
|---|---|
| `ISavesReadiness` | `IsReady` + `event Ready` (SDK-данные догрузились) |
| `IBalanceStorage` | монеты |
| `ILevelStorage` | `CurrentLevel`/`MaxLevel`, `SetLevelProgress` |
| `IAudioStorage` | `musicVolume`/`sfxVolume` |
| `IUpgradesStorage` | уровни апгрейдов + купленные перки |
| `ISkinStorage` | `SelectedSkinId`, открытые скины, витрина |
| `IRouletteStorage` | таймеры/счётчики рулеток (`PruneAdSpinsBefore` и т.п.) |
| `ILanguageStorage` | выбранный язык |
| `ICollectedItemsStorage` | счётчик съеденных предметов (лидерборд), `CommitCollectedItems()` |
| `ISaveConfirmation` + `Action<Action>` | подтверждение сейва (`ConfirmSavedAsync`) и транзакция |

**Прочие швы:**

| Интерфейс | Отвечает за | Реализация |
|---|---|---|
| `IAdsService` | реклама: `ShowRewarded`, `ShowInterstitial`, `IsAdShowing`, `IsPauseGame` | [[YG2-адаптеры \| Yg2AdsService]] |
| `ILeaderboardService` | `SetScore`, `RequestEntriesAsync`, `OpenAuthDialog`, `IsAuthorized` | Yg2LeaderboardService ([[Лидерборды]]) |
| `ILanguageProvider` | язык платформы, `LanguageSwitched` | Yg2LanguageProvider |
| `IGameVisibility` | сворачивание вкладки: `Hidden`/`Shown` ([[Pauser]]) | Yg2GameVisibility |
| `IGameplayReporter` | аналитика: `ReportStart`/`ReportStop` | Yg2GameplayReporter |

## Связи

- **Регистрация всех реализаций:** [[ProjectLifetimeScope]]; транзакция — `Saver.ExecuteTransaction`
- **Потребители:** [[Saver]]-хранилища (Wallet, PlayerUpgrades, SkinInventory, оркестраторы), [[AdScheduler]] и [[Pauser]] (IAdsService), [[Лидерборды]] (ILeaderboardService), [[LocalizationService]] (ILanguageProvider)
