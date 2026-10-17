# AdrenalineBoost

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Upgrades/Runtime/AdrenalineBoost.cs`

Перк **Adrenaline**: ускорение в финальном отсчёте таймера.

## Механика

- `Enable()` / `Disable()` — зовёт [[GameplayOrchestrator]] на пороге `_finalCountdownSeconds` и на финише/выходе; сам Timer не слушает
- `Enable`: перк не куплен (`PlayerUpgrades.HasAdrenaline`) → no-op; иначе `Movement.SetBoostMultiplier(GetPerk(PerkType.Adrenaline).Value)` — множитель из [[UpgradesConfig]]
- `Disable`: `SetBoostMultiplier(1f)`
- События `BoostStarted`/`BoostEnded` — презентация ([[Эффекты игрока | виньетка, AdrenalineSound]])

## Связи

- [[PlayerUpgrades]], [[Движение | Movement]] (единственный канал множителя скорости)
