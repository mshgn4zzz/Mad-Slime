# TierTable

**Слой:** Core · **Код:** `Assets/MadSlime/Scripts/ScriptableObjects/Items/Tiers/TierTable.cs` (+ `Items/Tiers/TierLookup.cs`, `SizeTier.cs`)

Таблица `SizeTier → TierEntry (scale, mass)`: визуальный масштаб предмета при спавне и масса, которую игрок получает за поглощение. Тиры: `Small, Medium, Large, Huge, Giant, Boss`.

## Особенности

- `TierLookup` — рантайм-обёртка над таблицей (строится один раз потребителем: [[LevelGenerator | LevelPlacementPlanner]], [[Рост игрока | MassGainer]])
- Записи нет → `InvalidOperationException` (fail-fast по канону проекта); геттеры клампят: `scale >= 0.01`, `mass >= 1`

## Связи

- Потребители: [[LevelGenerator]], [[Рост игрока]], [[Камера | CameraPullAnimator]] (сила пулла по массе)
- Регистрация: `RegisterInstance` в [[ProjectLifetimeScope]]

## Нюансы

- Баланс порогов игрока ([[PlayerConfig]]) и масс предметов здесь — две таблицы, рассинхрон не ловится автоматически
