# Прогресс ревью кода MadSlime (временный файл)

Задача: полное ревью всего СВОЕГО кода проекта. Только находки, БЕЗ правок кода.
При сжатии контекста: перечитать `Wiki/Ревью/Критерии оценки кода.md` (55 критериев) и этот файл.

## Правила задачи
- Формат находки: файл, класс, строка (1-based), в чём проблема, номер критерия (1–55), серьёзность.
- Итоговый отчёт: `Wiki/Ревью/` (файл вида «Отчёт ревью кода.md»).
- В отчёте также заполнить сводную таблицу 55 критериев (Соответствует / Не соответствует / Не применимо).
- Макс 2 сабагента. Используются: (1) форк skill code-review (фоновый), (2) general-purpose агент.
- Область: Assets/MadSlime + tools/ + корневые скрипты. Чужие ассеты — вне области (отметить в отчёте).
- Язык отчёта: русский, простыми словами. Код в находках НЕ предлагать.

## Критерии (кратко, для ориентира)
1–8 наименование (camelCase/PascalCase, классы, On-обработчики, -ing/-ed, нет имени класса в методе, глаголы, суть действия, поля)
9–24 общие правила (длина строк, 1 файл = 1 класс, namespace по папкам, краткие свойства, get/set, System.Action + event, Action/Func/Predicate, var, нет оверинжиниринга, понятные циклы, нет дублей, нет магических чисел, ref/out, нет расчётов в тексте, подписка в OnEnable/OnDisable или Awake/Start/OnDestroy, скрипты в папке Scripts)
25–34 SOLID (+анти-признаки), 35 YAGNI, 36 DRY, 37–38 KISS, 39 FSM, 40–41 Entry Point, 42–43 Factory
44–55 общие замечания (нет Debug.Log, свободные замечания, NullRef, if/else вместо switch, неиспользуемые поля/свойства/события/методы, Try без bool, new объект задержки каждый кадр, неиспользуемые using, оптимальные if/else)

## Опись и разделение работы
Свой код: Assets/MadSlime — 253 .cs, 26 395 строк (папка Scripts по модулям + ResponsiveUI). Плюс tools/verification/ (3 .cs + 2 .sh), setup-assets.sh/.cmd, Assets/WebGLTemplates/YandexGames/index.html + style.css.
PlayerInputActions.cs (367 строк) — сгенерирован Input System → «Не применимо».
Чужие ассеты вне ревью: Epic Toon FX, TextMesh Pro, QuickOutline, PluginYourGames, 300Mind, amusedART, ithappy и пр.

- Сабагент 1 (форк code-review, фон): методика code-review по всему проекту. В конце: сверить его находки, проверить не оставил ли он файлов-отчётов в репо.
- Сабагент 2 (general-purpose): Scripts/UI/** + Scripts/Roulette/** + Scripts/Audio/** + Scripts/Saves/** + ResponsiveUI/** (~110 файлов, ~11.5 тыс. строк). Формат находки: F# | файл | класс | строка | цитата | проблема | критерий | серьёзность.
- Я: остальное — Ads, Collectables, Camera, DI, Economy, Game, Input, Items, Levels, Localization, Player, Quota, ShapeFill, Skins, Shop, Upgrades, Leaderboards (~143 файла) + tools/ + setup-assets + WebGL-шаблон + глобальные grep-проверки (Debug.Log, строки >120 симв., >1 класса в файле, namespace, new WaitForSeconds, Find*/Camera.main, свои delegate).

## Находки (пополняется)

