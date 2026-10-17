# YG2-адаптеры

**Слой:** Adapters · **Код:** `Saves/Adapters/Yg2SavesAccess.cs`, `Ads/Adapters/Yg2AdsService.cs`, `Leaderboards/Adapters/Yg2LeaderboardService.cs`, `Localization/Adapters/Yg2LanguageProvider.cs`, `Game/Adapters/` (Yg2GameVisibility, Yg2GameplayReporter)

Шесть тонких обёрток над статическим `YG2`. Единственные классы (вместе с [[SavesYG]]), которым разрешён `using YG`. Регистрируются в [[ProjectLifetimeScope]] как реализации [[Интерфейсы Core (шов YG2) | интерфейсов Core]].

| Адаптер | Интерфейс | Что делает |
|---|---|---|
| **Yg2SavesAccess** | (сырой доступ) | прямые прокси на `YG2.saves.*`, `Save()` → `YG2.SaveProgress()`, `ConfirmSavedAsync`; `Ready` — транслатор `YG2.onGetSDKData`; `IsReady => YG2.isSDKEnabled` |
| **Yg2AdsService** | IAdsService | ретрансляция rewarded-событий плагина → свои; `ShowRewarded(id)`, `ShowInterstitial()`; `IsPauseGame => YG2.isPauseGame` |
| **Yg2LeaderboardService** | ILeaderboardService | `SetScore`/`RequestEntriesAsync` (таймаут 8 с) /`OpenAuthDialog`; маппинг ответов в `LeaderboardData` ([[Лидерборды]]) |
| **Yg2LanguageProvider** | ILanguageProvider | `YG2.lang` + транслатор `YG2.onSwitchLang` |
| **Yg2GameVisibility** | IGameVisibility | `Hidden`/`Shown` — сворачивание вкладки ([[Pauser]]) |
| **Yg2GameplayReporter** | IGameplayReporter | `YG2.GameplayStart()` / `GameplayStop()` |

## Зачем

asmdef'ы не ссылаются на Assembly-CSharp, где живёт плагин: игра видит только интерфейсы Core, адаптеры подменяются в одной точке ([[ProjectLifetimeScope]]). Смена платформы = переписать 6 маленьких классов.

## Нюансы

- `Yg2SavesAccess.Save()` **молча игнорируется**, пока `isSDKEnabled == false` — в редакторе прогресс тихо не сохраняется
- Автопауза плагина на Yandex-платформе выпилена (`#if`) — вкладку ловит только `Yg2GameVisibility`
- Поля сейва с «кривыми» JSON-ключами торчат наружу: `_openSkinIds` (с подчёркиванием), `musicVolume`/`sfxVolume` (с маленькой буквы) — см. [[SavesYG]]
