# UiSpawner

**Слой:** UI · **Код:** `Assets/MadSlime/Scripts/UI/Spawning/UiSpawner.cs` (+ `UiLayer.cs`, `IShowable.cs`)

Единая точка показа попапных окон: `Spawn<T>(prefab, UiLayer)` — `resolver.Instantiate` под `SetParent`-корень (в Menu — `_runtimeWindows` из скоупа) → слой → `Show()`; `Release(window)` — симметричное уничтожение (владелец сначала снимает подписки). `Show(IShowable)` — показать существующий объект без спавна.

- `UiLayer` — полосы sortingOrder: `Hud = 0`, `Buttons = 10`, `Popup = 20`, пишется в корневой Canvas (fail-fast) с `overrideSorting = true`
- `IShowable` — контракт: `Show()` / `Hide()` / `event Action Closed`; для транзиентных окон `Hide()` = закрыть и уничтожить
- Регистрация: `Register<UiSpawner>(Lifetime.Scoped)` в каждом сценическом скоупе (окнам нужен сценический контейнер); Menu-скоуп ставит родителя через `SetParent`
- Спавнер никогда не трогает паузу — время морозит только [[Pauser]]

## Кто что показывает

| Показ | Чем |
|---|---|
| PauseMenu, WinMenu, FailMenu, LeaderboardMenu | `Spawn(prefab, Popup)` из [[UI-фабрики]] и [[Меню | MenuNavigation]] |
| Ежедневная/скин-рулетка, RouletteWinPopup | `Spawn` из [[Меню | MenuNavigation]] / [[Рулетки | RoulettePreparation]] |
| HUD Game | `Show(IShowable)` из [[HUD | GameplayHud]] + [[UiAnimations | AnimationScheduler]] |
