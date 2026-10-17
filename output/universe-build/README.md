# Universe: planets and galaxies

New theme after Orbit: **Parking 1–24 → City 25–49 → Space/Orbit 50–74 → Universe 75 onward**. Earlier assets and theme ranges are retained.

## Collection

22 newly authored low-poly props, all modeled and exported through direct live Blender MCP. Blender is on workspace 20. Each model has its own editable `.blend` under `Blender/` and its own FBX under `Assets/MadSlime/Art/Universe/FBX/`. No external Python modeling runner or downloaded asset library was used.

| Tier | Models |
| --- | --- |
| Small | Comet, DwarfCeres, DwarfPluto, MoonCratered |
| Medium | EarthPlanet, MarsPlanet, MercuryPlanet, VenusPlanet |
| Large | JupiterPlanet, NeptunePlanet, SaturnPlanet, UranusPlanet |
| Huge | BinaryStar, BlueStar, RedGiant, SunStar |
| Giant | BlackHole, GlobularCluster, PlanetaryNebula |
| Boss | BarredGalaxy, EllipticalGalaxy, SpiralGalaxy |

Identity comes from geometry and broad color regions: crater rims and dark maria; Pluto's pale patch; Earth's land/ocean/polar regions; gas bands and storms; separated Saturn rings; a tilted Uranus ring; two comet tails; stellar prominences; the black hole disk and light arc; star clusters; volume in the spiral arms; and a bar in the two-armed galaxy. These are stylized game representations, with illustrative sizes and stellar structures rather than an astronomy simulation.

The existing ItemPropFactory generates 22 runtime prefabs, 22 icons and 132 tier definitions. UniverseProps selects one semantic size tier per model, so galaxies cannot be assigned a small-object role. RuntimeScale is 0.45 of the factory's standard normalization. Visuals and colliders scale together. The factory's floor-centered item pivot convention is preserved.

Exports have UV0 and one material per model. Most use UniversePalette; stars, nebula, black hole, cluster and galaxies use UniverseGlowPalette, sharing the same 16-color atlas with moderate material emission. This does not add point lights or bloom. Editable Blender sources retain per-part colors. Geometry audits in `manifest.json` report zero non-manifold edges and zero degenerate faces; deliberately intersecting closed component shells are not boolean-unioned.

## Background

The previous Orbit texture remains unchanged. Universe uses a new subdued plum-charcoal backdrop, with less visible haze and sparse dim stars. Colorful planets and luminous structures supply the visual focus.

Texture: `Assets/MadSlime/Resources/LevelSurfaces/Textures/UniverseQuietSpace.png`.
Material: `Assets/MadSlime/Resources/LevelSurfaces/Materials/UniverseGround.mat`.
The existing unlit SpaceGround shader is reused, with an 80 m mirrored world-space repeat and saturation 0.65. The existing extended background surface prevents exposed camera background at the map edge, without enlarging movement/spawn bounds.

Generated with the built-in image generation tool, no CLI fallback. Original: `/home/otecglinomes/.codex/generated_images/01a14194-7402-7e72-9079-df2919b0b342/exec-8fff4a25-3d63-496b-9edb-1e4706b6f6ec.png`.

Final prompt:

> Use case: stylized-concept. Asset type: seamless square ground backdrop texture for a cheerful low-poly game level with collectible planets stars and galaxies. Primary request: a very quiet DARK muted plum-charcoal outer-space background, subtly distinct from a midnight blue orbit level. Almost uniform deep desaturated aubergine charcoal, only extremely faint broad dusty mauve haze with very soft edges, a handful of tiny dim warm-gray star dots spaced very far apart. Calm simple cartoon painted backdrop, not photographic, flat illumination. The planets and galaxies are the colorful foreground heroes; background must be recessive and low contrast. Seamless matching edges, full opaque square. Avoid bright nebula clouds, dense stars, sparkles, large stars, galaxy shapes, planets, spacecraft, objects, central focal points, black holes, strong gradients, noise, grain, text, logos, watermarks, borders. Darkness should still show a subtle plum tint rather than pure black.

## Rebuild and verification

- `EditorTools.UniverseLevelSetup.Build`, also **Mad Slime / Build Universe Level**: import and material remaps, actual item factory, theme/config/catalog, three fill shapes (EarthPlanet, SaturnPlanet, SunStar), asset validation and overview sheet.
- `EditorTools.CityProgressionCheck.AuditUniverse`: real placement planner, 8 layouts × 8 samples per scale. Selected 0.45 passed 64/64 samples with all six tiers present and enough theoretical mass to reach the final tier. Planner samples are not complete game playthroughs; the layout planner also has its own random generator.
- `EditorTools.LevelPlayModeCheck.RunUniverse`: actual Game scene at levels 74, 75 and 80, transition check, every prefab instantiated through ItemFactory, natural spawn membership, tier roles/mass reachability, supported materials, icon pixel validation, visible quota HUD, reachable target counts, three fill grids, game start and real collection/growth. Tests restore the editor save and original scene. Background enable/disable and unchanged map bounds are checked too.

Evidence: `setup.log`, `asset-validation.txt`, `density-audit.log`, `density-audit.csv`, `playmode.log`, `playmode-validation.txt`, `models-sheet.png`, `models-sheet-order.txt`, gameplay captures and `PlayMode/` close-ups. Unity checks are batch mode only, without opening its editor window. No full WebGL build or complete fill-mode playthrough is claimed.
