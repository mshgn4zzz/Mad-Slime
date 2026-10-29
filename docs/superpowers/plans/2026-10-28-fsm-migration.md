# Переход оставшихся систем MadSlime на FSM — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans. Выполнять по этапам в текущем чате, без сабагентов. Реализация начинается только после отдельного указания пользователя. Каждый этап проходит проверку RED → GREEN и собственное ревью.

**Goal:** Перенести все актуальные находки из Wiki/Ревью/Потенциальные места для FSM.md, включая спорный CubeFlightAnimator, на BaseStateMachine/IState с тем же разделением логики и представления, которое применено к рулетке.

**Architecture:** Отдельные классы состояний владеют правилами переходов и допустимыми командами. Модели синхронно изменяют данные; сетевые запросы и подтверждение сохранения остаются асинхронными операциями соответствующих состояний. Презентеры принимают события и управляют представлением; анимации, визуальные паузы, движение и спавн по времени выполняет View. Независимые жизненные циклы получают отдельные машины, а не комбинации состояний одной глобальной машины.

**Tech Stack:** Unity 2022.3.62f2, C#, VContainer, UniTask, uGUI, DOTween. Без новых пакетов.

**Spec:** Wiki/Ревью/Потенциальные места для FSM.md; Wiki/Ревью/Требования проверки репозитория.md, особенно П-23/25/26/33/35/39; решения пользователя в текущем чате о мгновенной модели, событийном презентере, View с визуальными задержками и отсутствии дополнительных Flow.

## Глобальные ограничения

- В этом задании составляется план: игровой код, сцены, префабы и сохранения не изменяются.
- Использовать BaseStateMachine<TState> и IState.Enter/Exit. Не создавать вторую FSM-инфраструктуру.
- Единственный источник текущей фазы — CurrentState. Не сохранять рядом отражающий её enum или набор bool.
- Данные результата, числовые счётчики, идентификаторы запросов и команды пользователя не становятся состояниями только из-за своего типа. Enum действия навигации допустим как полезная нагрузка команды, но не как замена текущего состояния.
- Не добавлять Flow. FillRescueFlow поглощается машиной интерфейса Fill. Зависимый FillPauseFlow также убирается при миграции его владельца; остальные не относящиеся к задаче Flow не переписываются.
- View выполняет анимации и визуальные паузы. Машина может ожидать событие завершения представления, но не выдерживает его длительность сама.
- Отсчёт игрового Timer, ожидание SDK/сетевого ответа и подтверждение сохранения — работа механики, а не визуальная пауза; не переносить их в UI.
- Состояния создаются и передаются через DI/фабрики в точках композиции. Презентеры не разыскивают зависимости через Find и не собирают сервисы через new.
- Фабрики создают новые состояния и данные на каждый экземпляр окна/куба. Общий Singleton состояния для разных экземпляров запрещён.
- Подписки имеют именованные обработчики; анонимные подписки на события не вводятся. Выход отменяет собственные операции состояния; визуальные операции отменяет владелец представления.
- Поздние ответы проверяются по конкретному входу/запросу, а не только по типу текущего состояния: повторный вход в тот же класс не оживляет старую операцию.
- Сохранить существующие MonoBehaviour, GUID .meta, ссылки инспектора, настройки длительностей, цены и геймплейные события. Новые файлы получают .meta.
- Границы сборок сохраняются: Gameplay не зависит от UI. Новые машины помещаются под существующий asmref соответствующего владельца.
- Не исправлять посторонние замечания из общего ревью и не трогать незакоммиченные изменения вне перечисленных владельцев.

## Сверка ревью с текущим кодом

Проверка выполнена чтением текущего рабочего дерева, не старых версий из Git. Номера строк в исходном ревью не используются как адреса будущих правок.

