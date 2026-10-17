# SavesYG

**Слой:** Saves · **Код:** `Assets/MadSlime/Scripts/Saves/Data/SavesYG.cs` (partial; вторая часть — в плагине YG2, `SavesYG2.cs`, только `idSave`)

Схема сейва игрока. Сериализация YG2 — JsonUtility: **имена полей = JSON-ключи живых сейвов**. Переименование = потеря прогресса у всех игроков. Доступ из игры — только через `Yg2SavesAccess` → [[Saver]] (узкие хранилища; фасада PlayerProgress больше нет).

## Поля (JSON-ключи)

| Ключ | Тип | Дефолт | Что хранит |
|---|---|---|---|
| `CurrentLevel` / `MaxLevel` | int | 1 / 1 | текущий и максимальный уровень |
| `CollectedItemsCount` | int | 0 | съедено предметов за всё время (очки лидерборда) |
| `Language` | string | "" | выбранный язык |
| `musicVolume` / `sfxVolume` | float | 0.5 / 0.35 | громкость (**ключи с маленькой буквы — легаси**) |
| `Balance` | int | 250 | монеты |
| `SelectedSkinId` | string | "Slime" | надетый скин (строковый id; миграция с enum-эры) |
| `_openSkinIds` | List\<string\> | {Slime} | открытые скины (**ключ с подчёркиванием — легаси**) |
| `_showcaseSkinIds` | List\<string\> | {} | витрина скин-рулетки ([[Рулетки \| SkinShowcase]]) |
| `SpeedLevel`…`MetabolismLevel` | int ×4 | 0 | уровни апгрейдов |
| `PurchasedPerks` | List\<PerkType\> | {} | купленные перки |
| `LastFreeSpinUnixTime` | long | 0 | фри-спин рулетки |
| `RouletteAdSpinTimes` | List\<long\> | {} | таймстемпы рекламных спинов |
| `SkinSpinCount` | int | 0 | счётчик круток (растущая цена) |

## Связи

- Читается: [[YG2-адаптеры | Yg2SavesAccess]] → [[Saver]]
- Сброс в редакторе: `Saves/Editor/PlayerDataResetTool` (правит `SavesEditorYG2.json`)
- Новое поле добавляется атомарно в 4 места: `SavesYG`, `I*Storage`, `Saver`, потребитель ([[AI_RULES | правило сейв-контракта]])

## Нюансы

- Стилевые исключения (`_openSkinIds`, `musicVolume`) — намеренно заморожены, трогать нельзя
- `Save()` no-op до инициализации SDK; [[Saver | транзакции]] дают откат при падении операции
