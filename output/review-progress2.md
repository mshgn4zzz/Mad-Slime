# Прогресс ревью раунд 2 (временный файл, 2026-10-27)

Задача: повторное жёсткое ревью после исправлений. Только находки, БЕЗ правок. Отчёт перезаписать в `Wiki/Ревью/Отчёт ревью кода.md` в формате «критерий → класс "строка" — ошибка». Правки раунда 1 не закоммичены (дерево против e7ecadab); журнал правок прошлой сессии — `output/fix-review-progress.md` (L6 закрыт LevelsCatalog.OnValidate + throw в резолвере; C3 «притягивание при поглощении» осознанно не правилось — оставить «не уверен»; RoulettePresenter:341 Debug.LogWarning — осознанный лог вместо попапа при отмене dwell).

## Агенты (макс 2, effort high)
1. Форк skill code-review — весь проект + перепроверка 10 старых находок → output/code-review-2026-10-27.json. ЗАПУЩЕН, ждём.
2. General-purpose UI/Roulette/Audio/Saves/ResponsiveUI — ЗАВЕРШЁН: F1–F40 вердикты (34 FIXED / 2 STILL / 4 MOVED) + новые N1–N29. Ключевые сверены мной: N1 ✓, N6 ✓, N7 ✓ (MenuContent:119 берёт RouletteModel, обёртка презентера :76 мертва), N8 ✓, N9 ✓, N10 ✓, N18 ✓, N22 ✓, F7 ✓.

## Вердикты по моим старым находкам (раунд 1)
- G1 FIXED (FinishSession+TeardownSession, GameplayOrchestrator:270–307)
- G2 ЧАСТИЧНО (SceneNavigator 469 строк; LoadingScreenController+SceneSessionRegistry вынесены, но класс всё ещё: сцены+сессии+readiness+EventSystem+скин+ResetProgress) — оставить со смягчением
- G3 STILL-переформулировать (UI.* теперь иерархический, геймплей — плоский модуль: несогласованная стратегия; остались Economy→Game, RewardConfig/YandexConfig→Scriptables, SavesYG→YG — последний с комментарием-оправданием)
- G4 FIXED, G5 FIXED (Awake:145), G6 FIXED (TryTransitTo:374), G7 FIXED, G8 FIXED (события)
- G9 STILL (GenericOverlapDetector.cs:85,106 — Exited?.Invoke без проверки уничтоженных; в CameraObstacleDetector проверки есть)
- G10 FIXED (ItemAttractor.cs:91 проверка Definition)
- C1 FIXED; C2 СМЯГЧЁН (static есть, readonly нет — GenericOverlapDetector.cs:20–23); C3 «не уверен» (не правилось осознанно); C4 СМЯГЧЁН (оба SetRadius >0 согласованы, но тела дословно дублируются: GenericOverlapDetector.cs:113 / CameraObstacleDetector.cs:118)
- P1–P4 FIXED (мертвые Frequency/Compression/Duration/PreviousSceneId удалены)
- P5 FIXED (GetThreshold единый + throw вместо тихого fallback + ValidateScale)
- P6 FIXED; P7 FIXED (s_forward/s_right static); P8 FIXED (валидация PlayerConfig:126 + TierResolver:124)
- L1–L5, L7, L8 FIXED; L6 FIXED (OnValidate+throw, см. fix-review-progress); L9 STILL (LevelGenerator.cs:157 и FillSceneOrchestrator.cs:372 молча return при Theme==null)
- PF1 СНЯТЬ (split на 4 partial); PF2 FIXED (константы слоёв); PF3 FIXED (enumValueIndex исчез); PF4 СМЯГЧЁН (именованные константы полей Items/Editor/ItemPropFactory.cs:20–28 — Reflection-by-name остался, природа SerializedObject); PF5, PF6 FIXED (методы удалены)
- Q1 FIXED (LargestQuotaTier const:14); Q2 FIXED (все `== null`); Q3 FIXED (полная валидация); Q4 СМЯГЧЁН (static без readonly: QuotaItemHighlighter.cs:12–14, QuotaOutlineInitializer.cs:13–14)
- K1 FIXED (CameraImpulseAnimator базовый, три аниматора наследуют); K2–K5 FIXED
- S1 FIXED; S2 FIXED ({ get; }+валидация; новое: DTO лежит в ShapeFill/Logic вместо Data); S3 FIXED; S4 FIXED (пиксельное чтение по ячейкам); S5 FIXED; S6 FIXED (NavigateTo(SceneId)); S7 ЧАСТИЧНО (GameplayOrchestrator 19 параметров :73, FillSceneOrchestrator 12 :54 — оставить с новыми числами)
- A1–A5 FIXED (ValidatedLifetimeScope, SavesTransaction, Wallet/Rewarder Awake)
- LC1–LC3 FIXED; LB1 FIXED (полный Awake :22–46); U1, U2 FIXED; SK1, SK2 FIXED; SF1 FIXED (IdField const, enumValueIndex нет); T1 FIXED (копия проекта verification_dir)
- M1 по-прежнему чисто; M2 почти FIXED (в Scripts 13 строк >120 — список получен; +8 в ResponsiveUI по агенту 2); M3 по сути FIXED (остались только class+nested enum/private class — допустимо)

