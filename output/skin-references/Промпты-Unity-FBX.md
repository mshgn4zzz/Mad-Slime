# Mad Slime — промпты для 12 моделей Unity FBX

Каждый TXT — самостоятельный промпт. Передайте его вместе с одноимённым PNG. На PNG шесть видов: верхний ряд — спереди, левый бок, сзади; нижний — правый бок, сверху, три четверти. Все виды относятся к одному персонажу. Референсы — концепты, не готовые FBX.

Общий размер выбран для согласованности: ширина 1 м, глубина 1 м, высота 0,75 м вместе с выступающими деталями. В игре весь набор можно масштабировать одинаковым множителем. Бюджет треугольников в промптах — ориентир для генератора, не подтверждённая характеристика готовых моделей.

Уточнения: машинка без лица и глаз на стёклах; НЛО с гладким низом без выступа; кубический слайм зелёный, с почти прямыми рёбрами и квадратными глазами и ртом, близко к Minecraft. Все прочие скины сохраняют общий компактный объём.

## 01-slime

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Apple green classic slime, a broad plump rounded dome with flat grounded base, dome nearly as wide as deep. Two small dark charcoal spherical eyes high on front, slightly wide-set. No mouth, no limbs, no tail, no inner objects, opaque material. Back and top plain green. Cute neutral expression. Broad soft slightly irregular base, no droplet point.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 02-pacman

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Yellow plump spherical chomper with flattened base, compact nearly spherical rounded body fitting the shared squat proportions. Huge dark charcoal open wedge mouth on FRONT only, rounded thick upper and lower lips, empty dark mouth cavity, no teeth or tongue. Two dark pie-cut oval eyes above mouth. Back plain yellow; mouth extends only into front half, never a slit all around body. Simple happy expression. No limbs.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 03-cube-slime

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Bright apple/lime GREEN slime closely resembling the Minecraft slime mob: broad flat square faces, squared-off nearly cube shape with only very small corner bevels (radius 3% width), two LARGE DARK GREEN PERFECT SQUARE flat eyes and one smaller DARK GREEN SQUARE flat mouth centered lower between eyes, iconic blocky pixel-like face, NOT rounded pill eyes, NOT a dash mouth, NOT turquoise. A darker green inset central core suggested by a broad simple green panel within the light green outer shell, no complicated pixel noise. Soft toy adaptation with barely softened cube edges. Keep shared compact broad volume 1 width x1 depth x.75 height, not tall. Opaque surfaces for reconstruction, no glass reflections.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 04-cat

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Very fat ginger loaf cat, squat rounded cube body, head smoothly merged into body, short broad muzzle facing front. Two small thick triangular rounded ears, four tiny cream paw nubs at base, thick short curled tail pressed against back rising only to midbody. All appendages INSIDE common 1x1x.75 bounding box. Cream muzzle and paws, dark orange exactly three broad stripes across back, small dark sleepy half closed eyes, small pink triangular nose. No whisker strands or fur texture. Lazy comical expression. Cat must be as deep as wide, not elongated.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 05-dog

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Very plump cream puppy shaped like a squat rounded cube, short merged head and body, large broad short protruding muzzle with charcoal nose, four stubby paws at base. Two thick chocolate brown floppy ears close to sides, brown patch around CHARACTER LEFT eye, short thick upturned tail tucked close to rear, small pink tongue hanging from CHARACTER RIGHT corner of mouth. Round enthusiastic dark eyes. No fur strands. All ears muzzle tail fit in 1x1x.75 bounding box, no elongated dog anatomy.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 06-ufo

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Chunky lilac UFO, equal width and depth, thick rounded circular saucer with a large opaque sky blue centered domed cockpit. Exactly four broad small butter-yellow inset lamps around rim. Two dark eyes on front of cockpit, no separate alien. Continuous clean shallow rounded underside with absolutely NO nozzle, plug, knob, stalk, pedestal, protrusion, landing leg, beam or particle effect. Compact plump volume 1 x 1 x .75 m. All surfaces opaque. No complex mechanical details.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 07-car

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Short super-deformed coral red toy car with cream rounded roof, tiny hood, short broad cabin, four thick charcoal wheels with cream plain hubs, tiny integrated rear spoiler. Car as wide as long, overall envelope 1 x 1 x .75 m including wheels. Windshield and all other windows are plain opaque dark charcoal with NO eyes or face. Two normal round cream headlights and a straight small rounded rectangular dark radiator grille in the front bumper, no smiling mouth. No anthropomorphic features anywhere, no eyelashes, no pupils, no teeth. No mirrors, interior, logos, exhaust or texture detail.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 08-snail

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Chunky peach cartoon snail compressed into broad rounded cube proportions, width and depth equal. Huge plump purple rounded shell occupying most body, a single broad cream spiral on EACH SIDE of shell, same handed consistent geometry. Short broad peach head protrudes slightly in front, two SHORT thick eye stalks with dark eyes, broad fleshy base, tiny blunt tucked tail. No long thin slug body, no thin antennae, no slime trail, no decal lightning. Shell height and stalks within total .75 body width. Comically determined face.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 09-ghost

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Plump opaque warm white cartoon ghost shaped like a LOW broad rounded bell, width depth 1 and height .75, squat not tall. Smooth dome head seamlessly joined body, flared lower edge with exactly four broad soft scallops, two tiny thick side arm nubs hugging body. Two large charcoal oval eyes and small surprised round dark mouth on front, very pale lavender underside only. No fabric wrinkles, no floating wisps, no transparency, no glow. Flat low underside with small central recess. Compact broad solid mass comparable to slime.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 10-shark

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Very fat baby blue toy shark, compact rounded cube / puffer-like mass as WIDE as LONG, not realistic long shark. Cream belly and broad short muzzle, large dark smiling mouth with exactly four big blunt cream triangular teeth. Two tiny dark eyes wide apart. One SHORT thick rounded triangular dorsal fin, two small thick side flippers, extremely short thick tail tucked against rear, all within same 1x1x.75 bounds. No scales, no gill slits, no water. Friendly smug expression. Body is main volume; fin must not make it tall.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 11-vacuum

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Cute ROUND ROBOT VACUUM CLEANER, no humanoid robot, no tracks. Thick butter-yellow circular puck body with rounded cylindrical sides, equal width and depth, unusually plump and substantial to match slime mass. Large raised rounded domed top housing integrated with body brings total height to .75 width; no tower. A broad dark charcoal front bumper panel with two pale blue expressive eye shapes, centered wide intake low on front. One small flat coral round power button on top, concealed small wheels underneath. No antenna, no bristles, no hose, no handles, no text or symbols, no glow. Rear plain with one broad simple seam.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

