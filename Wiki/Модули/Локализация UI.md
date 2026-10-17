# Локализация

**Слой:** UI/Core · **Код:** `Assets/MadSlime/Scripts/Localization/` — Runtime (Localization, LocalizationService), UI (LocalizedText, LanguageSwitcher), Interfaces/Adapters (ILanguageProvider → Yg2LanguageProvider); данные — `LocalizationTable` ([[Мелкие конфиги Core]])

## Как работает

- **Localization** (static): `Initialize(table, savedLanguage)` (зовёт [[LocalizationService]]), `Get(key)` с фолбэками tr→en→ru→ключ, `SetLanguage`/`CycleLanguage`, событие `LanguageChanged`
- **LocalizedText** (`Localization/UI`, `[RequireComponent(TMP_Text)]`): ключ в поле, `OnEnable` применяет перевод и подписывается на `LanguageChanged`; пустой ключ → throw. Весь статичный текст UI живёт через него
- **LanguageSwitcher**: кнопка циклической смены + метка текущего языка
- **ILanguageProvider** (`Yg2LanguageProvider`) — язык платформы ([[Интерфейсы Core (шов YG2)]])
- Ключи раскладываются по таблице `LocalizationTable` (key → ru/en/tr)

## Где ключи собираются в коде

- Динамические строки: `tier_{tier}` — GrowthBarView; `upgrade_*`/`perk_*` — UpgradeItemView; `rarity_*` — карточки скинов; `roulette_*`, `shop_*`, `menu_*`, `leaderboard_*`

## Связи

- [[LocalizationService]] (источник языка и событий), все UI-доки; правило: хардкод строк в сценах/префабах/коде запрещён ([[AI_RULES | правила Яндекс Игр]])
