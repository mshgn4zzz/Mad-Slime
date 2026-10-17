# FillConfig

**Слой:** Core · **Код:** `Assets/MadSlime/Scripts/ScriptableObjects/ShapeFill/FillConfig.cs`

Все тайминги и визуал сцены Fill. Потребители — [[Заливка фигуры]] и [[FillSceneOrchestrator]].

## Поля

- **Стадии заливки:** `FillDelay` (0.55), Normal: `SpawnInterval` 0.04 / `FlightDuration` 0.5; Boosted: 0.015 / 0.25; Instant: `InstantFillDuration` 0.25 / полёт 0.1
- **Волны:** `BonusWaveDelay` (0.35) между квотной и бонусной, `BonusTintColor` + `BonusTintStrength` (0.3)
- **Бордюр:** `BorderCascadeDuration` (0.5), `BorderColor`
- **Призрак фигуры:** `GhostOpacity` (0.4)
- **Финал 100%:** `ShapePunch*` (сила 0.06, длительность 0.4, vibrato, elasticity), `FovKick` (9°) + `FovDuration` (0.6)
