# RewardConfig

**Слой:** Core · **Код:** `Assets/MadSlime/Scripts/ScriptableObjects/Economy/RewardConfig.cs`

Параметры награды за заливку. Потребитель — [[Rewarder]].

## Поля

- `BaseReward` (50) — награда за победу на 1-м уровне
- `RewardMultiplier` (1.27) — геометрический рост награды: `round(BaseReward × Multiplier^(level−1))`
- `MaxReward` (5000) — потолок награды
- `LoseMultiplierThreshold` (0.25) — с какой заливки платится утешение
- `LoseRewardDivisor` (4) — утешение = `GetReward(level) / делитель`

Валидация при расчёте: `base ≥ 0`, `max ≥ base`, `multiplier ≥ 1` — иначе исключение. Пример: 50 → 64 → 81 → 103 → … на 20-м уровне ≈ 2800, потолок на ~25-м.