| Пункт ревью | Что действительно в коде сейчас | Решение |
|---|---|---|
| RoulettePresenter | Старого enum RouletteState нет. Уже существуют RouletteStateMachine и шесть классов состояний; ResultReady получает презентер, а вращение и паузу выполняет RouletteView | Миграция выполнена; использовать как регрессионный пример, не переписывать повторно |
| LeaderboardMenu | Enum State и поле _state остались; LoadAsync смешивает SDK, авторизацию, сеть, Render и прокрутку; SetState и OnLanguageChanged зависят от enum | Разделить машину загрузки и событийное представление; жизненный цикл окна оставить отдельным |
| FillUiPresenter | Пять флагов фаз и WindowAction остались. Задержка результата находится в презентере; Win/Fail приходят через FillSceneState | Вынести решения в FillUiStateMachine, задержку — в View; сохранить событийную цепочку существующей FillStateMachine |
| ShapeFiller | BoostStage и три bool остались. Начальный результат уже рассчитан до ShowResult. Завершение выпуска кубов и прилёт последнего куба — разные события | Отдельная локальная FSM воспроизведения, не новая сцена/сессия. View сохраняет анимационные расписания |
| MenuNavigation | WindowState и SetWindow остались. Вложенными окнами уже занимается стек; рядом MenuStateMachine управляет жизненным циклом всей сессии меню | Локальная MenuPageStateMachine для страниц; существующая MenuStateMachine остаётся владельцем сессии |
| BaseWindow | Три bool остались. RequestClose только сообщает запрос, BeginClose запускает скрытие, Closed приходит по DisableCompleted | Общая FSM жизненного цикла окна; анимацию выполняет ReactiveUIView через адаптер |
| AdScheduler | Два bool и callbacks остались. Каждый сценовый scheduler подписан на один общий IAdsService | Одна проектная FSM rewarded-запроса; сценовые scheduler — фасады конфигурации и клиентских команд |
| FillRescueFlow | Оба bool остались; объект создаётся внутри FillUiPresenter и дублирует ожидание рекламного результата | Поглотить состоянием RescueAd в FillUiStateMachine; отдельную FSM/Flow не создавать |
| Timer | Оба bool и асинхронный цикл остались. Stop сохраняет остаток; Setup сбрасывает остаток даже во время работы | TimerStateMachine с моделью времени и одним Update-мостом, сохранить публичный API и смысл команд |
| CubeFlightAnimator, спорный | FlightState Idle/Flying и ручное переключение остались. Arrived вызывается сразу после достижения цели, до окончания settle-анимации | Включить в миграцию по требованию пользователя о FSM для смены состояний; сохранить два состояния и прежний момент Arrived |

Раздел исходного ревью «Проверено и исключено» остаётся вне миграции: Pauser, dirty-флаги, редкости/типы апгрейдов, параметры инициализации и прочие исключённые классы не получают машины автоматически. MenuRouletteFlow из этого раздела уже удалён. FillPauseFlow затрагивается только как зависимость переносимого FillUiPresenter.

В списках файлов сокращённые пути UI/…, Game/…, Ads/…, Economy/…, ShapeFill/… и DI/… относятся к Assets/MadSlime/Scripts/.

## Границы владения и времена жизни

| Машина | Время жизни | Адаптер / представление |
|---|---|---|
| WindowLifecycleStateMachine | На каждый экземпляр BaseWindow | Сам BaseWindow как адаптер существующего ReactiveUIView |
| RewardedAdStateMachine | ProjectLifetimeScope, один экземпляр на общий IAdsService | Сценовые AdScheduler и Yg2AdsService |
| TimerStateMachine | GameLifetimeScope | Timer как MonoBehaviour-мост; TimerUI и TimerTickSound остаются подписчиками |
| LeaderboardStateMachine | На открытое окно лидерборда | LeaderboardMenu как презентер + LeaderboardView |
| MenuPageStateMachine | MenuLifetimeScope | MenuNavigation; существующий MenuPopupStack остаётся независимым |
| CubeFlightStateMachine | На каждый куб | CubeFlightAnimator, сохраняющий визуальную математику и сериализованные настройки |
| ShapeFillPlaybackStateMachine | FillLifetimeScope | ShapeFiller как презентер/совместимый фасад + ShapeFillView |
| FillUiStateMachine | FillLifetimeScope | FillUiPresenter + FillResultView + существующие WinMenu/FailMenu/PauseMenu |

Новые машины создаются в существующих LifetimeScope. Для изменяемого числа объектов нужны только две предметные фабрики: WindowLifecycleStateMachineFactory и CubeFlightStateMachineFactory. Фабрика лидерборда создаёт независимую машину содержимого окна. Новых универсальных фабрик фабрик и глобального контроллера всех состояний нет.

## Review Focus

1. Синхронное завершение: окно без аниматоров, мгновенный ad-callback и готовый save-task могут вызвать переход прямо из Enter; подписки и CurrentState должны быть установлены заранее.
2. Отмена и повторное открытие: старый SDK/сетевой/рекламный ответ, старая анимация и старый прилёт куба не завершают новую операцию с тем же типом состояния.
3. Идемпотентность наград: повтор подтверждения x2 не начисляет деньги повторно; спасение передаётся существующей игровой машине один раз.
4. Независимые уровни: закрытие BaseWindow не путается с Error лидерборда; сессия меню не сбрасывается при смене страницы; общая реклама не считается свободной только из-за уничтожения клиента.
5. Временные границы: результат модели не ждёт анимации; FillCompleted зависит от прибытия кубов, Arrived не ждёт settle, StopFill не удаляет данные, нужные для Rescue.

## Проверочный контур для реализации

