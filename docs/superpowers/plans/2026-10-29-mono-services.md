# Plain C# services implementation plan

Goal: migrate the 14 approved classes from MonoBehaviour without changing gameplay.
Execution: inline, no subagents, current dirty workspace; no commits or unrelated cleanup.
Spec: user-approved list in chat. FlyingCube and SkinModel remain components.

- [x] Baseline: run existing FSM migration checks; record all component settings and references.
- [x] Add verification of plain service types, constructor behavior, shared scoped instances, radius growth and serialized asset integrity. Confirm the new type check fails before migration.
- [x] Convert simple services and config consumers to constructor injection; retain initialization methods, algorithms and event order. Register scene services Scoped and PlayerUpgrades Singleton in project scope.
- [x] Convert UI factories: menu factory prefab fields move to MenuLifetimeScope, page presenters receive factories via injection. Leaderboard row prefab moves to LeaderboardMenu, which owns its factory. Roulette prefab fields move to MenuLifetimeScope; MenuContent receives RoulettePreparation via injection.
- [x] Convert Clamper: move base radius to PlayerConfig, inject shared service into Movement, LevelGenerator and PlayerScaler. Remove Clamper from serialized radius recipients and update its radius using the captured base value on each scale application.
- [x] Migrate scene/prefab YAML: transfer exact settings before removing component documents, owner component-list entries and all old component references. Preserve markers and unrelated working changes.
- [x] Update verification harness callers for ordinary service construction/resolution. Run service, FSM, scene contract and PlayMode flow checks; inspect diff and serialization references.

Review focus: eager validation vs scene Awake ordering; factory instances owned by each leaderboard window; Wallet scene scope; PlayerUpgrades project scope; repeated scene transitions; Clamper radius does not accumulate growth multipliers; no missing scripts or stale serialized service fields.

## Execution notes

- Baseline: FSM checks 48/48. New plain-type assertion failed before migration as expected.
- Migrated all 14 approved classes and removed their 16 component instances from five scene/prefab assets.
- Existing configuration references and initial clamp radius were compared with a snapshot of the working tree and retained exactly.
- Scene service lifetime preserved: PlayerUpgrades is project Singleton; Wallet and gameplay/menu services are Scoped; each leaderboard window constructs its own row factory.
- Clamper was also serialized by RadiusRecipientEntry; it now receives explicit scale updates based on a captured initial radius, and shares the same bounds instance with Movement and LevelGenerator.
- PlayMode exposed MenuNavigation's additional serialized RoulettePreparation dependency; migrated it to injection alongside MenuContent.
- Verification: FSM checks, six scene-transition modes, ordinary-service behavior/asset integrity and skin roulette flows pass. Missing-script scan covers Menu/Game/Fill and Resources prefabs. Markers retain 1 FlyingCube and 34 SkinModel attachments.
- Review performed inline as requested; no subagents, commits or package changes.
