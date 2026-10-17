# LevelConfigResolver

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Levels/Runtime/LevelConfigResolver.cs` (plain-класс, синглтон [[ProjectLifetimeScope]])

Маппинг «номер уровня → LevelConfig» через [[Конфиги уровней | LevelsCatalog]].

## Логика

- Диапазон `LevelRange` (FromLevel..ToLevel) → конфиг; за последним диапазоном — конфиг последнего (эндгейм бесконечен)
- Каталог пуст — исключение
- **Playable-проверка:** конфиг существует, PropSet назначен и содержит запечённые варианты — иначе исключение с описанием проблемы и подсказкой «run Mad Slime → Prop Factory»