Создать tools/verification/run-fsm-migration.sh с режимами windows, ads, timer, leaderboard, menu, cube-flight, shape-fill, fill-ui, all. Проверки выполняются в отдельной .utmp/fsm-migration с копией Assets/Packages/ProjectSettings и существующего кэша Library. Не использовать одновременно одну копию проекта для двух Editor-процессов. Сохранения пользователя не читать/перезаписывать: отдельное productName и фикстуры в копии.

Реальные модели/машины/префабы проверяются напрямую. Подменяются внешние рекламные/сетевые/облачные ответы и источник игрового deltaTime в изолированных проверках. Ошибки Unity и необработанные исключения делают прогон неуспешным.

Перед расширением прогонов исправить состав копируемых фикстур в tools/verification/run-progression-fill.sh: сейчас он копирует все *Checks.cs, включая проверки сцен/рулетки, но не их FsmPlatformDoubles/SceneLifecycleProbe. Копировать только ProgressionFillChecks.cs и FillAnimationChecks.cs с их реальными зависимостями либо явно полный нужный набор; не оставлять случайный wildcard. Это выявленный статически риск запуска, не результат выполненного теста. Отдельный режим animation обязателен: запуск скрипта без аргумента выполняет только checks.

Каждый этап: сначала characterization существующего публичного поведения; затем тест нового контракта, наблюдаемое ожидаемое падение; реализация; зелёный предметный прогон; регрессии зависимых сцен. Никаких тестов наличия строк исходника вместо поведения. Проверки не запускаются в ходе составления этого плана.

## Этап 1. BaseWindow: общая FSM жизненного цикла

**Файлы:** изменить Assets/MadSlime/Scripts/UI/Windows/BaseWindow.cs и регистрацию фабрики в DI/ProjectLifetimeScope.cs. Создать UI/Windows/StateMachine/WindowLifecycleStateMachine.cs, IWindowLifecycleState.cs, WindowLifecycleStateMachineFactory.cs; States/VisibleState.cs, CloseRequestedState.cs, ClosingState.cs, ClosedState.cs. Тест: tools/verification/WindowLifecycleChecks.cs.

**Контракт машины:** Show(), RequestClose(), BeginClose(), NotifyHidden(), Disable(), Stop(), Dispose(); события ShowRequested, CloseRequested, HideRequested, Closed. Значение IsClosing вычисляется из состояния и сохраняет старый смысл: после BeginClose истинно до следующего Show, включая завершённое закрытие.

**Переходы:** остановлена/Closed → Visible по Show; Visible → CloseRequested по RequestClose; Visible/CloseRequested → Closing по BeginClose; Closing → Closed по NotifyHidden. Show во время скрытия отменяет старый цикл и открывает новый без приёма старого completion.

BaseWindow подписывается на события машины, вызывает прежний OnClosing и ReactiveUIView.Enable/Disable. Машина не знает длительностей и TweenAnimatorBase. RequestClose не скрывает автоматически: решение владельца окна сохраняется. Принудительный Disable завершает активное окно один раз; если оно уже закрыто либо владелец отписался при освобождении, повторного уведомления нет. Это явная унификация с уже реализованным поведением рулетки.

- [ ] Зафиксировать Show → RequestClose без автоматического скрытия, BeginClose → OnClosing → Closed, повторные команды и повторное использование окна.
- [ ] Добавить RED-проверки одной доставки Closed при синхронном DisableCompleted, внешнем выключении и выключении во время анимации.
- [ ] Внедрить машину, убрать три bool, сохранить события/хуки и сериализацию наследников. Проверить OnDisable-порядок у WinMenu, FailMenu, PauseMenu, LeaderboardMenu и RouletteWinPopup.
- [ ] Run: bash tools/verification/run-fsm-migration.sh windows. Expected: все проверки GREEN, один Closed на цикл, хуки вызываются однократно.
- [ ] Run: bash tools/verification/run-skin-roulette.sh и bash tools/verification/run-scene-transitions.sh. Expected: нет регрессий стека/переходов.

## Этап 2. AdScheduler: одна FSM общего rewarded-запроса

**Файлы:** изменить Ads/Logic/AdScheduler.cs, DI/ProjectLifetimeScope.cs; при необходимости только точки композиции сцен. Создать Ads/Logic/StateMachine/RewardedAdStateMachine.cs, IRewardedAdState.cs, RewardedAdRequest.cs; States/AvailableState.cs, RequestedState.cs, ShowingState.cs, RewardPendingCloseState.cs. Тест: tools/verification/RewardedAdChecks.cs.

