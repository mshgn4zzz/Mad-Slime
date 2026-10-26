# Правки по отчёту 2-й итерации (27.10.2026)

## A. Точечные мелкие
- [x] 8: CrawlAnimator SetUrgency(float progress → urgency)
- [x] 8: RouletteSector _rewardKind/RewardType разнобой (enum → RouletteRewardType.cs, asset rewardType)
- [x] 48: QuotaCounter MaxFillPercent удалить
- [x] 49: RouletteWheel IsSpinning удалить
- [x] 50: ReactiveUIView EnableCompleted (событие + вызов) удалить
- [x] 51: RoulettePresenter GetSpinPrice, HasAvailableSkin удалить
- [x] 52: AdScheduler TryShowInterstitial → bool
- [x] 54: unused using ×8 (GameLifetimeScope, MenuNavigation, MenuLifetimeScope, PlayerPickupSound, UiSpawner, ShopItemViewFactory, UpgradeItemViewFactory, LeaderboardRowViewFactory, LeaderboardMenu, PlayerDataResetTool)
- [x] 45: MusicChannel ArgumentException nameof
- [x] 45: ItemFactory 15 / ResponsiveSkinGrid 22 — System. краткое имя
- [x] 45: UiSpawner Destroy-обёртка удалить
- [x] 45: RouletteWinPopup 63 двойной пробел/конкатенация
- [x] 45: ItemPropFactory HelpBox двойные пробелы
- [x] 45: CameraConfig [Range] на Vector2 → OnValidate clamp
- [x] 45: MenuLabelFloat Mathf.Max дубль [Min]
- [x] 45: MenuRouletteFlow 56 / MenuPopupStack 30 bool-стиль
- [x] 45: RouletteSector Tooltip оторван
- [x] 45: LocaleEntry/RaritySettings/RouletteSector/PropVariant отступы
- [x] 45: ResponsiveUI editor s_ → _
- [x] 20: SaveConfirmation '|' → ResultSeparator

## B. Длинные строки (9)
- [x] 21 строка по списку отчёта (13 код + 8 ResponsiveUI); вне автогена PlayerInputActions >120 нет

## C. Дубли (19/36)
- [x] Release<T> дубль → в MenuPopupStack
- [x] SetRadius дубль → база OverlapRadiusDetector (Player.IRadiusRecipient)
- [x] OnTimeOut/OnQuotaCompleted → один OnSessionCompleted
- [x] NavigateToMenu/NavigateToFill → NavigateTo(SceneId)
- [x] FillSceneOrchestrator ExitToMenuAfterWin/ExitToMenu → ExitTo(SceneId)
- [x] GridBuilder min/max foreach → ExpandBounds
- [x] InitialSkinId дубль → SkinInventory.InitialSkinId
- [x] Editor-хелперы → ItemFactoryTools + IconRenderSettings (RenderToTexture с настройками света/камеры)
- [x] ShapeFiller: StartFillLoop (3 CTS) + PumpSpawnAsync (3 pump-цикла)

## D. KISS ShapeFiller (18/37)
- [x] бонусная волна вынесена: GetMainWaveTarget + SpawnBonusWaveAsync, IsBonusWaveStart удалён

## E. Поведенческие риски (44/45)
- [x] RoulettePresenter: onComplete при отмене приза, Debug.LogWarning удалён (п.44)
- [x] MenuRouletteFlow: null-чек рулетки в OnDailySpinCompleted/OnSkinSpinCompleted (при null → RefreshSkins(_rewardSkinId))
- [x] AdScheduler: флаг _isRewardedRequestRunning (ShowRewarded повтор → onRejected; сброс в Closed/Error)
- [x] SceneNavigator: WaitForSaveReadinessAsync с таймаутом SaveReadinessTimeoutSeconds=10
- [x] SceneNavigator: ConfirmSavedAsync ДО ResetProgress
- [x] Timer ↔ Pauser: комментарий-оправдание о связи через Time.timeScale
- [x] AudioVoicePool: MaxSourceCount=24 + FindSoonestSourceIndex при исчерпании
- [x] ShapeFiller StopFill: guard _isStopped в OnCubeArrived
- [x] LoadingScreenController: DestroyInstance() до throw
- [x] FillSceneOrchestrator: _isPreparing reset в catch перед rethrow
- [x] ApplyTheme: Theme == null → throw (LevelGenerator + LevelThemeApplier)
- [x] GenericOverlapDetector Exited: проверки уничтоженных уже есть (77–80, 98–101) — закрыто рефакторингом базы
- AudioPlayer loop (НЕ УВЕРЕН) — осознанный пропуск

## F. Переименования
- [x] 2: GameplayUIFabric → GameplayPauseFlow (git mv + .meta, DI, OnDisable: ResumeByRequest + Release меню паузы)
- [x] 11: ResponsiveCanvasLayoutEditor ns UI.ResponsiveUI (без суффикса Editor)
- [x] 24: ResponsiveUI → Assets/MadSlime/Scripts/UI/ResponsiveUI (git mv с .meta)

## G. SRP/DIP
- [x] 25: SceneNavigator → SceneTransitionRunner (LoadAdditiveAsync/UnloadOutgoingAsync + DisableOutgoingSystems/RestoreIncomingEventSystem)
- [x] 34: GameplayOrchestrator 19 → 14 (вынос GameplayPlayerSetup: PrepareSession/PrepareOutlineAsync/StopSession)
- [x] 34: FillSceneOrchestrator 12 → 11 (вынос FillExitFlow: Exit(SceneId) с интерстишелом + LevelThemeApplier: Apply)
- [x] 34/37: RouletteModel ctor 9 → RouletteModelDependencies; RouletteView.Display 9 → RouletteDisplayData

## H. Валидация (45/46)
- [x] ArgumentGuard.CheckNotNull (Game/Logic; UI ссылается на Gameplay)
- [x] GameplayActivityController 7 if / FinalCountdownEffects 4 / FillSessionSaver 5 → хелпер
- [x] GameplayOrchestrator Awake: +_playerSetup/_timer/_inputReader/_pauser
- [x] FillSceneOrchestrator Awake: +_themeApplier/_exitFlow/_musicPlayer/_rewarder/_pauser/_saver
- [x] Movement Awake: +_tierResolver/_upgrades/_inputReader
- [x] MenuContent: SkinModel null-чек в RefreshSkins и RefreshSkinPurchase
- [x] CameraPullAnimator/CameraShakeAnimator: Definition null (по образцу ItemAttractor)
- [x] BarFiller ctor bar; QuotaPlateUI _icon/_text; ShopItemView 4 поля; UpgradeItemView 5 полей + button; RouletteView 11 ссылок
- [x] MenuRouletteFlow/UpgradePurchaseModel/RouletteAccess/RouletteModel гварды (MenuPopupStack уже был)
- [x] QuotaGenerator 51-58: комментарий-оправдание про QuotaMaxSameTier
- [x] 209: отступы продолжений исключений — нормализатор (литерал +4, `+` +8 от throw), 71 файл

## Финал
- [x] компиляция 5/5
- [x] коммит + push
