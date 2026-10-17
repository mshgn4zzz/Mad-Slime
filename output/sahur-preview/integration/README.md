# Sahur integration

- ID: `Sahur`; rarity: Epic (2).
- Exported through direct Blender MCP. 1 × 1 × 1 m, centred origin, 4,552 triangles, one material / palette texture, one UV channel. Unity-facing direction +Z.
- Created with the existing SkinItemFactory; its batch entry point uses the same RebuildSkin path. No separate Editor tools or checking scripts were added.
- Registered once in ShopContent: 34 skins total. Eligible for the ordinary skin roulette’s epic pool (10% rarity weight); not in the exclusive pool.
- Unity 2022.3.62f2 batch import and factory execution passed in an isolated reflink project copy, exit 0. Generated Unity icon inspected after correcting UV channel export. Gameplay was not run for this addition.
- Original player save unchanged. Only the two factories and reset/unlock Editor file remain.
