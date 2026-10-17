# ShapeFillOrchestrator

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/ShapeFill/Runtime/ShapeFillOrchestrator.cs` (`RequireComponent(GridBuilder, ShapeFiller)`)

Фасад стадии заливки: собирает [[Заливка фигуры | GridBuilder/ShapeFiller/FillResultCalculator]]. Сценой владеет [[FillSceneOrchestrator]].

## API

- `Prepare()` — `ShapeFiller.Initialize` + `BuildShape` (под loading screen)
- `CalculateResult()` → `FillResult` — переводит проценты [[Квота | QuotaCounter]] прошедшего раунда в кубы (квотные + бонусные) от общего числа ячеек сетки
- `Show(result)` / `RescueShow()` / `Stop()` / `Accelerate()`
- `CanRescue`, событие `FillCompleted(percent)` — трансляция наверх

## Связи

- Вверх: [[FillSceneOrchestrator]] (награда/исходы), [[UI-фабрики | FillUIFabric]] ([[HUD | FillProgressUI]])
- Вниз: [[Заливка фигуры]]
