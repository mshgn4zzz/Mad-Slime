# Pauser

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Game/Pause/Pauser.cs`

Единственный писатель `Time.timeScale`. Регистрируется **в каждой сцене свой** (счётчик обнуляется сменой сцены, в Menu ещё и `ResetToPlay` в `RegisterBuildCallback`).

## Как работает

- `RequestPause()` → счётчик++, `RequestResume()` → декремент (не ниже 0). `IsPaused = счётчик > 0 || платформенная пауза`
- **Платформенная пауза:** `Update` поллит `IAdsService.IsPauseGame` — пока Яндекс показывает рекламу, игра заморожена независимо от окон
- **Пауза видимости:** `IGameVisibility.Hidden/Shown` (сворачивание вкладки) → пауза + `AudioListener.pause = true` + `ICollectedItemsStorage.CommitCollectedItems()` (прогресс фиксируется до потери фокуса); при возврате — обратная
- `ResetToPlay()` — обнуляет счётчик (сохраняя visibility-паузу) — его зовут оркестраторы перед переходами и Menu-скоуп на старте
- `StateChanged` — на каждое применение; по нему [[GameplayOrchestrator]] включает/выключает детекторы и управление

Игровая пауза окон звук НЕ глушит — `AudioListener.pause` включается только на платформенных паузах.

## Кто ставит/снимает

- [[GameplayOrchestrator]]: пауза до первого ввода и после конца раунда, resume на старте, `PauseByRequest`/`ResumeByRequest` из PauseMenu
- [[Окна]]: любое окно — pause на открытии, resume на закрытии
- [[FillSceneOrchestrator]]: `PauseByRequest`/`ResumeByRequest` + переходы через `ResetToPlay`

## Нюансы

- Пауза только через timeScale: анимации на unscaled time и Realtime-задержки продолжают жить — каждый новый tween надо не забыть про unscaled time
- Плагинная автопауза на Yandex-платформе выпилена — рассчитывать только на свою подписку `IGameVisibility` (см. [[AI_RULES | правила Яндекс Игр]])
