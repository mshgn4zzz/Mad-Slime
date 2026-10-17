# Финальные скины игрока

13 моделей: Slime, Pacman, CubeSlime, Cat, Dog, Ufo, Car, Snail, Ghost, Shark, Vacuum, Mimic, LegacySlime.

- `Blender/`: отдельный `.blend` для каждой модели. Финальная доработка, экспорт и проверка геометрии выполнены через прямой Blender MCP.
- `../../Assets/MadSlime/Resources/PlayerModel/FBX/`: отдельные FBX.
- `../../Assets/MadSlime/Resources/PlayerModel/Prefabs/`: игровые префабы, созданные через SkinItemFactory.
- `../../Assets/MadSlime/Scriptables/Skins/`: SkinItem с назначенными редкостями.
- `final-collection.png`: первоначальное превью 12 моделей по референсам; актуальные игровые превью всех 13 — в `playmode/`.
- `final-source-validation.json`: проверка отдельных Blender-файлов, размеров, центра и замкнутости сетки.
- `unity-validation.txt`: результаты проверки импортированных префабов, материалов, иконок и каталога.

12 новых моделей имеют габариты строго 1 × 1 × 1 м. Восстановленный LegacySlime сохраняет исходные низкие пропорции: 1 × 0,60955 × 1 м в Unity. Геометрия центрирована относительно pivot; лицо в Unity направлено по +Z. Количество треугольников: 256–3638 на модель. Полосы кота используют текстуру `PlayerModel/Textures/CatBody.png`; остальные цвета заданы материалами.

LegacySlime восстановлен из Git-коммита `a7307d55`, путь `Assets/MadSlime/Resources/Models/Player/Slime/Skin/SlimeSkin.fbx`. Его цветовые варианты использовали тот же SlimeSkin.prefab. Исходный FBX сохранён без изменений в `Assets/MadSlime/Art/Legacy/SlimeOriginal.fbx`; его материалы используются при повторном импорте. SHA-256 исходного FBX: `7f35c7eb178d5013bd5611dee41799ed442f7278ad1685d2ceca5e9c0eef497a`.

Повторный импорт и создание ассетов: меню Unity `Mad Slime/Rebuild Reference Skins`. Проверка без пересоздания: `Mad Slime/Validate Reference Skins`.

Проверка на настоящем игроке: `Mad Slime/Check All Skins in Play Mode`. Она запускает сцену Game, применяет каждый скин через SkinInventory/SkinApplier, проверяет замену, материалы, положение относительно коллайдера, сжатие и рост ×2. Результат и кадры игрового мира находятся в `playmode/`; `all-skins-on-player.png` и `all-skins-squashed.png` показывают крупные планы настоящего игрока с освещением уровня. Исходный редакторский файл сохранений восстанавливается после прогона.

Редкости: Common — Slime, CubeSlime, Car, Ufo; Rare — Cat, Dog, LegacySlime; Epic — Ghost, Shark, Mimic; Legendary — Pacman, Snail, Vacuum. Эксклюзивный пул ежедневной рулетки содержит ровно три Legendary. Платная рулетка: 65% Common, 25% Rare, 10% Epic, 0% Legendary; доли нормируются, если доступных скинов редкости больше нет. Проверка выполняет 2000 ежедневных вращений и проверяет получение всех обычных наград платной рулетки, включая блокировку вращений при полностью открытой коллекции.

Старые ресурсы сохранены в `previous-player-assets.zip`. Ранние общие файлы и резервные копии Blender находятся в `Archive/`; остальные промежуточные журналы и материалы в этой папке не являются финальными моделями.
