# Потенциальные места для FSM (ревью 28.10.2026)

Проверка критерия №39 «В случаях, когда у объекта сменяются состояния, применяется паттерн Final State Machine». Код пройден двумя независимыми проходами — основным и одним сабагентом — с последующим сравнением итогов. Каждый проход шёл по всему `Assets/MadSlime/Scripts/**/*.cs`: grep по bool-флагам, enum-состояниям, switch/if-цепочкам, затем подозрительные классы прочитаны целиком. Ниже — классы, где объект реально меняет состояния вручную (флаги/enum + присваивания + ветвления), но FSM-паттерн проекта (`BaseStateMachine` + `IState`) ещё не применяется. Только классы и причины, без предложений по рефакторингу.

Классы, уже использующие FSM-инфраструктуру, не перечислены: `BaseStateMachine`, `SceneStateMachine`/`SceneState` + сценовые состояния, `GameplayStateMachine` + `WaitingForStartState`/`PlayingState`/`CompletedState`, `FillStateMachine` + `FillingState`/`RescuingPrepareState`/`ResultState`, `MenuStateMachine` + `MenuRunningState`/`MenuPopupInputState`.

---


## Актуальный статус после перехода на FSM — 28.10.2026

Ниже сохранён исторический срез до изменений. Перечисленные находки перенесены на существующую `BaseStateMachine<TState>` и `IState`; сама базовая реализация при миграции не изменялась.

| Исходный владелец | Текущая реализация |
|---|---|
| RoulettePresenter | `RouletteStateMachine`, событийный презентер, визуальная последовательность в `RouletteView` |
| BaseWindow | `WindowLifecycleStateMachine`; анимацию выполняет `ReactiveUIView` |
| AdScheduler | Проектная `RewardedAdStateMachine`; сценовые фасады не хранят собственные фазы одного SDK |
| Timer | `TimerStateMachine` и `TimerData`; `Timer` передаёт scaled deltaTime |
| LeaderboardMenu | `LeaderboardStateMachine` и `LeaderboardView`; модель загрузки отделена от отображения |
| MenuNavigation | `MenuPageStateMachine`; машина сессии меню и стек popup остаются независимыми |
| CubeFlightAnimator | Двухсостоянийная `CubeFlightStateMachine`; `Arrived` по-прежнему предшествует завершению settle |
| ShapeFiller | `ShapeFillPlaybackStateMachine`, данные воспроизведения и `ShapeFillView` |
| FillUiPresenter | `FillUiStateMachine`, событийный презентер, визуальная пауза в `FillResultView` |
| FillRescueFlow | Удалён; спасение обрабатывает `RescueAdState` машины интерфейса Fill. Зависимый `FillPauseFlow` также удалён |

План и контракты: [переход на FSM](../../docs/superpowers/plans/2026-10-28-fsm-migration.md). Проверки: 45 FSM + 44 рулетки + 122 сценовых, прогрессия/анимации заливки и 419 проверок камеры — пройдены. [Отчёт](../Reports/2026-10-28-FSM-migration.md). Результаты находятся в `output/fsm-migration/`; список исключённых ниже классов не расширялся автоматически.

---

## Сравнение проходов

- Оба прохода независимо сошлись на 8 классах: `RoulettePresenter`, `LeaderboardMenu`, `FillUiPresenter`, `ShapeFiller`, `MenuNavigation`, `BaseWindow`, `AdScheduler`, `Timer`.
- `FillRescueFlow` — нашёл только проход сабагента; принят: тот же жизненный цикл ad-запроса, что у признанного обоими `AdScheduler`.
- `CubeFlightAnimator` — нашёлся только в основном проходе, сабагент отклонил; перенесён в «Спорные».
- `Pauser` — в основном проходе был на грани, сабагент отклонил; принято его обоснование: источники паузы ортогональны (счётчик запросов, видимость, платформа) — это комбинация причин, а не последовательные состояния.

## Находки

