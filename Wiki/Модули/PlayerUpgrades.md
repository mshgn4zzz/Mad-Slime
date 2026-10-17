# PlayerUpgrades

**Слой:** Gameplay · **Код:** `Assets/MadSlime/Scripts/Upgrades/Runtime/PlayerUpgrades.cs` (MonoBehaviour, префаб [[ProjectLifetimeScope]])

Фасад прогрессии: переводит уровни апгрейдов из сейва (`IUpgradesStorage`) + [[UpgradesConfig]] в числовые значения, которые читают геймплей и магазин.

## Производные значения

| Метод | Формула | Кто читает |
|---|---|---|
| `GetSpeedMultiplier()` | по Speed | [[Движение | Movement]] |
| `GetMassMultiplier()` | по Appetite | [[Рост игрока | MassGainer]] (масса за любой предмет) |
| `GetQuotaMassMultiplier()` | Appetite × Taste вместе | MassGainer (масса за квотный предмет) |
| `GetForeignFillMultiplier()` | по Metabolism | [[Квота | QuotaCounter]] (вес «заливки» не-квотой) |
| `HasSmell` / `HasAdrenaline` / `HasAmbitions` | `IsPerkPurchased` | подсветка квоты, [[AdrenalineBoost]], [[Рост игрока | CollectAvailability]] |
| `GetPerk(PerkType)` | запись перка (Value и т.п.) | AdrenalineBoost (множитель) |

## Магазин

`GetLevel`, `IsMaxed`, `GetNextCost` (base + step × level), `GetMaxSteps`, `GetNextStepValue`, `GetTotalValue`, `PurchaseStepped` (уровень+1 ⇒ сейв-транзакция), `GetPerkCost`, `PurchasePerk`. Для UI: `Config`, `UpgradeEntries`, `PerkEntries` (грид вкладки улучшений — [[Магазин | ShopPanel]]/[[Меню | MenuNavigation]]).

## Связи

- [[UpgradesConfig]], сейв-шов `IUpgradesStorage` ([[Интерфейсы Core (шов YG2)]])

## Нюансы

- Семантика «valuePerStep» у каждого типа своя (доля/множитель) — интерпретация живёт в этом классе
- Двойная роль: рантайм-множители + логика покупки — осознанная связка вокруг одного сейв-поля уровней
