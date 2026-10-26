# Progression and fill implementation plan

**Goal:** Start new players at zero, retain funded editor resets, target full upgrades after about 30 minutes, and make filled art readable inside the camera.

**Architecture:** Keep the existing reward formula, purchases, save transactions and level resolver. Tune the reward asset and expand the catalog to 28 levels (two per location). Increase the fill grid from 32 to 96 cells per axis, cap its world extent, fit it inside the current camera and batch animated cubes within fixed time windows.

**Tech stack:** Existing Unity 2022.3/C#, ScriptableObject assets; no new packages.

**Constraints:** Preserve existing working changes and saved player balances. No runtime test-money API. Editor reset retains 9,999,999 coins. Four five-step upgrades plus three perks cost 393,000 coins. Exclude skins, ads, roulette and failed attempts from the time estimate. Keep the 140-second failure timer as a safety ceiling, not an assumed completion time.

**Review focus:** Portrait and landscape framing including completion pulse; rectangular source aspect; all art textures; fill duration independent of resolution and frame rate; no gaps or overlaps in the level cycle.

- [x] Remove automatic funds and change new-save default to zero; keep editor reset tools.
- [x] Set base reward to 146, exponent 1.65: 27 wins earn 359,112, 28 earn 394,771. Allocate consecutive pairs: Table, Room, Cosmetics, ToyStore, School, Office, Bank, Backrooms, BlockGarden, Forest, Parking, City, Space, Universe.
- [x] Add executable Unity verification harness, demonstrate failures before changing grid behavior. Sample the exact target grid at cell centers, preserve source aspect, constrain world size to 64 and camera viewport with animation margin.
- [x] Batch border and fill spawning by elapsed time. Normal fill window 8 seconds, boosted 3 seconds, instant 0.25 seconds; cancellation must remain effective.
- [x] Run Unity compilation and checks, inspect rendered art, write a balance report with exact costs, level allocations and timing assumptions.

Ruling: Work in the existing checkout because the task depends on extensive uncommitted location assets and scene updates. Do not create commits containing the user's work.

Ruling: 30 minutes is a pacing estimate: 28 × (50 seconds collection + 14.3 seconds fill/transitions/purchases). Keep the existing time limit; real completion time requires player telemetry or a timed playtest.

Verification: initial checks failed against old behavior; final 6/6 passed. 50 artworks × four aspects passed bounds checks. Unity rendered Robot and FrogKing at portrait/landscape. Play Mode completed 4,305 cubes in 9.066/3.817/0.333 seconds, instant cancellation passed. Existing editor save byte-identical after checks. DOTween auto-capacity warnings observed on initial heavy fill; no animation failures.

## Revision: user-supplied round timing

User reports 60–70 seconds without upgrades and 20 seconds with upgrades; requests a 35-second collection average and more levels. Target now 36 wins: 35 seconds collection + 15 seconds other actions = 50 seconds × 36 = 30 minutes. First six locations have two levels each; remaining eight have three. Base reward 76, exponent unchanged: 35 wins yield 367,796; 36 yield 395,896, exceeding the unchanged 393,000 cost by 2,896. Earlier 28-level figures above record the initial implementation and are superseded by this revision. Update and execute existing economy/catalog checks; art behavior is unchanged.

Revision verification: updated tests first failed for early affordability and the shortened location cycle; after changing reward and ranges, all 6 Unity checks passed. Native RewardConfig reports 395,896 earned at level 36.

## Final revision: 50 wins requested by user

The user explicitly chooses 50 wins and 3–4 levels per location, superseding the 36-win target. First six locations get three levels, remaining eight get four, with the next cycle at 51. Reward base 32 at exponent 1.65: after 49 wins 373,743 coins; after 50 wins 394,088, enough for the unchanged 393,000 total. At 35 seconds collection this is 29:10 active play; overhead adds to wall time (41:40 with the prior 15-second overhead budget). Update existing Unity economy/catalog checks, then verify configuration and report.

Final revision verification: economy and catalog checks failed against the prior 36-level configuration; after applying 50-level ranges and base reward 32, all 6 Unity checks passed. Unity reports 394,088 earned at win 50.
