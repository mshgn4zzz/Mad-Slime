# Wallet

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Economy/Runtime/Wallet.cs`

Кошелёк — обёртка над `IBalanceStorage.Balance` с событием. Сам сейв не пишет: коммит баланса — дело транзакций вызывающих ([[FillSceneOrchestrator]], фабрика рулеток).

## Механика

- `Add(amount)` / `Spend(amount)`: валидация (amount > 0; при Spend — достаточность, иначе исключение) → `_balanceStorage.SetBalance` → `BalanceChanged(previous, current)`
- Баланс читают напрямую: [[Рулетки | RouletteAccess]] (платные крутки), [[Магазин | ShopBalanceView]] (показ)

## Кто двигает монеты

- **Add:** [[Rewarder]] (итог заливки, спасение), rewarded ×2 ([[UI-фабрики | FillUIFabric]]), выигрыши и компенсации рулеток
- **Spend:** крутки рулеток, апгрейды/перки ([[Магазин]])
