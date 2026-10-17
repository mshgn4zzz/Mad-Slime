# Menu mascot, floating title and economy

The generated 2D mascot is saved at `Assets/MadSlime/Art/UI/SlimeMascot.png` and replaces the old SlimeIcon sprite in Menu.unity. It has actual alpha transparency, no square tile, and preserves aspect ratio. The existing Mad Slime text remains a separate child of GameTitle.

`Assets/MadSlime/Scripts/UI/Menu/MenuLabelFloat.cs` is attached to **MainMenuWindow / GameTitle**, moving both the mascot and text together. Inspector settings: amplitude **10 Canvas units** above/below the resting position, full period **4.5 seconds**, unscaled time enabled. It restores the original anchored position on disable and recaptures that position on enable, preventing drift when returning from other menu screens. Set amplitude to zero or disable the component to stop movement.

## Economy

Rewards now use `round(20 × level^1.65)` without a cap. See [current pacing report](../progression-pacing/README.md): 75 rounds / 50 minutes, full upgrades affordable at round 60 / 40 minutes from win rewards alone. Existing purchases, levels and balances were not reset.

| Upgrade | New step prices |
| --- | --- |
| Speed | 500 → 1,500 → 4,500 → 13,500 → 40,500 |
| Appetite | 625 → 1,875 → 5,625 → 16,875 → 50,625 |
| Taste | 1,000 → 3,000 → 9,000 → 27,000 → 81,000 |
| Metabolism | 875 → 2,625 → 7,875 → 23,625 → 70,875 |

First-step prices are 2.5× their original values; each subsequent step now costs 3× rather than 2×. Upgrade effects are unchanged.

Perks: Smell **3,000 → 7,500**, Adrenaline **4,000 → 10,000**, Ambitions **5,000 → 12,500**.

The validation reports and screenshots here record the previous menu integration checks. The temporary Editor setup/check scripts were removed at the user’s request. Only the prop factory, skin factory, player reset and skin unlock commands remain.

## Mascot generation

Created with the built-in image generation tool, transparent background requested; no CLI fallback. Source: `/home/otecglinomes/.codex/generated_images/01a14194-7402-7e72-9079-df2919b0b342/exec-680ab6f2-85e1-47f9-8058-e6fe0fddcfdf.png`.

Final prompt:

> Use case: stylized-concept. Asset type: transparent 2D cartoon slime mascot for the main-menu game title and game icon asset. A single adorable little lime-green slime, rounded low dome body with a gently asymmetric wobbly jelly silhouette and three subtle rounded base lobes. Two large dark charcoal bean-shaped eyes with a single tiny white glint each, a tiny warm happy smile, very subtle blush cheeks. Looks lively, sweet and curious, like a small friendly pet. Simple hand-drawn clean 2D mobile-game cartoon, bold soft dark-green outline, broad flat green color regions, one simple pale green highlight patch and one darker green underside, minimal detail. Slight three-quarter turn but both eyes clearly visible. Centered isolated full mascot, generous clean padding, strong readable silhouette at small size. Truly transparent background, no square tile, no scenery, no lettering, no logo text, no watermark, no props, no arms, no legs, no realistic slime textures, no glass rendering, no complex shine, no 3D render, no shadow beneath. Expressive and polished, uncomplicated.