**Контракт:** Submit(RewardedAdRequest request) возвращает bool; CancelConsumer(RewardedAdRequest request) отсоединяет конкретного клиента; Dispose() снимает SDK-подписки. RewardedAdRequest содержит идентичность, rewardId и исходные callbacks. AdScheduler сохраняет ShowRewarded, ShowDoubleReward, RouletteRewardId и TryShowInterstitial как совместимые фасады.

**Переходы:** Available → Requested при приёме; Requested → Showing по Opened; Requested/Showing → RewardPendingClose по совпадающему rewardId; Closed возвращает в Available и только после этого доставляет granted либо completedWithoutReward; Error возвращает в Available и доставляет error. Повторная заявка отклоняется через onRejected, не меняя активный запрос. Opened после полученной награды не стирает её.

Машина получает IAdsService и подписывается на SDK-события на уровне ProjectLifetimeScope. Сценовые AdScheduler не ведут независимых флагов одного SDK. Отключение клиента убирает его callbacks, но машина продолжает учитывать физически незавершённую рекламу до терминального SDK-события; новый клиент не получает прежнюю награду. До внешнего callback активная заявка освобождается, чтобы он мог синхронно отправить следующую.

**Ограничение текущего интерфейса:** RewardedClosed и RewardedError не несут ID запроса. Локальные generation-ID защищают callbacks клиентов, но не могут доказуемо различить произвольные запоздалые нетегированные SDK-события разных запросов. Не обещать такую защиту фиктивным счётчиком: сохранять единственный физический запрос до терминала. Расширение корреляции SDK требует отдельной проверки адаптера, если его порядок событий окажется недостаточен.

- [ ] Проверить opened → reward → closed, closed без reward, error до/после opened, несовпадающий rewardId, повторный reward, конкурирующие заявки двух сценовых фасадов и синхронные callbacks.
- [ ] Добавить RED на отмену клиента без освобождения занятого SDK и на повторную заявку из завершившего callback.
- [ ] Перенести флаги и отложенные исходы в состояния/данные запроса. Interstitial не должен стартовать поверх принятой, но ещё не открытой rewarded-заявки.
- [ ] Run: bash tools/verification/run-fsm-migration.sh ads. Expected: один исход на принятый запрос, отклонённый запрос не портит активный, старый клиент не получает награду.
- [ ] Повторить roulette и сценовые проверки: текущий публичный API рекламы используется рулеткой, спасением и x2.

## Этап 3. Timer: FSM отсчёта

**Файлы:** изменить Game/Logic/Timer.cs и DI/GameLifetimeScope.cs. Создать Game/Logic/TimerStateMachine/TimerStateMachine.cs, ITimerState.cs, TimerData.cs; States/ReadyState.cs, RunningState.cs, CompletedState.cs. Тест: tools/verification/TimerStateChecks.cs.

**Контракт:** сохранить Timer.Setup(float), StartCount(), Stop(), Ticked(float), Finished. Внутренняя машина получает Configure(float), StartCount(), StopCount(), Tick(float), Stop(). Timer.Update передаёт масштабированный Time.deltaTime; единственного coroutine/UniTask-цикла больше нет. RunningState синхронно уменьшает TimerData и сообщает tick/completion. UI остаётся подписчиком.

**Переходы:** ненастроена → Ready по Setup; Ready → Running по StartCount; Running → Ready по StopCount с сохранением остатка; Running → Completed при нуле. Setup во время Running сбрасывает данные без второго цикла. StartCount до Setup и повторный StartCount во время Running сохраняют исключения. После Completed запуск без Setup не восстанавливает длительность: остаётся прежнее поведение немедленного завершения.

Явное изменение жизненного цикла: нынешний Timer не обрабатывает OnDisable, поэтому его UniTask может продолжать работу на отключённом компоненте. В новом варианте OnDisable вызывает StopCount с сохранением остатка, OnDestroy — Stop/Dispose. Это намеренное прекращение работы отключённого объекта, закрепляемое отдельным тестом, а не объявляемое сохранённым поведением.

- [ ] Зафиксировать Stop/продолжение, Setup во время работы, первый запуск, повторный Start, события и scaled time. Не нормализовать незаметно значения последнего Ticked или правила перезапуска.
- [ ] RED → реализовать машину и один Update-мост; проверить синхронный Stop/Setup из обработчика Ticked, Stop → Start в одном кадре, отсутствие двойного Finished.
- [ ] Run: bash tools/verification/run-fsm-migration.sh timer. Expected: timeScale=0 не продвигает время, завершение однократно, UI/TimerTickSound сохраняют подписки.
- [ ] Run: bash tools/verification/run-scene-transitions.sh. Expected: игровой старт, завершение и переход в Fill без изменений.

## Этап 4. LeaderboardMenu: машина данных и событийный презентер

