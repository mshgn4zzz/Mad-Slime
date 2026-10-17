# Space theme

Space follows City at level 50. The active catalog is Parking 1–24, City 25–49, Space 50–74, Universe 75 onward. Planets and galaxies belong to the following Universe theme.

## Assets and authoring

All 23 new props were modeled and exported through the live Blender MCP (`execute_blender_code` inside Blender), with Blender on workspace 20. There are no external Python modeling runners. Each model has a separate editable `.blend` under `Blender/` and an FBX under `Assets/MadSlime/Art/Space/FBX/`. Each FBX uses one atlas material, UV0, no animation. The source geometry audit in `manifest.json` reports zero non-manifold edges and degenerate faces. Closed intersecting component shells are deliberate low-poly assembly, not a boolean-unioned skin.

The Space palette has ivory, navy/blue/cyan, orange/gold, steel, rocks, ice and purple crystals. The runtime shared material is `Assets/MadSlime/Art/Space/Materials/SpacePalette.mat`. Sources retain editable per-part colors.

Size roles:

| Tier | Props |
| --- | --- |
| Small | AsteroidRock, DebrisAntenna, DebrisHull, DebrisPanel |
| Medium | AsteroidIce, CubeSatellite, DishSatellite, SputnikSatellite |
| Large | AsteroidMetal, CargoCapsule, DeepSpaceProbe, SpaceTelescope |
| Huge | BoosterRocket, CrewCapsule, LunarLander, StubbyRocket |
| Giant | ContainerFreighter, CrystalAsteroid, SpaceShuttle, SpentStage |
| Boss | OrbitalStation, ResearchShip, RingStation |

The existing item factory generates source wrappers, 23 runtime prefabs, 23 item icons and 138 tier definitions. SpaceProps selects one semantic tier per model, rather than letting stations appear as tiny debris. The item factory's floor-centered pivot convention is preserved. RuntimeScale is 0.45 of the standard factory maximum extent 0.15, applied equally to geometry and collider after factory normalization. Rockets, telescope, capsule and shuttle face the icon camera; runtime random rotations remain in place.

## Background

`Assets/MadSlime/Resources/LevelSurfaces/Textures/SpaceNebula.png` is a new opaque image generated with the built-in image generation tool. The unlit `MadSlime/SpaceGround` shader places it in world XZ with a mirrored 64 m repeat and saturation 0.8. There is no physical floor appearance or received shadow, giving the existing flat gameplay plane a space backdrop. LevelTheme enables a separate visual background mesh extending 16× beyond the original floor, slightly below it, so the camera does not reveal a plain background at map edges. It has no collider and does not change the bounds used for spawning or movement. Switching away disables it; reapplying Space enables it again. No runtime gravity/movement changes were made.

Generation source: `/home/otecglinomes/.codex/generated_images/01a14194-7402-7e72-9079-df2919b0b342/exec-3b0fee5b-54fe-4c05-b62e-5b56ed6cdaff.png`.

Final prompt (built-in tool, no CLI fallback):

> Use case: stylized-concept. Asset type: seamless square background floor texture for a colorful low-poly cartoon space collection game. Dark desaturated midnight blue and muted violet outer space, mostly calm empty space, a few very broad extremely faint soft nebula clouds, very sparse small dim cream and pale blue star dots spaced far apart. Flat simple painted cartoon, low contrast and no visual noise, not photographic. Colorful white orange gold cyan spacecraft and gray brown asteroids must remain the focus above this background. Uniform subtle brightness across image, no central bright point, seamless matching edges. No planets, no galaxies, no asteroids, no ships, no objects, no perspective, no text, no border, no watermark, no strong glow, no dense starfield, no black patches. Opaque full square.

Three fill-grid shapes are generated from factory icons: StubbyRocket, CubeSatellite and RingStation.

## Verification and rebuild

- `EditorTools.SpaceLevelSetup.Build`: import, factory generation, material/theme/config/catalog setup, asset validation and overview sheet. Also available as **Mad Slime / Build Space Level**.
- `EditorTools.CityProgressionCheck.AuditSpace`: real placement planner across 8 layouts × 8 samples per scale; checks all six tiers and theoretical mass reachability. Selected scale 0.45 passed 64/64 samples. This is a planner simulation, not 64 complete gameplay runs; placement also uses its own random generator.
- `EditorTools.LevelPlayModeCheck.RunSpace`: actual Game scene at levels 49, 50 and 55. Checks theme transition, all prefabs through ItemFactory, generated item membership and size roles, materials, icon pixels, visible quota HUD, enough spawned targets, fill-grid construction, game start, collection and player growth. Saves and the originally open scene are restored.

Logs: `final-setup.log`, `density-audit.log`, `density-audit.csv`, `playmode.log`. Asset validation: `asset-validation.txt`. The last Play Mode report is copied to `playmode-validation.txt`; original captures are in `output/level-check/` and prop close-ups in `PlayMode/`. See `models-sheet.png` and its ordering text for the full collection.

Unity checks use batch mode in the background; no editor GUI is opened. No full WebGL player build or complete fill-mode playthrough is claimed.
