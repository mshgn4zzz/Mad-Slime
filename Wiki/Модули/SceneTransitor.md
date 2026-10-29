# SceneTransitor

Постоянный сервис Unity-переходов, зарегистрированный в ProjectLifetimeScope. Заменяет SceneNavigator.

- `BeginTransition()` открывает загрузочный экран через LoadingScreenPresenter.
- `LoadAsync(source, targetName, token)` отключает Canvas, EventSystem и AudioListener исходной сцены, аддитивно загружает и активирует целевую.
- `CompleteAsync(outgoing, token)` дожидается завершения анимации загрузки и выгружает исходную сцену.
- `EndTransition()` закрывает загрузочный экран после входа в новое состояние.
- `Stop()` снимает временную подписку sceneLoaded и закрывает экран.

Результат загрузки определяется по событию sceneLoaded и конкретному Scene handle, включая перезапуск сцены с тем же именем.

Сервис не выбирает игровые состояния, не вызывает их Enter/Exit, не сбрасывает прогресс и не инициализирует инвентарь. Скин и прогресс view задаёт LoadingScreenPresenter. Первоначальную готовность сохранений и инвентаря обеспечивает GameStartup.

При отмене ожидания Unity может закончить физическую загрузку. SceneStateMachine отменяет свою операцию и не входит в новое состояние после остановки.

См. [[SceneStateMachine]].