**Файлы:** изменить UI/Leaderboards/LeaderboardMenu.cs, DI/ProjectLifetimeScope.cs для фабрики. Создать UI/Leaderboards/StateMachine/LeaderboardStateMachine.cs, ILeaderboardState.cs, LeaderboardSessionData.cs, LeaderboardStateMachineFactory.cs; States/WaitingForSdkState.cs, LoadingState.cs, AuthRequiredState.cs, ReadyState.cs, EmptyState.cs, ErrorState.cs. Создать UI/Leaderboards/LeaderboardView.cs. Тест: tools/verification/LeaderboardStateChecks.cs.

**Контракт машины:** Start(), Refresh(), RequestAuthorization(), Stop(), Dispose(); события LoadingStarted, AuthorizationRequired, SnapshotReady(LeaderboardData), Empty, Failed(LeaderboardData). Данные Error могут содержать частично полученный snapshot. Конкретный запрос и отмена принадлежат состоянию загрузки. SDK-ожидание — максимум прежние 8 секунд; запрашивается прежний TopCount=10.

**Переходы:** Start/Refresh → WaitingForSdk; SDKReady → AuthRequired или Loading; ответ → Ready/Empty/Error. Retry из Empty/Error запускает новый запрос; готовность SDK может инициировать обновление в соответствии с существующим поведением. Запрос во время текущей загрузки не создаёт второй параллельный запрос.

LeaderboardMenu сохраняется как наследник BaseWindow и событийный презентер. Он передаёт клики машине, получает данные и вызывает View. View строит строки, локализует сообщения, пересчитывает высоту и прокручивает после layout-pass. LateUpdate/ожидание LastPostLateUpdate не переносятся в машину данных. Начало закрытия останавливает загрузку немедленно, а не после анимации.

**Совместимость:** сейчас Render при HasPlayerError переводит окно в Error уже после создания строк; эти строки могут остаться видимыми. План сохраняет частичные данные и статус ошибки, а не превращает любой Error в пустое окно. Сериализованные ссылки LeaderboardMenu сохраняются; View может быть обычным объектом с переданными ссылками, без ручного переноса компонентов в префабе.

- [ ] Проверить SDK timeout, отсутствие авторизации, успешный top, own-player вне top, empty, полный и частичный error, retry, смену языка без повторной сети.
- [ ] RED на старый запрос после закрытия/повторного открытия и старую прокрутку после нового snapshot.
- [ ] Убрать State/_state/SetState и дублирующий switch из окна; привязать локализацию к текущей проекции данных View, без второго lifecycle-enum.
- [ ] Run: bash tools/verification/run-fsm-migration.sh leaderboard. Expected: все исходы и частичные строки отображаются правильно; закрытое окно не обновляется.
- [ ] Повторить windows и menu/scene smoke.

## Этап 5. MenuNavigation: локальная FSM страниц

**Файлы:** изменить UI/Menu/MenuNavigation.cs и DI/MenuLifetimeScope.cs. Создать UI/Menu/PageStateMachine/MenuPageStateMachine.cs, IMenuPageState.cs; States/MainPageState.cs, SkinsPageState.cs, UpgradesPageState.cs. Тест: tools/verification/MenuPageChecks.cs.

**Контракт:** Start(), OpenSkins(), OpenUpgrades(), Back(), RequestPlay(), RequestOpenSkin(), Stop(); события MainRequested, SkinsRequested, UpgradesRequested, PlayRequested, SkinOpeningRequested. События страниц управляют существующими окнами через MenuNavigation. Вход в Skins запрашивает обновление коллекции/preview.

**Переходы:** остановлена → Main; Main → Skins/Upgrades; Skins/Upgrades → Main. Play разрешён только Main; открытие скина — только Skins. MenuNavigation сохраняет внешнюю проверку модальности/scene-transition/reset перед передачей команды: это условия других систем, не дублирующее состояние страницы.

MenuStateMachine/MenuRunningState продолжают управлять всей сессией и музыкой. Их Start/Stop запускают/останавливают локальную машину через существующий адаптер. Не объединять Main/Skins с вариантами каждого popup: стек модальных окон остаётся отдельным механизмом. Это также позволяет избежать циклической DI-зависимости между существующей машиной сессии и MenuNavigation.

- [ ] RED на команды вне допустимой страницы и повторные Run/Stop. Проверить вход/выход коллекции, preview camera и возврат из вложенных окон.
- [ ] Убрать WindowState/_state/SetWindow; отображение выполняют обработчики событий презентера. _isSubscribed не превращать в состояние страницы: это учёт подписок, который можно убрать правильным жизненным циклом.
- [ ] Run: bash tools/verification/run-fsm-migration.sh menu. Expected: Main/Skins/Upgrades имеют одну активную страницу; музыка не перезапускается при смене страницы.
- [ ] Run: bash tools/verification/run-skin-roulette.sh и bash tools/verification/run-scene-transitions.sh. Expected: обе рулетки, настройки, лидерборд и сцены работают.

