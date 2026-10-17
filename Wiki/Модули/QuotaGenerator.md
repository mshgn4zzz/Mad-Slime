# QuotaGenerator

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Quota/Runtime/QuotaGenerator.cs` (plain-класс)

Превращает «что заспавнено» в выполнимую квоту уровня. Детали алгоритма и параметры — в [[Квота]].

## Контракт

- Вход: словарь «ItemDefinition → заспавнено» от [[LevelGenerator]] + параметры [[Конфиги уровней | LevelConfig]]
- Выход: `List<QuotaEntry>` → `QuotaBoard.Reset()` там же в `GenerateAsync`
- Невыполнимая конфигурация (типов меньше `QuotaTypesMin`) — исключение с именем ассета, не тихий деград
