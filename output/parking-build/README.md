# Уровень «Парковка»

Подключён с уровня 25: Table 1–24, Parking 25+. Существующая тема Room сохранена вне активного каталога. Используется общая сцена Game, как у других тем.

22 авторские модели, созданные и экспортированные прямым Blender MCP:

- Машины: CarHatchback, CarCoupe, CarSUV.
- Автобусы: BusCity, BusRetro, BusCoach.
- Фуры: TruckBox, TruckTanker, TruckFlatbed — отдельные силуэты тягача и полуприцепа.
- Велосипеды: BicycleCity, BicycleMountain, BicycleBMX.
- По одному: FireHydrant, Motorcycle, Tank, TrashBin, TrafficCone, RoadBarrier, KickScooter, ShoppingCart, ParkingMeter, TireStack.

TireStack содержит ровно три шины: две лежат друг на друге, третья сбоку наклонена и развёрнута по диагонали, с опорой на пол и стопку. Первоначальные вертикальная стопка и вариант из четырёх шин заменены по уточнению пользователя.

## Файлы

- `Blender/*.blend`: отдельный редактируемый файл для каждой из 22 моделей. Старые автокопии вынесены в Archive.
- `Assets/MadSlime/Art/Parking/FBX/*.fbx`: игровые модели, 176–3024 треугольника на предмет. В итоговом манифесте нет открытых рёбер или вырожденных граней.
- `Assets/MadSlime/Art/Parking/ParkingModels.json`: манифест моделей и палитры для повторной сборки; не зависит от временной папки output.
- `Assets/MadSlime/Art/Parking/Textures/ParkingPalette.png` и `Materials/ParkingPalette.mat`: общая цветовая палитра, один UV-канал и один материал на модель, включён instancing.
- `Assets/MadSlime/Resources/Models/ItemsFORFACTORY/ParkingFact`: подготовленные исходные префабы.
- `Assets/MadSlime/Resources/Prefabs/Items/Parking`: 22 игровых префаба, созданных штатной ItemPropFactory.
- `Assets/MadSlime/Scriptables/Items/ItemsDefinition/Parking`: 132 определения, по шесть размеров на предмет. Коллайдеры и опора на пол рассчитываются фабрикой.
- `Assets/MadSlime/Resources/Icons/Items/ParkingIcons`: 22 иконки, отрендеренные фабрикой; `models-sheet.png` — их обзор, порядок в `models-sheet-order.txt`.
- `Assets/MadSlime/Scriptables/Levels/Parking`: ParkingProps, ParkingTheme, ParkingLevelConfig.

## Пол и заливка

`Assets/MadSlime/Resources/LevelSurfaces/Textures/ParkingAsphalt.png` создан встроенным Imagegen. Материал `Materials/ParkingAsphalt.mat`, шейдер `MadSlime/ParkingRoad`: мягкий серо-голубой асфальт с повтором 24 м, редкие парковочные ряды и стрелки с периодом 40 м. Разметка вычисляется отдельно от изображения асфальта. Матовая поверхность, mipmaps, трилинейная фильтрация, анизотропия ×8.

Тема содержит три собственные картинки заливки: машина, автобус и танк. Они получены из фабричных иконок, доступны для чтения пикселей; GridBuilder успешно строит границы и внутренние клетки для каждой.

## Проверка

`setup.log`: штатная фабрика создала 22 префаба, 22 иконки и 132 варианта. `asset-validation.txt`: проверены ссылки, иконки, коллайдеры, материалы и граница уровней.

`final-playmode.log`, код завершения 0, `LEVEL_PLAYMODE_CHECK_PASSED`: настоящая сцена Game на уровнях 24, 25 и 30. На 24 — Table; на 25 и 30 — Parking. Каждый из 22 префабов отдельно создан через ItemFactory в Play Mode. В случайной генерации появились соответственно 17 и 14 разных типов: состав конкретной карты зависит от выбранного layout и тиров. Проверены принадлежность предметов теме, достижимость квот по количеству объектов, видимые иконки HUD, сбор предмета и рост игрока. Для трёх картинок заливки построены реальные сетки GridBuilder. Исходный сейв восстановлен побайтно.

`../level-check/level-25-hud.png` и `level-30-hud.png` — портретные кадры игровой камеры с настоящим HUD; для съёмки Overlay Canvas временно переводился в ScreenSpaceCamera и затем восстанавливался. `PlayMode/*.png` — дополнительные камеры вокруг естественно сгенерированных предметов. Кадры просмотрены. Полный WebGL-билд и полный цикл прохождения заливки не запускались.

Повторная сборка: `Mad Slime/Build Parking Level`. Проверка: `Mad Slime/Check Parking in Play Mode`. Сборка общих поверхностей теперь сохраняет существующие диапазоны каталога.
