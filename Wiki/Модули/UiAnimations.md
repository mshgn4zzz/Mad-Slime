# UiAnimations

**Слой:** UI · **Код:** `Assets/MadSlime/Scripts/UI/Animations/` (AnimationScheduler, ReactiveUIView, TweenAnimatorBase, UiFadeAnimator, UiMoveAnimator, UiScaleAnimator, UiEnableTarget)

Мини-фреймворк появления/исчезновения UI на DOTween.

## Состав

- **ReactiveUIView** — сердце окна ([[Окна | BaseWindow]]): `Enable()`/`Disable()` запускают каскад аниматоров (fade/move/scale), событие `DisableCompleted` — окно уничтожается только после него
- **UiFadeAnimator / UiMoveAnimator / UiScaleAnimator** (общая база `TweenAnimatorBase`) — шаги каскада, unscaled time
- **AnimationScheduler** — режиссёр каскадов: `Play(animations, delays)` / `Cancel`; два инстанса в HUD Game — «старт сцены» и «старт геймплея» ([[HUD | GameplayHud]] регистрирует `UiEnableTarget`-цели: при подготовке сцены HUD скрыт, при первом вводе ([[GameplayOrchestrator]] `GameplayStarted`) — каскад появления)
- **UiEnableTarget** — сериализуемая пара «цель + задержка»

## Связи

- [[HUD]], [[Окна]], [[GameplayOrchestrator]]