## Новые находки моего прохода (по актуальному коду)
1. Timer.cs:39 — сообщение «Timer.Start called before Setup», метод называется StartCount (45/8)
2. GameplayOrchestrator.cs:34–36 — сбитые отступы блока [Tooltip]/[SerializeField] _finalCountdownSeconds (50)
3. GameplayOrchestrator.cs:102–165 — Awake проверяет 10 из 19 зависимостей (_timer, _inputReader, _pauser, _movement, _playerTier, _playerScaler, _smellAbility, _growthAnimator, _cameraFollow не проверяются) (46)
4. GameplayOrchestrator.cs:354–372 — OnTimeOut и OnQuotaCompleted идентичные тела (36)
5. GameplayOrchestrator.cs:420–442 — NavigateToMenu/NavigateToFill близнецы; тихий return при IsTransitioning (36/46)
6. SceneNavigator.cs:29–32 — блок приватных полей зависимостей объявлен после свойств, разрывает структуру (45)
7. SceneNavigator.cs:116 — WaitUntil(IsSaveReady) без таймаута: сейвы не готовы → вечный экран загрузки; в TransitionAsync таймаут 10с есть (46) — КРУПНЕЕ МЕЛОЧИ, отметить
8. SceneNavigator.cs:396–467 — GetSceneName и TryResolveSceneId: два switch по одним строкам "Menu"/"Game"/"Fill" (36)
9. SceneNavigator.cs:124 vs :324 — sessionStart.Invoke() без проверки vs sessionStart?.Invoke() (50)
10. SceneNavigator.cs:146–183 — ResetProgressAndLoadMenuAsync: ResetProgress() ДО ConfirmSavedAsync — при отмене прогресс стёрт, сцена не сменилась (46, риск)
11. GameplayActivityController.cs:21–61 — 7 одинаковых if-ArgumentNullException; FinalCountdownEffects.cs:17–39 — 4; FillSessionSaver.cs:23–46 — 5 (36, системно: конструкторная валидация копипастой)
12. Movement.cs:11–12 — s_forward/s_right без readonly и дублируют Vector3.forward/right (13/51)
13. Movement.cs:24–28 — событие SpeedChanged внутри блока полей (45); Awake:41–69 не проверяет _tierResolver/_upgrades/_inputReader (46)
14. CrawlAnimator.cs:53 — параметр SetUrgency(float progress) на деле urgency (8)
15. Timer.cs:81 — зависимость от Pauser через Time.timeScale > 0f — скрытая связность (23/46)
16. ShapeFiller.cs:42–45 — события между методами (45); :125–172 трижды дублируется создание fill-CTS+запуск (36); :197–235 StopFill не отменяет летящие кубы — OnCubeArrived добьёт _arrivedCount и может выстрелить FillCompleted после Stop (53); :333–344 UniTask.Delay бонусной волны внутри цикла спавна с условием _fillIndex == _quotaTarget (37/55); три pump-цикла SpawnBorderCascade/FillAsync/CompleteInstantly однотипны (36)
17. FillSceneOrchestrator.cs:305–323 — OnFillCompleted(float percent): percent не используется (49/14); :200–208 ExitToMenuAfterWin ≡ ExitToMenu (36); Awake:73–128 проверяет 8 из 12 зависимостей (46)
18. ItemPropFactory.cs:58–67 — HelpBox: двойные пробелы («layer-isolated  », «bakes  », «BoxCollider  ») и хаос отступов продолжений (50)
19. QuotaGenerator.cs:51–58 — второй добирающий цикл игнорирует QuotaMaxSameTier без комментария (55)
20. СИСТЕМНО (50, подтверждено агентом 2 как N14): аномально глубокие отступы продолжений сообщений исключений, ~28 файлов: TierResolver.cs:27–28, SmellAbilityActivator.cs:40–41, Rewarder.cs:34–35, Movement.cs:58–59, CrawlAnimator.cs:41–42, FillSceneOrchestrator.cs:78–79, LevelGenerator.cs:61–62 + список агента (LeaderboardMenu.cs:71, LanguageSwitcher.cs:382, GameplayUIFabric.cs:29,50, UiSpawner.cs:17,88, PauseMenu.cs:46,57, GrowthBarView.cs:36,43, FillProgressUI.cs:30, SkinRarityGroupView.cs:25, ShopItemViewFactory.cs:24, UpgradeItemViewFactory.cs:23, AudioPlayer.cs:190, RouletteConfigValidator.cs:76,108,143, RouletteFactory.cs:56, SkinShowcase.cs:215, AdrenalineSound.cs:25, FillCompleteSound.cs:22, FlyingCubeArrivalSound.cs:28, PlayerPickupSound.cs:30,43, TierUpSound.cs:26,33, TimerTickSound.cs:23, UIButtonSound.cs:25, RouletteWinPopup.cs:284, RouletteWheel.cs:178, PlayerDataResetTool.cs:69,128)
21. LocaleEntry.cs, RaritySettings.cs, RouletteSector.cs — тела без отступов внутри namespace/класса (50); RouletteSector.cs:19–23 [Tooltip] оторван от поля пустой строкой; поле _rewardKind vs свойство RewardType (8)
22. FillResult.cs — DTO в ShapeFill/Logic вместо Data (структура/2)
23. LoadingScreenController.cs:34–40 — при отсутствии LoadingScreenView исключение ПОСЛЕ Instantiate: канвас-заглушка остаётся (46)
24. GridBuilder.cs:66–78 — два одинаковых foreach min/max (36)
25. PlayerInputReader.cs:42 — своё свойство MoveInput вместо поля _moveInput внутри класса (50, микро)
26. M2-остаток: 13 строк >120 в Scripts (Pauser.cs:26, ItemPropFactory.Assets.cs:177, Yg2LeaderboardService.cs:122, CubeSpawner.cs:19, Clamper.cs:43, GameplayOrchestrator.cs:73, MenuContent.cs:59, QuotaGenerator.cs:27, TouchJoystick.cs:30, FillConfig.cs:62, RoulettePresenter.cs:406, FillUiPresenter.cs:138,150)