## 12-mimic

```text
Create exactly ONE complete production-ready 3D player skin for Unity 2022.3, using the attached turnaround as a geometric reference, not as a texture to project. The multiple drawings are views of ONE asset: do not create six characters, planes, a collage or a background.

STYLE AND PROPORTIONS
Simple low-poly cartoon mobile/WebGL game asset, chunky rounded toy forms, broad bevels, smooth normals, clean solid-color materials. Match Mad Slime's soft cartoon food and furniture aesthetic. All skins share the same world-space envelope: X width 1.00 m, Z depth 1.00 m, Y height 0.75 m, including ears, fins, tails, wheels and shell. Keep the main body broad, plump and visually substantial; do not achieve size only through thin protrusions. Equal width and depth, no tall thin narrow or elongated anatomy. Silhouette remains recognizable from an elevated gameplay camera. Use this same scale for every skin.

CHARACTER
Plump cartoon mimic treasure chest, broad rounded box width depth 1 height .75 including short integrated feet. Warm medium brown lower box and thick arched rounded lid, exactly two wide muted golden straps wrapping lid front to back. Lid slightly open at FRONT only forming wide dark purple horizontal mouth, two large blunt cream teeth and small pink tongue. Two dark mischievous eyes on front lid and small central gold lock like nose. Four very short thick feet integrated at base. No planks texture, no woodgrain, no nails or rivets, no coins or props. Compact toy silhouette.

MODEL AND EXPORT
Deliver a single character asset as FBX with clean named meshes and separate simple material slots where useful. Unity orientation: Y up, face forward +Z, ground at Y=0, pivot at bottom center. Apply transforms and ensure export/import preserves 1 m width without 100x scale errors. No rig or animation needed for this static base asset. Use economical actual low-poly geometry, suggested budget about 800–2500 triangles, up to 3000 if the mouth needs it; do not add subdivisions solely for polished rendering. Clean outward normals, no accidental internal faces, no intersecting duplicate shells, no floating disconnected tiny details. Model visible cavities with real geometry, never black holes cut into texture planes. Keep thin features thick enough to survive simplification and deformation. If small appendages are separate meshes, keep them attached visually.

SURFACES
Use 2–4 broad main color regions plus small face accents, opaque matte / very lightly satin surfaces, nonmetallic appearance. Solid base colors without baked highlights, directional shadows, gradients from lighting, ambient-occlusion grime or reflections. Prefer material colors; if textures are required, use a simple compact color atlas. Any roughness belongs in the material, not painted specular spots. No transparency on body or cockpit. No realistic fur, scales, woodgrain, fabric, microdetail, dirt or scratches.

REFERENCE INTERPRETATION
Maintain the same markings and geometry in front, rear, both side, top and three-quarter views. Reconstruct obscured surfaces consistently. If the AI image has a minor view inconsistency, preserve a coherent symmetric volume and the explicit character description; never duplicate limbs or facial features. No floor, props, pedestal, camera, lights, text, watermark, effects, trails, smoke or surrounding scene in the FBX.

```

