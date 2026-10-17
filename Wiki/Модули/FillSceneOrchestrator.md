# FillSceneOrchestrator

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/ShapeFill/Runtime/FillSceneOrchestrator.cs` · сцена Fill

Хозяин наградной сцены: применяет тему уровня, показывает заливку, коммитит награду транзакцией, решает навигацию, дёргает рекламу и лидерборд. Один из трёх сессионных оркестраторов [[SceneNavigator]].

## Сценарий

1. `Start`: `RegisterSession(PrepareSessionAsync, StartSession, Finish, token)`
2. `PrepareSessionAsync` (под loading screen): `ApplyTheme()` — `LevelConfig.Theme.PickFillShapeTexture()` → `GridBuilder.SetShapeTexture` (нет текстуры — fail-fast) → `ShapeFillOrchestrator.Prepare()` → `CalculateResult()` → `Rewarder.ResetLevelResult()` → сейв-транзакция `CommitResult` (`CommitLevelResult(percent)`, при победе — `CommitLevelCompletion`: `CurrentLevel+1`, `MaxLevel = max(MaxLevel, next)` в [[SavesYG | ILevelStorage]]) → при победе `LeaderboardReporter.Report()` → `ConfirmSavedAsync` — переход не покажется, пока сейв не подтверждён
3. `StartSession`: музыка, `ShapeFillOrchestrator.Show(result)`, `IGameplayReporter.ReportStart()`
4. `ShapeFillOrchestrator.FillCompleted(percent)`:
   - победа (`Rewarder.IsWin`) → задержка `_winWindowDelay` (1 с) → `Win(GrantedTotal)` — слушает [[UI-фабрики | FillUIFabric]] (WinMenu)
   - иначе → `CanRescue = ShapeFillOrchestrator.CanRescue` → `Failed(GrantedTotal)` (FailMenu)
5. `RescueFill()` (кнопка спасения за рекламу): сейв-транзакция `CommitRescue` (`CommitRescueTopUp` + `CommitLevelCompletion`) → `Report()` → `ConfirmSavedAsync` → `RescueShow()` → `ReportStart()`
6. Навигация из окон — все уходы через `NavigateToAfterStop`: `Finish()` → **`AdScheduler.TryShowInterstitial()`** → переход. `LoadNextLevel`/`RestartLevel` → Game, `ExitToMenuAfterWin`/`ExitToMenu` → Menu

## Публичное API для UI

`LoadNextLevel()`, `RestartLevel()`, `ExitToMenuAfterWin()`, `ExitToMenu()`, `RescueFill()`, `CanRescueFill`, `IsFinished`, `PauseByRequest()`, `ResumeByRequest()`, события `Win(int)`, `Failed(int)`, `Finished`.

## Связи

- [[ShapeFillOrchestrator]], [[Заливка фигуры | GridBuilder]], [[Rewarder]] (+ [[Wallet]] внутри награды), [[AdScheduler]], [[LeaderboardReporter]], [[LevelConfigResolver]], [[SavesYG | ILevelStorage]], [[Pauser]], [[SceneNavigator]], [[UI-фабрики | FillUIFabric]], сейв-шов `ISaveConfirmation`/`Action<Action>` ([[Интерфейсы Core (шов YG2)]])

## Нюансы

- Прогресс уровня коммитится только внутри сейв-транзакции и только при победе/спасении — провал уровень не двигает
- `ConfirmSavedAsync` перед стартом сессии: игрок не увидит сцену с неприменённой наградой
- Интерстишл перед каждым выходом из Fill — см. [[AdScheduler]]