### Модуль Game (моё ревью)
- G1 (36 DRY, КРУПНО) GameplayOrchestrator.cs:419–432 `FinishGame` дублирует разборку из `FinishSession` (306–333): StopMotion, Stop таймера, StopCollecting, growthAnimator.Stop, ReportStop, DisableFinalCountdownEffects, SessionFinished — две ветки, риск расхождения.
- G2 (25 SRP, КРУПНО) SceneNavigator.cs (509 строк): грузит/выгружает сцены + управляет экраном загрузки (Show/Hide/прогресс, 346–387) + реестр сессий + readiness сейвов + EventSystem-переключение.
- G3 (11, КРУПНО-системно) namespace не повторяет иерархию папок: Game/Logic/GameplayOrchestrator = `Game`; Collectables/Data/AttractConfig = `Skills`; Game/Data/YandexConfig = `Scriptables`; */Adapters = `Adapters`; Camera/... = `CameraSystem`. Стиль единый (модуль без подпространств Logic/Data/View), но критерий «namespace по папкам» не соблюдён. Оформить одной системной находкой.
- G4 (50, КРУПНО) Timer.cs:18 событие `Stopped` — подписчиков нет во всём Scripts (grep); вызывается в никуда.
- G5 (46, МЕЛОЧЬ) GameplayOrchestrator: Awake (115–176) проверяет 8 зависимостей, а QuotaCounter — только в OnEnable (181–186); бросок в OnEnable ведёт к OnDisable→FinishSession с null-полями → каскад NRE.
- G6 (39/37, МЕЛОЧЬ) GameplayOrchestrator.RunSession:271 меняет `_state` напрямую мимо TryTransitTo — переходы FSM непоследовательны.
- G7 (8, МЕЛОЧЬ) GameplayOrchestrator:55 `Movement.Movement _movement` — класс Movement в namespace Movement, дублирование в имени.
- G8 (45, МЕЛОЧЬ) Pauser.cs:47–55 Update каждый кадр опрашивает `_adsService.IsPauseGame` вместо подписки на событие.
- G9 (46, МЕЛОЧЬ) GenericOverlapDetector.cs:86–92 и 102–111 — `Exited?.Invoke(target)` без null-проверки уничтоженных объектов (в CameraObstacleDetector.OnDisable:89–95 проверка есть) — несогласованность, риск NRE у подписчиков.
- G10 (46, КРУПНО) ItemAttractor.cs:93 `attractable.Definition.Tier` без проверки Definition==null; ItemCollector.cs:110–115 тот же случай обрабатывает явной ошибкой. Несогласованность → NRE вместо понятного сообщения.

### Модуль Player (моё ревью)
- P1 (49, МЕЛОЧЬ) CrawlAnimator.cs:22 свойство `Frequency` — не используется нигде (grep).
- P2 (49, МЕЛОЧЬ) ModelDeformer.cs:30 свойство `Compression` — не используется.
- P3 (49, МЕЛОЧЬ) Timer.cs:21 свойство `Duration` — не используется (плюс G4 про Stopped).
- P4 (49, МЕЛОЧЬ) SceneNavigator.cs:36 свойство `PreviousSceneId` — не используется (поле _previousSceneId только пишется).
- P5 (36 DRY, МЕЛОЧЬ) TierResolver.cs:52–124 три одинаковых линейных поиска (GetSpeedFor/GetScaleFor/GetCameraOffsetFor) — дублирование; при этом fallback разные: GetSpeedFor возвращает скорость ПЕРВОЙ строки таблицы (62), остальные 1f — тихий неверный результат, если тира нет в таблице.
- P6 (8/стиль, МЕЛОЧЬ) PlayerTier.cs:19 приватное поле _currentTier объявлено между публичными свойствами Mass/CurrentTier.
- P7 (45, МЕЛОЧЬ) Movement.cs:147–158 ConvertToWorldDirection нормализует константы Vector3.forward/right каждый кадр.
- P8 (45, МЕЛОЧЬ) PlayerScaler/TierResolver: не проверяется ScaleMultiplier > 0 в порогах (TierResolver.Awake проверяет только непустоту) — при 0 слайм схлопнется.
- Масса: PlayerConfig, TierLookup, GhostAnimator, ScalePunch, FinalCountdownVignette, GrowthAnimator, SpeedSmoke, TierUpFx, Rotator, Mover, Clamper, CollectAvailability, MassGainer — чисто, паттерн подписок OnEnable/OnDisable соблюдён.

