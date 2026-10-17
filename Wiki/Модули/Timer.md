# Timer

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Game/Timer/Timer.cs`

Таймер раунда на UniTask.

## API и механика

- `Setup(duration)` — только > 0, иначе исключение; `StartCount()` — до Setup или повторно — исключение; `Stop()` — отмена через CTS, событие `Stopped`
- `RunTimerAsync`: `UniTask.Yield(Update)` в цикле; **тик только при `Time.timeScale > 0`** — под паузой время не течёт и `Ticked` не стреляет
- События: `Ticked(float remainingSeconds)` — каждый кадр; `Finished` — один раз при 0; `Stopped` — при ручной остановке
- CTS линкуется с `GetCancellationTokenOnDestroy()` — смерть объекта останавливает счёт

## Связи

- Хозяин: [[GameplayOrchestrator]] (Setup → StartCount на первом вводе → Stop на финише)
- `Ticked`: [[GameplayOrchestrator]] (urgency + запуск финального отсчёта), [[HUD | TimerUI]], TimerTickSound ([[Аудио-подсистема]])
- `Finished` → конец раунда → Fill

## Нюансы

- `Ticked` стреляет каждый кадр раунда — для WebGL приемлемо, осознанный cost
- Про финальный отсчёт (последние N секунд) Timer не знает: порог и эффекты живут в [[GameplayOrchestrator]]
