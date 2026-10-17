# LeaderboardReporter

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Leaderboards/Runtime/LeaderboardReporter.cs` · сцена Fill

Запись очков в Яндекс-лидерборд. Система целиком — [[Лидерборды]].

## Механика

- `Report()`: не авторизован → тихий early-return; авторизован → `ILeaderboardService.SetScore(YandexConfig.LeaderboardName, CollectedItemsCount)` — очки = **сколько предметов съедено за всё время** ([[SavesYG | CollectedItemsCount]])
- Вызывается из [[FillSceneOrchestrator]]: при победе в подготовке сцены и при спасении — всегда после сейв-транзакции и `ConfirmSavedAsync`

## Связи

- [[Лидерборды | CollectedItemsCounter, Yg2LeaderboardService]], [[Интерфейсы Core (шов YG2) | ILeaderboardService]], [[Мелкие конфиги Core | YandexConfig]]
