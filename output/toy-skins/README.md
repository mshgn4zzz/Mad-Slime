# Twenty toy skins

20 new skins were authored through the live Blender MCP, using Python only inside Blender's MCP tool. Blender remained on workspace 20. Each model has its own editable `.blend` in `Blender/` and FBX under `Assets/MadSlime/Resources/PlayerModel/FBX/`. No external modeling runner or downloaded models were used.

The collection uses soft cube-like bodies, simple faces, broad pastel colors and a single shared palette material. Meshes have 1,028–2,344 triangles each. `manifest.json` reports 20 models with no non-manifold edges or degenerate faces. Closed overlapping component shells are intentional. Each finished model fills exactly 1 × 1 × 1 m with its origin at the geometric bounds center. The skin importer's baked axis conversion is accounted for: the faces point toward Unity +Z. Source colors remain editable; FBXs use atlas UV0.

| Rarity | New skins |
| --- | --- |
| Common | Bunny, Frog, Penguin, Chick, Piglet, BearCub, Dumpling |
| Rare | Panda, Fox, RedPanda, Bee, Mushroom, Cupcake, Cactus |
| Epic | Axolotl, Astronaut, ToyKnight, Jellyfish |
| Legendary | BabyDragon, Unicorn |

The existing SkinItemFactory created 20 SkinItems, runtime/preview prefabs and icons, and appended them to ShopContent. Total catalog: 33 unique skins. The two new legendary skins join Pacman, Snail and Vacuum in the exclusive daily roulette. Paid skin roulette retains its 65/25/10 common/rare/epic distribution; legendary probability there remains zero. Existing skin IDs and ownership are preserved.

Rebuild: **Mad Slime / Build Toy Skins**, or `EditorTools.ToySkinBuilder.Build`. `ToySkinBuilder.Validate` verifies size, center, readable meshes, forward-facing eyes, icons, rarity and exclusive pool. The old reference builder now permits an expanded catalog and preserves all configured legendary exclusives when rebuilt.

## Verification

The user's existing Unity editor remained open on workspace 7, without scene switching or focus changes. An isolated project copy was made using Btrfs copy-on-write copies of Assets, Packages, ProjectSettings and Library. All Unity commands ran there in batch mode. Tested assets were then copied into the real project after confirming that Menu.unity, ShopContent and RouletteConfig had not changed concurrently. `integration-hashes.json` records the delivered assets and their verified-copy hashes. Neither test grants, temporary balances nor test save files were copied into the real project.

`EditorTools.ReferenceSkinPlayModeCheck.Run` checks all 33 skins on the real player in the Game scene: selection, single active model, runtime squash material/texture, exact proportions, ground anchoring, collection compression and 2× growth. It also simulates 10,000 daily spins, verifies that all five exclusives can be awarded, and exercises all 27 regular skin rewards and the all-owned state. The final result passed; see `playmode/playmode-validation.txt` and `playmode.log`. Actual captures are under `playmode/`; the factory overview is `models-sheet.png` with its ordering text.

Testing with the saved Smell perk revealed unreadable item meshes used by QuickOutline. Read/Write was enabled only on imported meshes referenced by item prefabs, so its normal calculation and UV3 write work. `../menu-refresh/outline-imports.txt` lists 131 affected imports. The four level builders now retain that import setting on rebuild. Other import settings in the main project were preserved.

The main-menu mascot, motion and economy changes are documented in `../menu-refresh/README.md`. No full WebGL player build is claimed.
