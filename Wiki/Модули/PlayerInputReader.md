# PlayerInputReader

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Input/Runtime/` (PlayerInputReader, TouchJoystick, PlayerInputActions — автоген)

Обёртка New Input System для движения слайма.

## Механика

- `Awake` создаёт `PlayerInputActions` (карта Player, action Move: WASD + геймпад-стик); `OnEnable` подписывается и включает карту; `OnDestroy` — Dispose
- `MoveInput: Vector2` — читается [[Движение | Movement]] каждый кадр
- **Событие `MovementKeyPressed`** — фронт «из нуля в неноль»; стартовый триггер [[GameplayOrchestrator]] (раунд заморожен до первого движения)
- **TouchJoystick** (`OnScreenControl`): виртуальный стик — тап ставит базу в точку касания, drag двигает хэндл, значение уходит в `<Gamepad>/leftStick`; визуал скрыт до касания; активен только при наличии тача
- Drag сбрасывается в `OnDisable` — смена ориентации не может заклинить ввод

## Связи

- [[Движение]] (MoveInput), [[GameplayOrchestrator]] (MovementKeyPressed)