## Этап 6. CubeFlightAnimator: минимальная двухсостоянийная FSM

**Файлы:** изменить ShapeFill/View/CubeFlightAnimator.cs, ShapeFill/Logic/CubeSpawner.cs и DI/FillLifetimeScope.cs. Создать ShapeFill/Logic/CubeFlightStateMachine/CubeFlightStateMachine.cs, ICubeFlightState.cs, CubeFlightData.cs, CubeFlightStateMachineFactory.cs; States/IdleState.cs, FlyingState.cs. Тест: tools/verification/CubeFlightStateChecks.cs; использовать существующие FillAnimationChecks как регрессию.

**Контракт:** машина Launch(параметры полёта), CompleteFlight(), Stop(); события FlightRequested(данные), FlightStopped, Arrived. Существующий CubeFlightAnimator.Launch(target, departureDirection, departureDistance, duration) и Arrived(CubeFlightAnimator) остаются внешним API. CubeSpawner получает новую машину из фабрики и передаёт каждому созданному animator до первого Launch/GrowIn.

**Переходы:** Idle → Flying по Launch; Flying → Idle при достижении цели; Stop/Disable прекращает текущий полёт без Arrived. Повторный Launch заменяет полёт без ложного прибытия старого. Внешний disabled Update не считается новой фазой полёта.

Математика Bezier, растяжение, вращение, GrowIn и settle остаются в CubeFlightAnimator как View/адаптере с прежними сериализованными настройками. Update включён только на время визуального полёта; FSM не получает словарный поиск/новые объекты на каждый кадр. По достижении цели View сообщает машине, машина переходит в Idle и уведомляет Arrived. Приземление остаётся визуальным эффектом и не задерживает Arrived.

- [ ] RED на контракт нового контроллера: переход Launch → прибытие, повторный Launch, выключение и независимость двух кубов. Удаление enum проверяется ревью diff, а не тестом текста исходника.
- [ ] Проверить, что Arrived однократен и происходит до окончания settle; GrowIn не считается прибытием.
- [ ] Run: bash tools/verification/run-fsm-migration.sh cube-flight. Expected: независимые машины, прежние конечные transform и момент событий, нет новых покадровых аллокаций.
- [ ] Run: bash tools/verification/run-progression-fill.sh animation. Expected: существующие проверки анимации проходят после явной упаковки нужных фикстур.

## Этап 7. ShapeFiller: FSM воспроизведения заливки

**Файлы:** изменить ShapeFill/Logic/ShapeFiller.cs, ShapeFill/Logic/ShapeFillOrchestrator.cs только в точках делегирования и DI/FillLifetimeScope.cs. Создать ShapeFill/Logic/PlaybackStateMachine/ShapeFillPlaybackStateMachine.cs, IShapeFillPlaybackState.cs, ShapeFillPlaybackData.cs; States/NormalFillState.cs, BoostedFillState.cs, InstantFillState.cs, AwaitingArrivalsState.cs, CompletedState.cs. Создать ShapeFill/View/ShapeFillView.cs как обычный объект представления с переданными зависимостями. Тест: tools/verification/ShapeFillPlaybackChecks.cs.

**Контракт:** сохранить Initialize, BuildShape, ShowResult(FillResult), Rescue, StopFill, Accelerate, CanRescue, GetFillFraction/GetFormFillFraction и события FillCompleted/CubeArrived фасада. Машина принимает ShowResult, Rescue, Accelerate, SpawningCompleted, CubeArrived и Stop. События машины: PlaybackRequested(план), SpeedChanged(параметры), Completed(доля). Счётчики target/spawned/arrived и сохранённый FillResult — данные, не флаги режима.

**Переходы:** ShowResult → Normal; первое Accelerate → Boosted; второе → Instant; конец выпуска → AwaitingArrivals; последний требуемый прилёт → Completed. Если всё прилетело синхронно, переход из активного режима в Completed допускается сразу. Нулевой target завершает результат сразу и однократно. Rescue использует сохранённый результат/индекс, увеличивает target до полного и запускает оставшуюся часть с Normal.

ShapeFiller становится событийным презентером/совместимым фасадом. ShapeFillView владеет начальной задержкой, основной и бонусной волнами, border cascade, спавном и подключением аниматоров. Переключение Normal → Boosted меняет параметры уже идущего представления без повторного проигрывания FillDelay/BonusWaveDelay и без сброса прогресса. Instant отменяет только прежнюю задачу выпуска и выпускает остаток; уже летящие кубы продолжают учитываться. Граница/ghost — независимое визуальное оформление, не дополнительные состояния заливки.

