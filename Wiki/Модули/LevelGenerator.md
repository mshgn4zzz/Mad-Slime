# LevelGenerator

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Levels/Runtime/` — `LevelGenerator.cs`, `LevelPlacementPlanner.cs`, `ZoneLayoutPlanner.cs`, `LevelItemsSpawner.cs`, `PropVariantsAssigner.cs`, `LevelPlacement.cs` (+ Editor: `Levels/Editor/LayoutPreview*`, гизмо раскладок)

Строит уровень под loading screen: тема пола → выбор раскладки → назначение вариантов пропов → план плейсментов по тирам → спавн → квота. Отдельных ItemPool больше нет — предметы живут и умирают через `LevelItemsSpawner` + `ItemFactory` ([[Детекция и сбор]]).

## Пайплайн GenerateAsync(progress, token)

1. `LevelConfigResolver.GetConfigFor(CurrentLevel)`
2. **PickLayout** ([[LevelGenerator | LevelPlacementPlanner]]): случайный `LayoutSet` из LayoutsLibrary; пустая раскладка — исключение
3. **PropVariantsAssigner.Assign(config)** — каждому пропу PropSet назначается `ItemDefinition` (вариант определяет тир/иконку)
4. **Plan** — плейсменты: проход по тирам от `MaxTier` вниз; на каждый тир собирается пул пропов этого тира, зоны с подходящим тир-диапазоном раскладываются через `ZoneLayoutPlanner.CollectPlacements` (зеркалирование X/Z независимо, `SingleType`-зоны — один предмет на зону). Предметы `Large` и выше становятся препятствиями: последующие плейсменты проверяют `IsSpaceFree` против них. Позиции клампятся внутрь пола
5. `Clamper.SetBounds(floorBounds)` — игрок получает границы пола здесь
6. Тема: `sharedMaterial` пола из LevelTheme
7. **SpawnAsync** ([[LevelGenerator | LevelItemsSpawner]]): фабрика создаёт предметы, `Attach(ItemCollector)` — собранное возвращается спавнеру; прогресс уходит в полосу загрузки [[SceneNavigator]]
8. `Physics.SyncTransforms()` — оверлап-детекторы сразу видят предметы
9. [[Квота | QuotaGenerator.Generate]] по факту спавна → `QuotaBoard.Reset`

## ZoneLayoutPlanner — формы зон

`SpawnShape`: Grid, Circle, Scatter, CircleGrid (кольца), SquareRing (периметр), SquareScatter. Spacing: авто (шаг = радиус крупнейшего в зоне × `AutoSpacingFactor` 2.2) или ручной. Fit-culling: в Grid предмет с радиусом больше полу-шага не размещается; Scatter отбрасывает пропы крупнее радиуса зоны. Тот же класс используют редакторские гизмо раскладок (`CollectPlacements` общий для рантайма и превью).

## Связи

- [[Конфиги уровней]], [[TierTable]], [[LevelConfigResolver]], [[Движение | Clamper]], [[Квота]], [[Детекция и сбор | ItemCollector, ItemFactory]]

## Нюансы

- Генерация идёт в `PrepareSessionAsync` [[GameplayOrchestrator]] — не в Awake: кривой ассет роняет сессию под loading screen, прогресс виден
- `LevelPlayModeCheck`/`*LevelSetup` в `Levels/Editor` — editor-проверки раскладок
