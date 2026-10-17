# AdScheduler

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Ads/Runtime/AdScheduler.cs` (+ `IAdsService`, `Yg2AdsService` — см. [[YG2-адаптеры]])

Мост к `IAdsService`: превращает сырые события YG2 в надёжный rewarded-флоу. Живёт в сценах Menu/Fill; id-шники — [[Мелкие конфиги Core | YandexConfig]].

## Rewarded-механика

`ShowRewarded(rewardId, onGranted, onCompletedWithoutReward)`:
- реклама уже идёт (`IsAdShowing`) → **сразу колбэк без награды**, молча
- `granted` только если `RewardReceived` пришёл до `RewardedClosed`; ошибка показа → без награды
- `ShowDoubleReward(onGranted)` — обёртка с id `DoubleReward` ([[UI-фабрики | FillUIFabric]]); `RouletteRewardId` — для [[Рулетки | рекламных спинов]]

**TryShowInterstitial()** — прямой `ShowInterstitial()` без счётчика и капа: зовёт [[FillSceneOrchestrator]] перед каждым уходом из Fill.

## Кто вызывает

- [[FillSceneOrchestrator]]: интерстишл перед каждым уходом из Fill
- [[UI-фабрики | FillUIFabric]]: ×2 награды, спасение заливки
- [[Меню | MenuNavigation]]/[[Рулетки | RoulettePreparation]]: рекламные спины ежедневной рулетки

## Нюансы

- Частотного капа интерстишла нет — показ на каждом выходе из Fill; риск политик Яндекса держится на таймаутах плагина
- Гварды `IsPauseGame` не удалять: пауза плагина приоритетнее игровой ([[Pauser]])
