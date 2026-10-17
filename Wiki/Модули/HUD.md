# HUD

**Слой:** UI · **Код:** `Assets/MadSlime/Scripts/UI/HUD/` (GameplayHud, TimerUI, LevelLabelUI) + `Quota/UI/QuotaUI.cs` + `Player/Growth/UI/GrowthBarView.cs` + `ShapeFill/UI/FillProgressUI.cs`

Набор Mono-view: каждое подписано на один источник данных и только рисует. Хореография показа — `GameplayHud` + `AnimationScheduler` ([[UiAnimations]]).

## Состав

| Вью | Источник | Что показывает |
|---|---|---|
| **GameplayHud** | [[GameplayOrchestrator]] (события сессии) | сценарий появления/скрытия HUD: старт сцены → финальный отсчёт/геймплей |
| **TimerUI** | `Timer.Ticked` (`Action<float>`) | остаток секунд |
| **QuotaUI** + тарелки | `QuotaBoard.QuotaChanged` (`Action<int, QuotaEntry>`) | тарелки квоты: иконка + «осталось»; intro-каскад, pop при тике; твины `SetUpdate(true)` — живут под паузой |
| **GrowthBarView** | `PlayerTier.MassChanged`/`TierChanged` | прогресс массы до следующего тира ([[Рост игрока | TierResolver.GetTierProgress]]) + локализованное имя `tier_{tier}` |
| **LevelLabelUI** | локализация | «Уровень N» (`level_label`) |
| **FillProgressUI** | `ShapeFiller` | % заливки ([[Заливка фигуры]]) |

Все — method injection (`Construct`), подписки строго в `OnEnable`/`OnDisable`.

## Связи

- Источники: [[Timer]], [[Квота | QuotaBoard]], [[Рост игрока]], [[LocalizationService]], [[Заливка фигуры | ShapeFiller]]
- Инжект: [[Скоупы сцен | GameLifetimeScope]]
