# City level

City occupies levels 25–49. Parking 1–24 is preserved; Space follows at 50–74 and Universe starts at 75.

26 props: 21 new Blender models and 5 reused Parking props. Each new model has its own source `.blend` in `Blender/` and an exported FBX in `Assets/MadSlime/Art/City/FBX/`. Modeling and exports used the live Blender MCP. Models have a single palette material and UV0; their exports passed non-manifold and degenerate-face checks. Runtime props use the shared Parking palette material.

Size progression: cars/hydrant/bin → street furniture → kiosks → shops/houses → apartment buildings → towers/construction site. The traffic light has a straight broad post and three lenses on each of four faces. Building backs and sides include windows so random rotation still presents a finished facade.

The item factory produced 156 definitions (26 × 6 tiers); CityProps selects 26 definitions with fixed semantic size roles. Runtime normalization uses 0.8 of the factory's standard maximum dimension, including both visuals and colliders. The density audit tests 8 layouts × 8 seeds per scale with the real placement planner and mass thresholds. The selected 0.8 scale passed 64/64 simulations; these are planner simulations, not 64 full game playthroughs. See `density-audit.csv` and `density-audit.log`.

Floor: muted grass blocks and broad pale paths, world-space mirrored repeat 48 m, saturation 0.6. The floor pattern is visual; the existing placement system does not align buildings to its paths. Texture: `Assets/MadSlime/Resources/LevelSurfaces/Textures/CityPaths.png`; shader: `Assets/MadSlime/Shaders/CityGround.shader`.

Rebuild: Unity menu **Mad Slime / Build City Level**, or batch execute `EditorTools.CityLevelSetup.Build`. Play Mode verification: `EditorTools.LevelPlayModeCheck.RunCity`. Planner audit: `EditorTools.CityProgressionCheck.Audit`. Reports and gameplay captures are in this folder and `output/level-check/`. Three City fill shapes are checked through the actual grid builder; this is not a complete fill-mode playthrough. No full WebGL player build was run for this change.

## Floor image generation

Method: built-in image generation, new opaque square image; no external texture scraping. Generated image copied into CityPaths.png and configured by CityLevelSetup.

Original: `/home/otecglinomes/.codex/generated_images/01a14194-7402-7e72-9079-df2919b0b342/exec-5b5a8af2-7866-4edb-aaea-2425fc37443d.png`.

Prompt:

> Use case: stylized-concept. Asset type: seamless repeating top-down floor albedo texture for a cheerful toy-like low-poly CITY level in a mobile collection game. Entire square filled by a calm little-town ground pattern: large muted dusty sage-green grassy neighborhood blocks with softly rounded corners, separated by broad light warm gray-beige pedestrian pathways and spacious intersections. About 65% quiet grass and 35% paths. Very large simple shapes, few blocks, generous wide paths, no dense grid. Paths are smooth pale warm concrete, with extremely faint widely spaced slab joins. Grass is flat hand-painted cartoon, no realistic blades, only a few broad subtle soft patches, low saturation and low contrast. Ground is a recessive background for colorful shops and buildings and a vivid green player. Flat orthographic overhead, uniform color illumination, no cast shadows, no perspective, no bevel render lighting, no border. Seamless matching opposite edges so paths continue through repeat boundaries. No buildings, no vehicles, no trees, no props, no asphalt, no road paint, no labels, no text or watermark. Super simple charming cartoon game floor, not a realistic aerial map.