- **RoulettePresenter** — `Assets/MadSlime/Scripts/UI/Roulette/RoulettePresenter.cs` "19"
  Приватный enum `RouletteState` (Hidden, Idle, WaitingForAd, ConfirmingSave, Spinning, SaveFailed), поле `_state` на строке 38. Переходы размазаны присваиваниями по Show/Hide/ResumeAfterPrize/OnAdRequested/OnAdGranted/OnAdNotGranted/Spin/ConfirmAndSpinAsync/OnViewDisabled, а поведение ветвится по `_state` в Update, IsBusy, OnSpinRequested и RefreshButtons. Шесть взаимоисключающих режимов с чёткими входами-выходами и асинхронными переходами (ожидание рекламы, ожидание подтверждения сейва) — самый явный кандидат на FSM в проекте.

- **LeaderboardMenu** — `Assets/MadSlime/Scripts/UI/Leaderboards/LeaderboardMenu.cs` "24"
  Приватный enum `State` (Loading, AuthRequired, Ready, Empty, Error), поле `_state` на строке 47. Единственный метод `SetState` присваивает состояние и вручную перенастраивает UI под каждое значение, повторный switch по `_state` в `OnLanguageChanged` (строка 305) дублирует ту же привязку состояния к поведению. Переходы инициируются из асинхронной загрузки (таймаут SDK, нет авторизации, ошибка запроса) и повторного запроса — жизненный цикл окна с пятью режимами просит FSM.

- **FillUiPresenter** — `Assets/MadSlime/Scripts/UI/Fill/FillUiPresenter.cs` "47"
  Кластер bool-флагов `_isResultPending`, `_isActive`, `_isClosingWindow`, `_isDoubleRewardRequested`, `_isDoubleRewardGranted` плюс enum `WindowAction _pendingAction` (строки 47–53). Состояния (идёт игра, ждём окно результата, показан WinMenu, показан FailMenu, окно закрывается с отложенным действием, ждём рекламу за x2) закодированы комбинациями флагов: `CanOpenPauseMenu` (строка 87) и `CanResolveRescue` (строка 105) проверяют сразу по 4–5 флагов, переходы присваиваниями разбросаны по ShowWin/ShowFailed/CloseWinMenu/CloseFailMenu/CompleteWindowAction. Неявная машина состояний на флагах — прямой кандидат на явный FSM.

- **ShapeFiller** — `Assets/MadSlime/Scripts/ShapeFill/Logic/ShapeFiller.cs` "13"
  Приватный enum `BoostStage` (Normal, Boosted, Instant) рядом с флагами `_isFilling`, `_isStopped`, `_hasResult` (строки 26–29). Фактические состояния — сборка формы, заливка, ускоренная заливка, мгновенная досыпка, остановка — кодируются комбинацией четырёх полей; переходы присваиваниями в ShowResult/Rescue/StopFill/Accelerate/StartInstantCompletion и в асинхронных лупах. Параметры спавна выбираются ветвлениями по stage (`GetSpawnInterval`, `GetFlightDuration`, `PumpFillAsync`), а `CanRescue` проверяет сразу три флага — состояние напрашивается на явный FSM.

- **MenuNavigation** — `Assets/MadSlime/Scripts/UI/Menu/MenuNavigation.cs` "19"
  Приватный enum `WindowState` (Stopped, Main, Skins, Upgrades), поле `_state` на строке 46. Взаимоисключающие экраны меню переключаются присваиванием в `SetWindow`, каждый клик гейтится сравнением состояния (`CanOpenFromMain`, `OnCollectionCloseClicked`, `OnOpenSkinClicked`). Классический ручной переключатель режимов: переходы Main↔Skins↔Upgrades и вход/выход из Stopped — готовая таблица переходов для FSM, тем более что рядом в проекте уже живёт `MenuStateMachine`.

- **BaseWindow** — `Assets/MadSlime/Scripts/UI/Windows/BaseWindow.cs` "16"
  Жизненный цикл окна закодирован тремя bool: `_isClosing`, `_isCloseRequested`, `_isClosed` — итоговые состояния Shown / CloseRequested / Closing / Closed и их комбинации. Переходы присваиваниями в Show/RequestClose/BeginClose/OnDisableCompleted с хуком OnClosing и событием Closed; от корректности комбинаций флагов зависит поведение всех окон проекта (WinMenu, FailMenu, PauseMenu, LeaderboardMenu, RouletteWinPopup). Явные состояния окна (в том числе ветка «закрытие запрошено, ждём завершения анимации») легли бы на FSM без изменения поведения.

