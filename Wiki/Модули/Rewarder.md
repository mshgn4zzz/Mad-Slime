# Rewarder

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Economy/Runtime/Rewarder.cs` · сцена Fill

Начисление монет за итог заливки. Единственный потребитель [[RewardConfig]]; коммиты дергает [[FillSceneOrchestrator]] внутри сейв-транзакций.

## Механика

- `ResetLevelResult()` — в подготовке сцены
- `CommitLevelResult(percent)` (один раз за уровень, повтор — исключение): `_levelReward = GetReward(CurrentLevel)` (награда **растёт по уровням** — база × множитель^уровень, cap `MaxReward`); win = percent ≥ 1 → полная награда; `percent ≥ LoseMultiplierThreshold` (0.25) → `levelReward / LoseRewardDivisor` (4); меньше — 0. Положительная сумма сразу `Wallet.Add`
- `CommitRescueTopUp()` (спасение за рекламу): если ещё не win — доплата `levelReward − granted` → `IsWin = true`
- `GrantedTotal`, `IsWin` — читает [[FillSceneOrchestrator]] для событий `Win`/`Failed` ([[UI-фабрики | FillUIFabric]])

## Цепочка

[[FillSceneOrchestrator]] (транзакция) → Rewarder → [[Wallet]] (Add ⇒ сейв) → события Win/Failed → окна

## Нюансы

- Награда растёт с уровнем (см. [[RewardConfig]]) — баланс дохода подпитывает дорожающие скины
- «Почти win» (95%+) платит те же levelReward/4, что и 25% — порог один
