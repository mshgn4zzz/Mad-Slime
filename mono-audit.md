# Аудит: где MonoBehaviour не нужен (сводный отчёт)

Дата: 29.10.2026. Ветка: `codex/fill-player-emission`.

Как считалось: два независимых поиска одного и того же вопроса, затем сведение.

- **Проход 1 (сабагент):** полное транзитивное дерево наследования всех MB-классов, греп/чтение каждого на lifecycle и instance-API, проверка guid по всем `*.unity / *.prefab / *.asset`, поиск `GetComponent`/`AddComponent`/`FindObjectOfType`/`new`.
- **Проход 2 (Клод):** триаж всех файлов тремя группами паттернов (lifecycle / instance-API / инспекторная сериализация), GUID-срез всех классов по Assets+Packages, транзитивное замыкание наследования в 3 раунда, разбор всех классов с refs=0 (кто их создаёт).

Расхождения по существу — два (Clamper, LeaderboardRowViewFactory, см. «спорные»); всё остальное — консенсус. Отчёты проходов: `mono-audit-main.md`, `mono-audit-agent.md` (удалены после сведения).

## Главный вывод

В игровом коде MadSlime **нет ни одного класса, который можно «спокойно» (без правки сцен/префабов/DI) перевести в обычный C# класс**: у каждого есть либо Unity-lifecycle, либо instance-API, либо инспекторная сериализация, либо компонентная привязка. Реальные кандидаты — два пустых класса-маркера (уход только с перепайкой префабов), плюс два спорных. Единственный формальный кандидат «ухода без перепайки» — статический хелпер в стороннем плагине YG2; править его смысла нет (обновление плагина сотрёт правку).

## Уровень A — уход безопасен

### MadSlime — пусто (консенсус)

### Сторонний код
- `Assets/PluginYourGames/Modules/Localization/Scripts/UtilsLang.cs` :: `UtilsLang` (нашёл сабагент, проверено Клодом): только статические методы, база `MonoBehaviour` не используется, guid `e5193d4a…` не встречается ни в одном файле проекта (0 ссылок в Assets+Packages). Уход = убрать `: MonoBehaviour` (строка 9), вызовы статические — не меняются (`UtilsLB.cs:79`, `LeaderboardYG.cs:117`, `GetPlayerYG.cs:67,69,81,83`). **Оговорка: сторонний плагин — правку перезатрёт обновление.**

## Уровень B — уход с перепайкой

### FlyingCube — консенсус
`Assets/MadSlime/Scripts/ShapeFill/View/FlyingCube.cs` — пустой класс-маркер (тела нет), никакого Unity-API, но компонент в `Assets/MadSlime/Resources/Prefabs/FlyingCube.prefab` (refs=1) и тип сериализованной ссылки в спавнере.
Перепайка: снять компонент с префаба (m_Script, guid `0fada3f7…`); `CubeSpawner.cs:14` — сменить тип `_cubePrefab` (например `GameObject`); `:49` — инстанцирование; `:29-39` — упоминание маркера в валидации.

### SkinModel — консенсус
`Assets/MadSlime/Scripts/Shop/View/SkinModel.cs` — пустой класс-маркер; навешан на 34 скин-префаба `Resources/PlayerModel/Prefabs/*Skin.prefab` (refs=34, guid `c6f7a62a…`), создаётся редакторным инструментованием, достаётся `TryGetComponent` в рантайме.
Перепайка: снять компонент с 34 префабов (YAML-правка по guid); `SkinItemFactory.cs:293` (`AddComponent<SkinModel>`), `:283` и `ModelPlacer.cs:63` (`TryGetComponent`), сигнатуры `ModelPlacer.cs:124,232,254` (принимают `SkinModel`), сообщения `:66,254`.