- **AdScheduler** — `Assets/MadSlime/Scripts/Ads/Logic/AdScheduler.cs` "17"
  Жизненный цикл rewarded-запроса на двух флагах `_rewardedReceived` и `_isRewardedRequestRunning` плюс набор отложенных колбэков. Состояния: свободен → запрос запущен → реклама открыта → награда получена/не получена/ошибка; переходы присваиваниями в ShowRewarded/OnRewardedOpened/OnRewardReceived/OnRewardedClosed/OnRewardedError, а ShowRewarded отклоняет повторный запрос сравнением флага. Последовательность «ожидание → результат с тремя исходами» — типичный кандидат на FSM с терминальными состояниями.

- **FillRescueFlow** — `Assets/MadSlime/Scripts/UI/Fill/FillRescueFlow.cs` "14"
  Три состояния (простой, идёт запрос рекламы, спасение засчитано) на двух флагах `_isRequestRunning` и `_isResolved`. Переходы присваиваниями в Reset/StopRequest/Begin/Resolve/OnAdCompletedWithoutReward с ручными проверками в каждом методе. Мини-машина жизненного цикла ad-запроса — зеркало AdScheduler внутри UI-слоя.

- **Timer** — `Assets/MadSlime/Scripts/Game/Logic/Timer.cs` "12"
  Состояния «не настроен / готов / идёт / завершён» на двух флагах `_isSetupFinished` и `_isRunning`; часть переходов выбрасывает исключения (StartCount до Setup, повторный StartCount), Stop и завершение лупа присваиванием возвращают в останов. У работающего таймера своя логика (UniTask-тик, учёт timeScale) с явным входом и выходом — состояния просятся в явный FSM вместо пары флагов.

## Спорные

- **CubeFlightAnimator** — `Assets/MadSlime/Scripts/ShapeFill/View/CubeFlightAnimator.cs` "10"
  Формально это уже ручная FSM: enum `FlightState` (Idle, Flying), поле `_state` (строка 39), Update ветвится по нему, переходы Launch → Flying → Idle по завершении полёта. Сабагент возразил, что два состояния — «по сути один bool» и FSM там оверинжиниринг; по критерию, читаемому буквально, это единственное место, где собственный state-enum живёт вне FSM-инфраструктуры проекта. Оставлено на усмотрение: находка реальна, но выгода от внедрения сомнительна.

## Проверено и исключено

Не считаются находками (флаг-параметр или одна ветка логики, FSM был бы оверинжинирингом): `SmellAbilityActivator`, `AdrenalineBoost`, `SpeedSmoke`, `TimerTickSound`, `Movement`, `FillPlayerView`, `TouchJoystick`, `GhostAnimator` (визуальные галки с guard-сеттерами), `RouletteWheel` (`_isSpinning` + флаг инициализации), `MusicChannel` (непрерывный кроссфейд, не дискретные режимы), `Pauser` (ортогональные источники паузы, а не последовательные состояния), `ItemCollector`, `QuotaCounter`, `Rewarder`, `FillPlayerPresenter`, `SaveConfirmation`, `SavesCore`, `GameStartup`, `ResetProgressFlow`, `MenuContent`, `SkinsPagePresenter`, `GameplayPauseFlow`, `MenuRouletteFlow`, `LoadingScreenController` (состояние = ссылка на активный инстанс окна, без собственных режимов), `ShapeFillOrchestrator` (фасад без состояния), `LootRoller`, `AnimationScheduler` (без состояния), наследники `BaseWindow` с одноразовым `_isNavigationRequested`/`_isActionRequested`, dirty-флаги (`QuotaTypeState`, `QuotaItemHighlighter`, `UniformTextSize`, `CurrencyAmountLayout`, `CollectedItemsStorage`, `AudioMixerController`), флаги видимости/инициализации (`LoadingScreenPresenter`, `SceneTransitor`, `GameplayHud`, `GridCameraFitter`, `Clamper`, `CameraFollow`, `Yg2SavesAccess`, `LocalizationService`), Editor-инструменты (конфигурационные bool без смены состояний) и все switch по enum-данным (редкости, типы апгрейдов, формы зон) — там нет смены состояния объекта.
