# Мелкие UI-компоненты

**Слой:** UI · **Код:** разрозненные мелочи

- **LookAtCamera** (`Camera/Billboards/`): в `LateUpdate` копирует поворот камеры — билборды (тарелки квоты над предметами)
- **UiLayer** (`UI/Spawning/`): полосы sortingOrder попапной системы ([[UiSpawner]])
- **ShopBalanceView** (`Shop/UI`): баланс монет ([[Wallet | BalanceChanged]])
- **RouletteSectorIcon / RouletteSoundFeedback** (`Roulette/UI`): иконка сектора и щелчки колеса ([[Рулетки]])

## Связи

- LookAtCamera: [[HUD]]/мировые плашки + [[Камера]]
