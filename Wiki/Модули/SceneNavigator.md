# SceneNavigator

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Game/Runtime/SceneNavigator.cs` · root-скоуп ([[ProjectLifetimeScope]])

Единственная точка смены сцен. Никто не имеет права дергать `SceneManager.LoadScene` — только `LoadMenuAsync`/`LoadGameAsync`/`LoadFillAsync` (внутри один `LoadAsync(SceneId)`). Сцены: `SceneId` = Menu/Game/Fill (enum `SceneId` в `Game/Data`); отдельной сцены Shop больше нет — магазин живёт окнами в Menu.

## Session-протокол

Сценический оркестратор ([[GameplayOrchestrator]], [[Меню | MenuOrchestrator]], [[FillSceneOrchestrator]]) в `Start` вызывает `RegisterSession(preparation, sessionStart, sessionFinish, token)`:

- вне перехода — колбэки запускаются сразу (`StartInitialSessionAsync`): loading screen → ожидание готовности сейва (`ISavesReadiness.IsReady`) → `SkinInventory.Initialize()` → `EnsureInitialized()` → `preparation(token)` → `CompleteAsync` → `sessionStart()`
- во время перехода — колбэки складываются в pending и запускаются новой сценой

`UnregisterSession(sessionStart)` — симметричная очистка в `OnDisable` оркестратора.

## TransitionAsync — что происходит

1. Loading screen: инстанс префаба с `LoadingScreenView` (показывает выбранный скин игрока, прогресс из `ReportPreparationProgress` — его тянет генерация уровня)
2. `sessionFinish()` уходящей сцены, у её корней выключаются `EventSystem`/`Canvas`, перед загрузкой входящей — `AudioListener` (звук живёт без разрыва)
3. Аддитивная загрузка с `allowSceneActivation = false`, ожидание прогресса до 0.9, активация, `SetActiveScene`
4. Ожидание регистрации входящей сессии → `preparation(token)` → `CompleteAsync`
5. Выгрузка исходящей сцены → восстановление `EventSystem`, если он остался только в умершей сцене → `sessionStart()` входящей → скрыть loading
6. Обновление `PreviousSceneId`/`CurrentSceneId`

## API

- `EnsureInitialized()` — лениво резолвит `SceneId` по открытым сценам; неизвестная сцена — `InvalidOperationException`. Вызывается из `RegisterBuildCallback` сценических скоупов (Menu, Game) и перед каждым переходом
- `IsTransitioning` — guard, который проверяют все вызывающие перед навигацией
- `TryMarkDailyShown()` — одноразовый показ ежедневной рулетки за сессию ([[Меню | MenuNavigation]])
- `ReportPreparationProgress(float)` — прокидывает прогресс в `LoadingScreenView`

## Связи

- Владелец: [[ProjectLifetimeScope]] (компонент на ProjectScope)
- Вызывают: [[Меню | MenuOrchestrator]] → [[Меню | MenuNavigation]], [[GameplayOrchestrator]], [[FillSceneOrchestrator]]
- Время не трогает: перед переходами оркестраторы зовут `Pauser.ResetToPlay()` ([[Pauser]])

## Нюансы

- Повторный `LoadAsync` во время перехода молча выходит (early-return) — потерянное нажатие без лога, вызывающие страхуются проверкой `IsTransitioning`
- Loading screen инстансится от поля `_loadingScreenPrefab` на SceneNavigator; префаб обязан содержать `LoadingScreenView` (fail-fast)
- Сессия стартует не раньше готовности сейва — [[SavesYG]]/[[YG2-адаптеры | Yg2SavesAccess]]
