# GameplayOrchestrator

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Game/Runtime/GameplayOrchestrator.cs` · сцена Game

Хозяин раунда «ешь предметы»: заморозка до первого ввода → отсчёт таймера и квоты → финальный отсчёт → конец → переход в Fill. Один из трёх сессионных оркестраторов [[SceneNavigator]].

## FSM

`Preparing → WaitingForStart → Running → Finished`, переходы только вперёд через `TryTransitTo` (назад и в сторону — `false`).

## Сценарий раунда

1. `Start`: `RegisterSession(PrepareSessionAsync, RunSession, FinishSession, token)`
2. `PrepareSessionAsync` (под loading screen): инициализация игрока (`PlayerTier.Initialize`, `PlayerScaler.ApplyInitialScale`, `Movement.Initialize`, `SmellAbilityActivator.Apply`), гашение финальных эффектов → `GetConfigFor(CurrentLevel)` → `Timer.Setup(duration)` → `LevelGenerator.GenerateAsync(ReportPreparationProgress)` (прогресс — в полосу загрузки) → `QuotaCounter.ResetSession()` → `RequestPause()` — раунд стартует замороженным
3. `RunSession`: музыка, `QuotaCounter.Attach(ItemCollector)`, `WaitingForStart`, ввод включён — HUD жив, раунд ждёт первого движения
4. `PlayerInputReader.MovementKeyPressed` → `Running`: `RequestResume`, `Timer.StartCount()`, `IGameplayReporter.ReportStart()` (аналитика), событие `GameplayStarted`
5. `Timer.Ticked(remaining)`: `CrawlAnimator.SetUrgency` (разгон к 1 в финальном отсчёте); на пороге `_finalCountdownSeconds` (SerializeField, 20 с) — `AdrenalineBoost.Enable()`, `FinalCountdownVignette.StartPulse()`, `TimerTickSound.StartTicking()`
6. `QuotaCounter.QuotaCompleted` или `Timer.Finished` → `Finished`: стоп таймера/коллектора/`GrowthAnimator`, `ReportStop()`, пауза, эффекты финала выключены → задержка `_sessionEndDelay` (1 с, Realtime) → `LoadFillAsync()`
7. `ExitToMenu()` — из PauseMenu: `FinishSession` + `ResetToPlay` + Menu

## Публичное API для UI

`ExitToMenu()`, `PauseByRequest()`, `ResumeByRequest()` (кнопки PauseMenu), события `SceneSessionPrepared/Started`, `GameplayStarted`, `SessionFinished`.

## Связи

- [[Timer]], [[Квота | QuotaCounter]]/QuotaBoard, [[LevelConfigResolver]], [[LevelGenerator]], [[Детекция и сбор | ItemCollector]], [[PlayerTier]], [[Рост игрока | PlayerScaler]], [[Движение | Movement]], [[Движение | CrawlAnimator]], [[Рост игрока | GrowthAnimator]], [[Квота | SmellAbilityActivator]], детекторы ([[Детекция и сбор]]), [[AdrenalineBoost]], FinalCountdownVignette, TimerTickSound, [[Интерфейсы Core (шов YG2) | IGameplayReporter]], [[Pauser]], [[SceneNavigator]], [[UI-фабрики | GameplayUIFabric]] (PauseMenu)

## Нюансы

- Таймер тикает только при `Time.timeScale > 0` — пауза ([[Pauser]]) автоматически останавливает отсчёт
- Задержка перед Fill — Realtime: не растягивается рекламной паузой
- `OnDisable` снимает подписки и завершает сессию — корректно переживает выгрузку сцены переходом