### Clamper — спорный (Клод: уровень B; сабагент: отказ)
`Assets/MadSlime/Scripts/Player/Logic/Clamper.cs` — чистая математика (`ClampPosition`), lifecycle нет; привязки: `[SerializeField] _radius` (в сцене, refs=1), `name` в сообщении исключения (заменяемо), `TryGetComponent` в `Movement.cs:47`.
Аргумент за B: наследование объективно избыточно, радиус может прийти из конфига/конструктора. Аргумент за отказ: сидит на игроке и сериализуется в Инспекторе — перепайка трогает сцену и DI. **Итог: возможен уход, но только вместе с переносом радиуса в конфиг — в приоритете после маркеров.**

### LeaderboardRowViewFactory — спорный (Клод: уровень B; сабагент: отказ)
`Assets/MadSlime/Scripts/UI/Leaderboards/LeaderboardRowViewFactory.cs` — lifecycle нет; `Instantiate` в файле — это вызов VContainer-резолвера, не MonoBehaviour; привязки: `[SerializeField] _prefab`, компонентность в сцене (refs=1).
Аргумент за B: фабрика без lifecycle — естественный plain-класс, префаб можно подать через DI. Аргумент за отказ: сериализуемый префаб в Инспекторе — та же привязка, что у Clamper. **Итог: аналогично Clamper.**

## Оставляем MonoBehaviour — отказы (консенсус)

- `Rotator`, `Mover`, `GridBuilder`, `CubeSpawner` — `transform`-математика, `Instantiate`.
- `Item` — `[SerializeField] ItemDefinition`, `[RequireComponent(ItemBody)]`, refs=420 (префабы предметов); `ItemBody` аналогично.
- `FillTapButton` — `IPointerDownHandler`: обязан быть компонентом, чтобы EventSystem звал колбэк; `[RequireComponent(Image)]`, в `Fill.unity`.
- `OverlapRadiusDetector` + семья: `GenericOverlapDetector<T>` → `QuotaItemDetector`/`ItemDetector`/`AttractableDetector`, `CameraObstacleDetector` — сериализуемый `DetectionRadius` по всей семье.
- `CameraImpulseAnimator` — `SetLink(gameObject, …)` (DOTween), база `CameraFovKick/Pull/ShakeAnimator`.
- `Yg2LeaderboardService` — `gameObject.name` как имя JS-колбэка (refs=1); `RoulettePreparation` — два `[SerializeField]` префаба.
- Транзитивные семейства: `BaseWindow` ← FailMenu/LeaderboardMenu/PauseMenu/RouletteWinPopup/WinMenu; `TweenAnimatorBase` ← UiFade/UiMove/UiScaleAnimator; `SceneState<T>` ← Fill/Game/MenuSceneState — наследуют lifecycle и инспекторные поля базы.
- Все остальные: у каждого lifecycle или instance-API (полные проходы — в удалённых отчётах, выводы совпали).

## Побочные находки (вне задачи)

- **Мёртвые классы** (нашёл Клод, проверено по всему Assets): `LookAtCamera`, `UiFadeAnimator` — guid не встречается ни в сценах, ни в префабах, ни в `.asset`, код-ссылок нет вообще. Это кандидаты не на рефакторинг, а на удаление.
- **PlayerInputActions.cs — не MonoBehaviour** (поймали оба независимо): автогенерат Input System, `MonoBehaviour` только в док-примере; создаётся через `new` (`PlayerInputReader.cs:19`). Ложное срабатывание наивного `rg ':\s*MonoBehaviour'`.
- `CameraImpulseAnimator` сам refs=0, но жив как база трёх аниматоров, висящих в сценах — не мёртвый.

## Перепись

- Клод: **128 MB-классов MadSlime** = 109 прямых (якорный rg по объявлениям, мимо док-комментариев) + 19 транзитивных (перечислены выше и в этом файле).
- Сабагент: 151 всего = 122 MadSlime + 29 сторонних.
- Расхождение переписи MadSlime (128 vs 122) — в подсчёте транзитивных, на выводы не влияет: все содержательные расхождения сведены выше (2 спорных), остальное — консенсус.