**Особенность существующего взаимодействия:** FillingState.Exit вызывает StopFill до ResultState.Enter, где проверяется CanRescue. Поэтому StopFill останавливает воспроизведение, но не удаляет FillResult, target и счётчики. Только BuildShape/новая сессия сбрасывает эти данные. Не заменять остановку очисткой всей сессии, иначе пропадёт спасение. Доступность CanRescue после раннего Stop и после окончания выпуска зафиксировать characterization-тестом; ужесточение её правил не подмешивать незаметно к переносу архитектуры.

- [ ] RED на Normal/Boosted/Instant, сохранение счётчиков при смене режима, число событий и независимое завершение выпуска/прилётов.
- [ ] Проверить нулевой target, начальную/бонусную задержки, Stop → Result → Rescue, поздние прилёты прежнего поколения и повторный ShowResult.
- [ ] Убрать BoostStage/_isFilling/_isStopped/_hasResult. Представление хранит свои cancellation handles; ожидающие callbacks старых кубов снимаются при сбросе, а номер поколения не заменяет текущую фазу FSM.
- [ ] Сохранить поле _ghostBackground на существующем компоненте; передать ссылку View при настройке. Переносить MonoBehaviour и поля префаба ради разделения не требуется.
- [ ] Run: bash tools/verification/run-fsm-migration.sh shape-fill; bash tools/verification/run-progression-fill.sh checks; bash tools/verification/run-progression-fill.sh animation; bash tools/verification/run-fill-camera.sh checks. Expected: прежняя форма/эмиссия/скорости, корректное завершение и спасение, нет старых callbacks после Stop.

## Этап 8. FillUiPresenter + FillRescueFlow: единая машина интерфейса Fill

**Файлы:** изменить UI/Fill/FillUiPresenter.cs и DI/FillLifetimeScope.cs; удалить UI/Fill/FillRescueFlow.cs/.meta и зависимый FillPauseFlow.cs/.meta после переноса подписок. Создать UI/Fill/StateMachine/FillUiStateMachine.cs, IFillUiState.cs, FillUiSessionData.cs, FillUiAction.cs; States/ActiveState.cs, PausedState.cs, PresentingResultState.cs, WinState.cs, FailState.cs, DoubleRewardAdState.cs, DoubleRewardSaveState.cs, DoubleRewardSaveFailedState.cs, RewardedWinState.cs, RescueAdState.cs, ClosingState.cs. Создать UI/Fill/FillResultView.cs, Economy/Logic/DoubleRewardModel.cs и Economy/Data/DoubleRewardReceipt.cs; изменить UI/Windows/WinMenu.cs и Assets/MadSlime/Resources/Prefabs/UI/ResultMenu/WinMenu.prefab для состояния ошибки/повтора, добавить локализованные подписи через существующий Scriptables/Localization/Localization.asset. Тест: tools/verification/FillUiStateChecks.cs.

**Контракт:** машина Start/Stop, PresentWin(int), PresentFail(int,bool), ResultShown(), RequestPause(), RequestDoubleReward(), RequestRescue(), RequestAction(FillUiAction), WindowClosed(); события ResultPresentationRequested(данные), PauseRequested, WindowCloseRequested, AdvertisementRequested(данные запроса), RewardChanged(int), NavigationRequested(FillUiAction), InteractionChanged. FillUiAction — намерение пользователя (продолжить/следующий уровень/рестарт/меню/спасение), не текущая фаза.

**Переходы:** Active → Paused по Pause; входящие Win/Failed → PresentingResult; ResultShown → Win/Fail; Win → DoubleRewardAd по x2; грант → DoubleRewardSave; ошибка подтверждения → DoubleRewardSaveFailed; retry возвращает только в DoubleRewardSave; успешное подтверждение → RewardedWin. Fail → RescueAd по спасению; грант → Closing(Rescue), отказ → Fail. Команда выхода из окна → Closing; только WindowClosed выполняет NavigationRequested. Закрытие паузы с Continue возвращает Active. Завершение сессии останавливает машину из любой фазы. Входящий Win/Failed во время паузы сначала освобождает принадлежащую UI паузу/окно, затем входит в PresentingResult; пауза не остаётся спрятанной под результатом.

PresentingResult нужен для блокировки действий до реально показанного окна; он сразу запрашивает представление и не содержит задержки. FillResultView выдерживает прежний _resultWindowDelay, а WinMenu выполняет count-up. Closing запрашивает скрытие и принимает WindowClosed; сам не анимирует и не отсчитывает длительность. Это машина интерфейсного протокола, отдельная от существующей FillStateMachine сессии и от WindowLifecycleStateMachine конкретного окна.