### Модуль Levels (моё ревью)
- L1 (49, МЕЛОЧЬ) ZoneLayoutPlanner.cs:28 свойство `Positions` — не используется.
- L2 (51, КРУПНО) ItemPropFactory.cs:32–53 GeneratePreparedSet, :313–319 RebuildPreparedIcons, :321–344 RebuildPreparedIcon, :346–382 ValidateIcon — публичные методы без единого вызова в проекте (grep, включая tools) — ~90 строк мёртвого кода.
- L3 (47, МЕЛОЧЬ) LayoutPreviewDrawer.cs:313–341 GetTierColor — if-цепочка по значениям enum вместо switch.
- L4 (10, МЕЛОЧЬ) PropSet.cs:9–38 два класса в файле (PropSet + PropVariant).
- L5 (54/стиль, МЕЛОЧЬ) SpawnZone.cs:6 `[System.Serializable]` полным именем без using System.
- L6 (45, МЕЛОЧЬ) LevelRange — нет валидации FromLevel ≤ ToLevel; при ошибке резолвер молча отдаёт последний диапазон (LevelConfigResolver.cs:59).
- L7 (45, МЕЛОЧЬ) LevelPlacementPlanner.cs:55 — при MinTier > MaxTier цикл пуст, ошибка «no zones match…» (95) вводит в заблуждение.
- L8 (13, МЕЛОЧЬ) LevelItemsSpawner.cs:23 SpawnedCounts отдаёт мутабельный Dictionary.
- L9 (45, МЕЛОЧЬ) LevelGenerator.ApplyTheme (153–163) молча пропускает absent FloorMaterial, но LevelSurface.Apply (26–29) бросает исключение при absent Background/Edge — несогласованная обработка конфига темы.
- Качество: планировщик расстановки/occupancy/бюджет кадра — хорошо; критерий 18 (понятные циклы) соблюдён.

### ItemPropFactory (Editor, 887 строк)
- PF1 (25 SRP, КРУПНО, но Editor — мягче) класс совмещает GUI, рендер иконок, запечку определений, создание префабов, запись PropSet, persist состояния окна.
- PF2 (20, МЕЛОЧЬ) ResolveFreeLayer:668–679 магические 9 и 31 (границы слоёв).
- PF3 (45, МЕЛОЧЬ) BakeJobDefinitions:445 `enumValueIndex = (int)entry.Tier` — индекс вместо значения enum, хрупко при перестройке SizeTier.
- PF4 (45, МЕЛОЧЬ) WritePropSet/WriteItemFields — строковые имена полей («_props», «_definition»…) в SerializedObject: переименование сломает тихо.
- PF5 (36, МЕЛОЧЬ) RebuildPreparedIcon дублирует RenderIcon (render → png → import).
- PF6 (45, МЕЛОЧЬ) GeneratePreparedSet создаёт временный EditorWindow как сервис ради Generate() — обходной приём.
- Длинные строки: 15 шт. (см. M2).

### Модуль Quota (моё ревью)
- Q1 (45, МЕЛОЧЬ) QuotaGenerator.cs:77 `pair.Key.Tier <= SizeTier.Giant` — правило «Boss не в квоте» зашито магическим литералом, без имени/комментария.
- Q2 (8/стиль, МЕЛОЧЬ) QuotaItemHighlighter.cs:81 `item == null` vs :99 `ReferenceEquals(item, null)` — разные проверки null в одном файле.
- Q3 (45, МЕЛОЧЬ) SmellAbilityActivator.Awake не проверяет _detector/_outlineInitializer (в проекте принято проверять все инъекции).
- Q4 (45, МЕЛОЧЬ) ProfilerMarker инстанс-поля (QuotaItemHighlighter.cs:15–17, QuotaOutlineInitializer.cs:15–16) — рекомендуется static (то же, что C2).
- QuotaEntry/QuotaBoard/QuotaCounter/QuotaOutlineInitializer/QuotaTrackedItem/QuotaTypeState — чисто, Attach/Detach симметричны.

