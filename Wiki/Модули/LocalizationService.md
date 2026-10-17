# LocalizationService

**Слой:** Core · **Код:** `Assets/MadSlime/Scripts/Localization/Runtime/LocalizationService.cs` (+ `Localization.cs`) · префаб [[ProjectLifetimeScope]]

Синхронизирует статическую локализацию с сейвом и языком платформы. Детали UI — [[Локализация UI | Локализация]].

## Статический Localization

- Языки: ru / en / tr (константы), цикл кнопкой `CycleLanguage` ([[Локализация UI | LanguageSwitcher]])
- `Get(key)` — из `LocalizationTable`; не инициализирован → исключение
- Событие `LanguageChanged` — на него сидит весь UI (LocalizedText и пр.)

## LocalizationService (MonoBehaviour)

- `OnEnable`: ждёт готовность сейва (`ISavesReadiness`) → `Localization.Initialize(table, сейв)`; подписки: `LanguageChanged` (persist — только ручная смена), `ILanguageProvider.LanguageSwitched`
- **Приоритет:** сейв непуст → язык игрока; сейв пуст → язык Яндекса с `_suppressPersist` (авто-выбор **не** пишется в сейв)
- В сейв язык пишется **только при ручной смене** игроком