## Из агента 2 — принять как есть (сверены выборочно)
F7 STILL (GameplayUIFabric.cs:60 OnDisable без Resume — с оговоркой «при уничтожении объекта»), F16 STILL (SavesYG именование, контракт), F5/F13/F14/F33 MOVED (актуальные строки в ответе агента), N1–N29 (N18 — single loop-source, НЕ УВЕРЕН мягко; N21 — ResponsiveUI вне Scripts, критерий 24; N22 — InitialSkinId дублируется SavesYG.cs:10 + SkinInventory.cs:7; N10 — Release<T> дублируется MenuNavigation.cs:330 / MenuRouletteFlow.cs:172; N12/N13 — конструкторы/Display по 9 параметров, 37)

## Статус
- [x] Агент 2 завершён и принят
- [x] Сверка моих старых находок
- [x] Мой свежий проход
- [x] Агент 1 завершён: output/code-review-2026-10-27.json (10 старых: 9 fixed/moved-fixed, 1 still — RoulettePresenter.ShowPrizeAsync onComplete при отмене; 11 новых, ключевые сверены: A1 гонка AdScheduler ✓, A3 Definition.Tier в CameraPull/Shake ✓, A4 Range на Vector2 ✓, A5 Try void ✓, A6 MaxFillPercent ✓, A7 usings ✓, A8 свет Euler(45,-30) vs (35,210) ✓, A11 ✓). N12 агента 2: строка 194 неверна, верная — RouletteModel.cs:27–29.
- [x] Отчёт перезаписан в Wiki/Ревью/Отчёт ревью кода.md (27.10.2026, формат «критерий → класс "строка"»)
- [x] После отчёта: computer-use, вкладка «орка», чат «Правки кода по отчёту…» (window_id 93959173977936) — сообщение отправлено 27.10.2026 тремя кусками по ≤100 символов (лимит stdin Hyprland) + Enter; агент начал обработку (Grooving…). Текст: файл обновился + ultrathink + строго по отчёту правки кода + git push.
