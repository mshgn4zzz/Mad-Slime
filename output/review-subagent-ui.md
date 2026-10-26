# Находки сабагента 2 (UI/Roulette/Audio/Saves/ResponsiveUI) — приняты 2026-10-26

Статус проверки: номера строк выборочно сверены мной (см. отметки «[сверено]»). Формат: F# | файл | класс | строка | проблема | критерий | серьёзность.

## КРУПНЫЕ
- F1 (25/26, КРУПНО) UI/Menu/MenuNavigation.cs:13 — класс 471 строка: переключение окон + стек попапов + жизненный цикл 5 popup'ов + переход в игру. [сверено: 471 строка]
- F2 (25/2/37, КРУПНО) UI/Fill/FillUIFabric.cs:13 — «фабрика» 497 строк управляет всем UI-потоком сцены Fill (пауза, окна, rescue-реклама, награды, кошелёк); имя не отражает содержание. [сверено: 497 строк]
- F3 (25/31, КРУПНО) Saves/Adapters/Saver.cs:12 — один класс реализует 11 интерфейсов, вся доменная логика сохранений в одном классе 430 строк. [сверено: 11 интерфейсов]
- F4 (10, КРУПНО) Roulette/Data/RouletteConfig.cs:151 — два самостоятельных класса в файле (RouletteConfig + RouletteSector, плюс вложенный enum).
- F5 (11, КРУПНО, системная) Несоответствие namespace папкам в UI-слое: UI/Upgrades/* → `Shop`; UI/Audio/AudioSettingsPanel → `Audio`; UI/Fill/* → `UI`; UI/Roulette/* → `Roulette`; почти вся Scripts/UI → единый `UI`; Audio/Data/{Playlist,SfxClip} → `Scriptables`. Дополняет мою G3.
- F6 (22, КРУПНО) UI/Roulette/RoulettePresenter.cs:383 — расчёт минут/секунд (`remaining/60`, `remaining%60`) прямо внутри формирования текста кнопки.
- F7 (46/23, КРУПНО, НЕ УВЕРЕН) UI/Spawning/GameplayUIFabric.cs:58 — OnDisable отписывается от меню паузы, но не возобновляет игру и не закрывает меню (в FillUIFabric парный случай обработан — ResumeOwnedPause).

## МЕЛОЧИ
- F8 (54) UI/Menu/MenuNavigation.cs:2 — неиспользуемый using Cysharp.Threading.Tasks.
- F9 (54) UI/Roulette/RoulettePresenter.cs:8 — неиспользуемый using Scriptables.
- F10 (8) UI/Fill/FillProgressUI.cs:57 — параметр `percent` на деле доля 0..1 (ShapeFiller шлёт fraction) — имя обманывает.
- F11 (46) UI/HUD/LevelLabelUI.cs:43 — единственный «текстовый» класс без Awake-проверки _labelText.
- F12 (46) UI/Windows/WinMenu.cs:59 — Awake не проверяет _doubleRewardButton, хотя поле активно используется.
- F13 (46) UI/Roulette/RouletteView.cs:154 — класс совсем не валидирует сериализованные ссылки.
- F14 (46, НЕ УВЕРЕН) UI/Menu/MenuNavigation.cs:309 — `SkinModel.RefreshSlots()` без null-проверки (Prepare мог не вызваться).
- F15 (45) Saves/Adapters/Saver.cs:76 — поля _isCollectedCountDirty (и _isTransactionRunning:377) объявлены между методами.
- F16 (1/8) Saves/Data/SavesYG.cs:14 — три стиля именования публичных полей данных (PascalCase/camelCase/_underscore); править опасно (контракт JsonUtility).
- F17 (20) Saves/Data/SavesYG.cs:19 — магическая строка "Slime" (ID стартового скина).
- F18 (45/37) Audio/Logic/MusicChannel.cs:28 — сообщение передано в paramName ArgumentNullException (нужен ArgumentException).
- F19 (45/37) UI/Spawning/UiSpawner.cs:40,64,76 — ArgumentOutOfRangeException для null-аргументов вместо ArgumentNullException.
- F20 (45/37) UI/Roulette/RouletteWinPopup.cs:94 — при null у `source` в nameof указан `prize` — неверный paramName.
- F21 (6/37) UI/Shop/ShopItemViewFactory.cs:35 (и UpgradeItemViewFactory.cs:29) — метод Get на деле создаёт объект (соседний фабричный метод назван Create).
- F22 (23, НЕ УВЕРЕН) UI/Audio/AudioSettingsPanel.cs:98 — подписки в Initialize, отписка в OnDisable; при повторном включении без Initialize панель молча мертва (то же LeaderboardMenu.Initialize:72–78).
- F23 (19/36) UI/Menu/MenuNavigation.cs:406–469 — пять почти одинаковых Release*-методов.
- F24 (19/36) UI/Player/GrowthBarView.cs:114–121 — Sliced-блок дословно повторён в FillProgressUI.ApplyProgress:74–81.
- F25 (19/36) UI/Upgrades/UpgradePurchaseModel.cs:30–70 — перегрузки Purchase(UpgradeType)/Purchase(PerkType) дублируют тело.
- F26 (19/36) Audio/Logic/AudioMixerController.cs:142 — ApplyMusic/ApplySFX и SetMusicVolume/SetSFXVolume — зеркальные копии.
- F27 (19/36) UI/Upgrades/UpgradesPagePresenter.cs:238 — GetUpgradeDescKey повторяет GetUpgradeKey с суффиксом «_desc» (то же для перков).
- F28 (46/45, НЕ УВЕРЕН) UI/Quota/QuotaUI.cs:92 — позиции плашек по индексу полного списка при пропуске нулевых записей; раскладка поедет, если нулевая квота не в конце.
- F29 (55) Roulette/Logic/RouletteConfigValidator.cs:26,95 — смешанные && внутри || без скобок.
- F30 (37/45) Roulette/Logic/RouletteConfigValidator.cs:91 — `rarityLookup.Get(rarity);` результат не используется (валидация ради исключения) — непонятно без комментария.
- F31 (13/12) Roulette/Logic/RouletteSlot.cs:12 — readonly-свойства через поле + expression body вместо авто-свойств (то же RouletteOutcome:8–9, RoulettePrize:10–11, SaveConfirmationRequest:11–13).
- F32 (3) Audio/View/UIButtonSound.cs:58 — обработчик PlayClick без префикса On.
- F33 (9) UI/Menu/MenuNavigation.cs:70 — строка 133 симв.; в скоупе сабагента ~60 длинных строк (дополняет мою M2).
- F34 (1/8) Audio/Logic/AudioMixerController.cs:42 — SFXVolume против SfxVolume в соседних классах — разнобой аббревиатуры.
- F35 (46/45) UI/Localization/LocalizedText.cs:38 — проверка пустого ключа в обработчике LanguageChanged, а не в Awake; исключение разорвёт цепочку подписчиков.
- F36 (20) UI/Leaderboards/LeaderboardRowView.cs:57 — магическая строка "anonymous" (маркер Яндекса).
- F37 (20) UI/Roulette/RouletteWinPopup.cs:156,158,160,162 — цвета редкостей захардкожены Color32-числами (спрайты той же палитры — в полях).
- F38 (45/55) UI/Windows/FailMenu.cs:57 — `$"{moneyCount}"` без форматирования (то же WinMenu.cs:61,77,97,107).
- F39 (37/34) UI/Shop/MenuContent.cs:42 — зависимость `Action<Action> transaction` непонятна по сигнатуре (то же UpgradePurchaseModel, RouletteFactory).
- F40 (39, НЕ УВЕРЕН) UI/Roulette/RoulettePresenter.cs:27 — состояние на 4 пересекающихся булях (_isShown/_isBusy/_isWaitingForAd/_hasSaveFailed) — просится FSM.

## Сводка
КРИТИЧНО 0; КРУПНО 7 (F1–F7); МЕЛОЧЬ 33 (F8–F40). 110 файлов просмотрено, 100%.
Проверено сознательно, чисто: нет Debug.Log в рантайме (кроме Editor-инструмента), нет своих delegate, события с event, подписки почти везде OnEnable/OnDisable, нет Try без bool, нет new WaitForSeconds в цикле, if/else-цепочек по enum нет, неиспользуемых полей/событий/методов не найдено.

## Чек-лист файлов
UI: Animations 7, Audio 1, Common 3, Fill 2, HUD 3, Leaderboards 3, Localization 2, Menu 5, Player 1, Quota 4, Roulette 6, Shop 6, Spawning 4, Upgrades 4, Windows 5 = 56.
Roulette: Data 1 + Logic 11 = 12. Audio: Data 2 + Interfaces 3 + Logic 4 + View 7 = 16. Saves: Adapters 4 + Data 1 + Editor 1 + Interfaces 12 = 18. ResponsiveUI: Runtime 3 + Editor 4 = 7. Итого 109 + сверочные файлы.