FillUiPresenter подписывается на FillSceneState.Win/Failed/Finished/PreparationFailed, передаёт их машине и исполняет её UI-события. Поля _isResultPending/_isActive/_isClosingWindow/_isDoubleRewardRequested/_isDoubleRewardGranted исчезают. Окна создаёт и освобождает презентер, а переход в сцену или Rescue передаётся FillSceneState после закрытия окна. Пауза принадлежит соответствующему входу и освобождается один раз при выходе/выключении. Навигация во время ожидания x2-рекламы/сохранения остаётся доступной там, где она доступна сейчас; выход инвалидирует поздние callbacks.

**Модель x2:** сейчас OnDoubleRewardGranted вызывает Wallet.Add, затем ConfirmSavedAsync без отдельного SavesTransaction. DoubleRewardModel.Grant(int baseReward) применяет начисление baseReward один раз через существующий SavesTransaction и возвращает DoubleRewardReceipt с BaseReward и TotalReward (baseReward * 2); receipt сохраняется в данных принятой операции. Доступ к Grant допускает только состояние первого принятого гранта; retry использует сохранённый receipt. Подтверждение вынесено в save-state; retry не вызывает модель начисления снова. Базовая сумма, множитель 2 и момент допуска к x2 сохраняются. Ошибка подтверждения имеет явный повтор/выход, а не скрытое повторное начисление; UI-сообщение и retry добавляются к WinMenu как часть этого этапа. Спасение не переносит оплату в UI: существующие FillStateMachine/RescuingPrepareState/FillSessionSaver продолжают владеть игровым продолжением и сохранением.

- [ ] Зафиксировать текущий показ Win/Fail, задержку, паузу/возобновление, x2, спасение и действие только после Closed.
- [ ] RED на позднюю рекламу после ухода из окна, однократный x2, retry подтверждения, однократный Rescue и закрытие в процессе сохранения.
- [ ] Проверить PresentingResult без таймера в FSM, отсутствие ввода до ResultShown, подготовительную ошибку во время восстановления и остановку сессии во всех фазах.
- [ ] Перенести два Flow без новой прослойки; сохранить события внешних окон и Finish/Prepare игрового контура.
- [ ] Run: bash tools/verification/run-fsm-migration.sh fill-ui. Expected: одна награда/навигационная команда/пара pause-resume на принятую операцию; визуальная отмена не отменяет подтверждённое начисление.
- [ ] Run: bash tools/verification/run-scene-transitions.sh; bash tools/verification/run-progression-fill.sh checks. Expected: win, fail, rescue success/reject/error, повторная сессия и выход в меню без регрессий.

## Финальная проверка и обновление Wiki

- [ ] Run: bash tools/verification/run-fsm-migration.sh all — все восемь групп зелёные.
- [ ] Run: bash tools/verification/run-skin-roulette.sh — уже перенесённая рулетка работает с новыми окнами и общей рекламой.
- [ ] Run: bash tools/verification/run-scene-transitions.sh; bash tools/verification/run-progression-fill.sh; bash tools/verification/run-fill-camera.sh — все существующие связанные проверки проходят. Любые исходные посторонние падения указать отдельно, не скрывать.
- [ ] Осмотреть реальные Menu/Game/Fill на штатных портретном и ландшафтном разрешениях: layout, preview, обе рулетки, лидерборд, пауза, x2 и спасение. Не считать headless-проверки доказательством визуального совпадения.
- [ ] Самостоятельно проверить diff без сабагентов: нет второго поля текущей фазы, состояния не управляют Tween/задержками UI, нет новых Flow и циклических зависимостей сборок, DI создаёт правильное количество машин.
- [ ] Обновить статусы исходной страницы Wiki отдельным разделом с датой реализации и ссылками на фактический код. Не стирать историческое описание находок и не объявлять исключённые классы автоматически перенесёнными.
- [ ] Представить результат по этапам и проверки; не включать чужие незакоммиченные изменения в commit.

## Самопроверка плана

Все 9 основных пунктов и спорный CubeFlightAnimator учтены: рулетка отмечена выполненной; восемь оставшихся находок покрываются семью предметными миграциями, поскольку FillRescueFlow объединён с FillUiPresenter; куб добавляет восьмой этап. Нет повторной миграции готовой рулетки и нет миграции явно исключённых классов. У каждого этапа указаны источники, целевые состояния, сигналы, правила отмены и наблюдаемые проверки. Общие окна/реклама идут перед потребителями; полёт — перед заливкой, а UI Fill — после зависимостей. Неоднозначности текущего поведения (нетегированные SDK-события, CanRescue после остановки, restart Timer, частичный leaderboard error) явно описаны и не маскируются архитектурным переносом.