### Модуль Camera (моё ревью)
- K1 (36 DRY, КРУПНО) CameraPullAnimator / CameraShakeAnimator / CameraFovKickAnimator — трижды скопирован один паттерн (поле-импульс + KillRecoverTween + Read/Apply + tween-возврат, ~90 строк каждый).
- K2 (11, дополнение G3) LookAtCamera.cs — файл Camera/View, namespace `UI`; CameraSystem для остальных.
- K3 (стиль, МЕЛОЧЬ) LookAtCamera.cs:1 ведущий пробел перед using.
- K4 (стиль, МЕЛОЧЬ) CameraConfig.cs:28,31,34,37,57,60 — атрибуты [Tooltip]/[SerializeField] на одной строке с полем, в остальном файле — раздельно.
- K5 (45, МЕЛОЧЬ) Несогласованные Awake-проверки: CameraFovKickAnimator не проверяет _playerTier; CameraPullAnimator не проверяет _itemCollector/_playerTier; CameraShakeAnimator не проверяет _itemCollector.
- CameraFollow — чисто (StopMotion необратим, но сцена всегда новая — допустимо).

### Уточнения механических проверок
- M1 уточнение: в РАНТАЙМ-коде Debug.Log нет вообще (все 36 — Editor-фабрики + tools + комментарии генерённого файла) → критерий 44 «Соответствует» с оговоркой.
- G4/P3: событие Timer.Stopped и свойство Timer.Duration мертвы (grep по всем Scripts).
- C1 (45/стиль, МЕЛОЧЬ) ItemCollector.cs:139 битые отступы (строка с лишними пробелами).
- C2 (45, МЕЛОЧЬ) GenericOverlapDetector.cs:32–39 ProfilerMarker создаются на экземпляр в Awake, рекомендуется static readonly.
- C3 (45, НЕ УВЕРЕН) ItemAttractor продолжает тянуть предмет во время поглощения Absorber'ом (оба пишут transform.position) — лишняя работа/возможный джиттер.
- C4 (45, МЕЛОЧЬ) Несогласованные проверки: CameraObstacleDetector не имеет Awake-валидации инъекций (соседи имеют); SetRadius: GenericOverlapDetector:115 допускает 0, CameraObstacleDetector:103 требует >0.

### Модуль ShapeFill (моё ревью, 14 файлов)
- S1 (45, МЕЛОЧЬ) FillResultCalculator.cs — Awake-валидации _quotaCounter нет (в проекте принято проверять все инъекции; cf. Q3).
- S2 (12, МЕЛОЧЬ) FillResult.cs:8–28 — неизменяемый DTO на бэкинг-полях и свойствах с телами вместо кратких `{ get; }`.
- S3 (стиль/45, МЕЛОЧЬ, системная) Раскол стиля внутри ShapeFill: ShapeFiller.cs:289,341; GridBuilder.cs:55,94–95,106–112,118–119; CubeSpawnSchedule.cs:9–12; GridCameraFitter.cs:24–25,32–33,57–58,122–123 — if без фигурных скобок, в остальных модулях проекта — Allman со скобками.
- S4 (45, МЕЛОЧЬ) GridBuilder.ReadPixels — GetPixels всей текстуры (при 1024² ~16 МБ мусора) вместо чтения только ячеек сетки; одноразовый пик GC на подготовке сцены (WebGL/Яндекс).
- S5 (45/46, МЕЛОЧЬ) FillSceneOrchestrator.cs:159 — `_sceneNavigator.UnregisterSession` в OnDisable без null-проверки, при этом _fillOrchestrator на 166-й проверяется; несимметричная защита после возможного падения Awake.
- S6 (36, МЕЛОЧЬ) FillSceneOrchestrator.cs:225–228 и 230–233 — LoadNextLevel и RestartLevel: тела идентичны (обе → SceneId.Game).
- S7 (34, КРУПНО) GameplayOrchestrator.cs:80–86 — Construct принимает 23 зависимости; FillSceneOrchestrator.cs:57–61 — 14. Признак Anti-DIP: в класс передано слишком много зависимостей, логика размывается.
- Чисто: FillConfig (все поля с Tooltip/Min/Range), FillTapButton, CubeSpawner (static s_colorId, fail-fast), CubeSpawnSchedule, GridCameraFitter (пере-фит при смене аспекта с сохранённым FOV — грамотно), CubeFlightAnimator (ручной FSM-полёт, DOKill+SetLink), FillCompleteAnimator (токены отмены, OCE пойман), FlyingCube (маркер-класс — приемлемо), ShapeFillOrchestrator (тонкий фасад). Prepare() без параметров извне не вызывается, но используется перегрузкой Prepare(Bounds) — не мёртвый код. FillCompleteAnimator.Play() внешних вызовов не имеет — вызывается из OnFillCompleted.

