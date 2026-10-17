# Saver

**Слой:** Saves · **Код:** `Assets/MadSlime/Scripts/Saves/Adapters/Saver.cs` (+ `SaveConfirmation`, `SaveConfirmationRequest`) · конструируется в [[ProjectLifetimeScope]]

Фасад сейва: один класс реализует все хранилища шва (`ISavesReadiness` + 8 storage-интерфейсов + `ISaveConfirmation`) поверх `Yg2SavesAccess` ([[YG2-адаптеры]]). Фасада PlayerProgress больше нет — потребители получают узкий интерфейс.

## Механика

- Геттеры читают `LoadData()` (актуальные данные SDK); сеттеры пишут поле и коммят изменения
- **Транзакция** `ExecuteTransaction(operation)` (`Action<Action>` в DI): снапшот JSON → операция → `Save()`; исключение в операции → откат `FromJsonOverwrite` + rethrow; вложенные транзакции запрещены. Транзакции подают: [[FillSceneOrchestrator]] (награда/спасение/уровень), фабрика рулеток ([[Рулетки]]), покупки ([[PlayerUpgrades]], [[Магазин]])
- `ConfirmSavedAsync(token)` ([[Интерфейсы Core (шов YG2) | ISaveConfirmation]]) — [[SceneNavigator]] не стартует сессию, пока сейв не подтверждён
- `CommitCollectedItems()` — сброс счётчика лидерборда при сворачивании ([[Pauser]])
- `PruneAdSpinsBefore(cutoff)` — чистка устаревших рекламных спинов ([[Рулетки]])

## Связи

- [[Интерфейсы Core (шов YG2)]], [[SavesYG]], [[YG2-адаптеры | Yg2SavesAccess]]
