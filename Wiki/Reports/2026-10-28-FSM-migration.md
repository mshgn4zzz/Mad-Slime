# Переход на FSM — 28.10.2026

Выполнены все актуальные находки страницы «Потенциальные места для FSM», включая CubeFlightAnimator. Базовые StateMachine и IState не изменены относительно начала этой работы; существующие изменения базового класса до начала работы сохранены.

## Разделение ответственности

| Система | Машина | Представление/адаптер |
|---|---|---|
| Окна | WindowLifecycleStateMachine | BaseWindow → ReactiveUIView |
| Общая rewarded-реклама | RewardedAdStateMachine | AdScheduler → IAdsService |
| Время игры | TimerStateMachine | Timer.Update передаёт scaled deltaTime |
| Лидерборд | LeaderboardStateMachine | LeaderboardMenu → LeaderboardView |
| Страницы меню | MenuPageStateMachine | MenuNavigation; существующая MenuStateMachine управляет сессией |
| Полёт куба | CubeFlightStateMachine | CubeFlightAnimator, отдельная машина на куб |
| Заливка | ShapeFillPlaybackStateMachine | ShapeFiller → ShapeFillView |
| Интерфейс Fill | FillUiStateMachine | FillUiPresenter → FillResultView/окна |

Рулетка была перенесена ранее и проверена повторно. FillRescueFlow и FillPauseFlow удалены: их решения принадлежат машине UI Fill. Начисление x2 выполняет DoubleRewardModel через SavesTransaction; повтор подтверждения не начисляет повторно. Остановленная заливка сохраняет данные для Rescue, а Arrived куба не задерживается до конца эффекта приземления.

## Проверки после завершения реализации

- Новые FSM: 45 проверок, exit 0 (`run-fsm-migration.sh`).
- Рулетка и связанные окна/реклама: 44 проверки, exit 0 (`run-skin-roulette.sh`).
- Финальные сценовые сценарии: 122 проверки в 6 прогонах, все exit 0 (`run-scene-transitions.sh`).
- Прогрессия/расчёт заливки: 6 проверок, failed 0.
- Анимации: проверены точный прилёт, нормальный/ускоренный/мгновенный выпуск, отмена, спасение и переключение режима уже во время выпуска; 4305 кубов в полных сценариях.
- Камера: 419 проверок, failed 0.
- SHA-256 базовых BaseStateMachine и IState совпадает с началом работы.
- git diff --check: без замечаний. Новые C# assets и папки имеют .meta.

Полный ручной визуальный обход всех окон в двух ориентациях экрана не выполнялся; проверки UI здесь функциональные, а анимации и геометрия проверялись отдельным runtime-прогоном. Производственные сцены/префабы не переписывались.

## Артефакты

- План: docs/superpowers/plans/2026-10-28-fsm-migration.md.
- Журнал выполнения: docs/superpowers/plans/fsm-migration-execution.md.
- Результаты: output/fsm-migration/{results.txt,scenes-final.txt,roulette.txt,animation.txt,progression.txt,camera.txt}.

Изменения выполнены последовательно по системам без сабагентов. По прямому указанию пользователя тестирование началось только после завершения всех блоков реализации. Проверка кода выполнена самостоятельно; commit/merge/push не выполнялись.
