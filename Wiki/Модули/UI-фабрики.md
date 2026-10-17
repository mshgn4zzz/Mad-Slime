# UI-фабрики и карточки

**Слой:** UI · **Код:** `Assets/MadSlime/Scripts/UI/Spawning/GameplayUIFabric.cs`, `ShapeFill/UI/FillUIFabric.cs`, карточки — `Upgrades/UI/`, `Shop/UI/`

Спавн окон идёт через [[UiSpawner]]; фабрики — точки привязки кнопок и ad-флоу сцены.

## GameplayUIFabric (сцена Game)

- Кнопка паузы → `Spawn(_pauseMenuPrefab, Popup)` + `Initialize(true, menuAction: orchestrator.ExitToMenu)`

## FillUIFabric (сцена Fill) — фабрика исходов

- Подписки: `FillSceneOrchestrator.Win` / `Failed` (`Action<int>` — сумма награды)
- **Победа** → WinMenu(reward, LoadNextLevel, `RequestDoubleReward`, ExitToMenuAfterWin). Двойная награда: `AdScheduler.ShowDoubleReward` → по granted `Wallet.Add`
- **Провал** → FailMenu(..., rescue: rewarded → granted: `FailMenu.Dismiss()` + `FillSceneOrchestrator.RescueFill()`; rejected: кнопка возвращается)
- Плюс pause-кнопка как в Game

## Карточные фабрики и MVP страниц

- **ShopItemViewFactory** (`Shop/UI`) — карточки скинов для [[Магазин | SkinsPagePresenter]]; **SkinRarityGroupView** — группы редкостей
- **UpgradeItemViewFactory + UpgradeItemView** (`Upgrades/UI`) — карточки апгрейдов/перков; **UpgradesPagePresenter + UpgradePurchaseModel** — MVP окна улучшений: `CanPurchase`/`Purchase` через [[PlayerUpgrades]] + [[Wallet]] внутри сейв-транзакции

## Связи

- [[UiSpawner]], [[Окна]], [[GameplayOrchestrator]], [[FillSceneOrchestrator]], [[Wallet]], [[AdScheduler]], [[Магазин]]