### Модули Ads / DI / Economy / Input (моё ревью)
- A1 (46/45, МЕЛОЧЬ) ProjectLifetimeScope.cs:93 — `_saveConfirmation` не валидируется перед `new Yg2SavesAccess(_saveConfirmation)`, хотя остальные 9 полей файла проверяются (32–91).
- A2 (19/36, МЕЛОЧЬ) Три одинаковые копии приватного ValidateAssigned: GameLifetimeScope.cs:181–188, MenuLifetimeScope.cs:78–85, FillLifetimeScope.cs:85–92; ProjectLifetimeScope те же проверки пишет вручную (32–91).
- A3 (33 DIP, МЕЛОЧЬ) MenuLifetimeScope.cs:56 — `resolver.Resolve<Saver>()` — зависимость от конкретного класса (не интерфейса) ради ExecuteTransaction; Action<Action> уже зарегистрирован глобально (ProjectLifetimeScope.cs:100).
- A4 (стиль, МЕЛОЧЬ) Rewarder.cs:21 — поле _isWin объявлено между свойствами GrantedTotal/IsWin (как P6).
- A5 (46/45, МЕЛОЧЬ) Wallet.cs — нет Awake-проверки _balanceStorage (дисциплина проекта).
- Чисто: IAdsService, Yg2AdsService (Dispose симметричен конструктору), AdScheduler (событийная схема, правильные типы исключений), SceneButtonSoundInjector, RewardConfig (валидация + OverflowException), Rewarder (двойной commit пойман, rescue доплачивает разницу), PlayerInputReader, TouchJoystick (валидация Awake, OnDisable→EndDrag).
- Дополнение к G3/F5 (namespace≠папки): Ads/Logic/AdScheduler → `Game`; Economy/{Data,Logic} → `Game`; Input/** → `PlayerInput`; RewardConfig (Economy/Data) → `Scriptables`.
- Сабагент 2 завершён: 40 находок (7 КРУПНО, 33 МЕЛОЧЬ) — полный список в output/review-subagent-ui.md (F1–F40), сверка номеров строк — отдельным проходом.

### Модули Localization / Leaderboards / Upgrades (моё ревью)
- LC1 (10, МЕЛОЧЬ) LocalizationTable.cs:31 — второй класс LocaleEntry в файле (M3 подтверждён).
- LC2 (20, МЕЛОЧЬ) LocalizationTable.cs:50,55 — литералы "en"/"tr" вместо констант Localization.English/Turkish (Localization.cs:9–11); коды языков размазаны по файлам.
- LC3 (46, МЕЛОЧЬ) LocalizationService.cs:61 — OnDisable: `_languageProvider.LanguageSwitched -=` без null-проверки, хотя _savesReadiness на 56-й проверяется; при броске в OnEnable (39–43) OnDisable добьёт NRE поверх исходной ошибки.
- LB1 (45/46, МЕЛОЧЬ) LeaderboardReporter.cs:24–45 — Awake проверяет _config/_leaderboardService, но не _storage.
- U1 (10, МЕЛОЧЬ) UpgradesConfig.cs — три класса в файле (UpgradesConfig + UpgradeEntry + PerkEntry) (M3 подтверждён).
- U2 (37/45, МЕЛОЧЬ) PlayerUpgrades.cs:85 и 148 — `GetPerkCost(...)`/`GetPerk(...)` вызываются ради исключения, результат не используется, без комментария (тот же приём, что F30).
- Нормализация к F4: несколько DTO-классов в одном SO-файле — системная МЕЛОЧЬ по критерию 10 (RouletteConfig, UpgradesConfig, LocalizationTable, PropSet); в итоговом отчёте уравнять серьёзность.
- Чисто: ILanguageProvider (add/remove-адаптер к YG2 — грамотно), Yg2LanguageProvider, Localization (static-фасад с s_-статикой), CollectedItemsCounter (ctor-подписка + Dispose-отписка и коммит), LeaderboardData (nullable struct), Yg2LeaderboardService (таймаут 8с realtime, linked CTS, checked-инкремент, editor-симуляция — образцово), AdrenalineBoost, PerkType/UpgradeType.

### Skins/Shop, SkinItemFactory, tools, шаблон (моё ревью)
- SK1 (10, МЕЛОЧЬ) SkinRarityTable.cs:17 — второй класс RaritySettings в файле (M3 подтверждён).
- SK2 (45/46, МЕЛОЧЬ) SkinApplier.cs:33–46 — Awake проверяет только сериализованные поля, инъекции не проверяются; при незавершённой инъекции NRE в OnEnable:50.
- К F17 (дополнение): SkinInventory.cs:8 — константа InitialSkinId="Slime"; SavesYG.cs:19 дублирует литерал (обратная зависимость невозможна — Skins зависит от Saves; отметить как замечание).
- SF1 (45, МЕЛОЧЬ) SkinItemFactory.cs:183 — enumValueIndex=(int)_rarity (родственник PF3); :181–184 — строковые имена полей SerializedObject (родственник PF4).
- T1 (45, МЕЛОЧЬ) tools/verification/run-progression-fill.sh:34 — batchmode на ОСНОВНОМ проекте; редактор обычно открыт → конфликт блокировки проекта. run-fill-camera.sh решает это копией проекта (строки 8–16) — несогласованность.
- T2 (пометка) tools/*.cs — инструментальные, вне Assets, без namespace, стиль (var, однострочные блоки) отличается от продуктового кода; критерии 11/24 к ним не применимы, к остальным — мягко.
- WebGL-шаблон (index.html/style.css) — производная стандартного шаблона PluginYourGames; критичных проблем нет; критерии C# не применимы. setup-assets.sh/.cmd, run-*.sh — чисто (set -euo pipefail, аккуратная очистка).
- Чисто: SkinItem, SkinRarity, SkinRarityLookup, SkinInventory (guard-логика), SkinModel (маркер), ShopContent, PreviewRotator, ModelPlacer (именованные константы, аккуратный фит камеры), SkinItemFactory в целом (константы, persist состояния, command-line вход).
- Подтверждено от сабагента 1: AttractConfig.cs:23 OrbitStrength — свойство + поле, ноль чтений в проекте (grep), ассет задаёт 0.25 впустую → критерий 49, КРУПНО.

### Механические проверки (grep по всей своей части)
- M1 (44) Debug.Log: 36 вхождений; почти все в Editor-фабриках и tools. Рантайм уточняется отдельным grep.
- M2 (9) Строки >120 симв.: 210 шт.; топ: ItemPropFactory 15, PlayerInputActions 13 (генерированная), SkinItemFactory 10, RouletteConfigValidator 6, ModelPlacer 5, ShapeFiller 5, FillSceneOrchestrator 5, PlayerConfig 5, GameplayOrchestrator 5.
- M3 (10) Файлы с >1 типом: 18 кандидатов — проверить вручную (ResponsiveCanvasLayout 4, RouletteConfig 3, UpgradesConfig 3, остальные по 2; PlayerInputActions — генерированный, не в счёт).
- M4 (15) Своих delegate нет — критерий соблюден. new WaitForSeconds — нет (53 соблюден). namespace отсутствует — нет (11 частично, см. G3).
- События GameplayOrchestrator (SceneSessionPrepared/Started, GameplayStarted, SessionFinished) используются в GameplayHud — ок.
- PlayerInputActions.cs — auto-generated InputActionCodeGenerator 1.14.2 → «Не применимо» почти ко всем критериям.

## Статус
- [x] Критерии прочитаны
- [x] Память записана
- [x] Инвентаризация файлов
- [x] Разделение работы
- [x] Сабагент 2 отправлен (завершён: F1–F40, отчёт в output/review-subagent-ui.md, строки сверены)
- [x] Моё ревью своей части (все модули + tools + setup-скрипты + WebGL-шаблон)
- [x] Результаты сабагентов проверены (форк code-review: 10 находок в output/code-review-2026-10-26.json, все сверены по строкам; 4 кандидата сняты им же)
- [x] Отчёт записан в Wiki/Ревью/Отчёт ревью кода.md
- [x] Посторонних файлов в репо нет (сабагенты писали только в output/)
