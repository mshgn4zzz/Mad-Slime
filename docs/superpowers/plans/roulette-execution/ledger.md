# Roulette execution

- Scope: implement in the current checkout, preserving unrelated uncommitted changes. No subagents or commits.
- Ruling: the user's latest instruction adopts FillUiPresenter's event-driven scheme. Replace the earlier seven-state UI process with six model states (Ready, WaitingForAd, CommittingSpin, ConfirmingSave, SaveFailed, Result). Result emits immediately after save confirmation; presenter/view own the complete animation/dwell/prize sequence. Cost if wrong: the lifecycle naming and event boundary would need revision.
- Baseline: actual Menu scene reproduces the failure with an enabled button and sufficient funds; first purchase costs 300, clicking removes the roulette before showing it. output/skin-roulette/baseline.txt.
- Ruling: reuse the existing UI assembly for Roulette.Logic and preserve its assembly boundary; no gameplay-to-UI dependency is introduced. BaseStateMachine is already accessible from MadSlime.UI.

- Task 1/2: machine and real Menu integration initially passed 33 behavioral assertions. Scene regression passed contracts 7, main 101, game 5, fill 3, missing-scene 3, missing-registration 3.
- Self-review found external-disable cleanup could leave the parent roulette registered in the menu stack. Added a real scene test with a visible prize; RED reproduced the missing menu notification. Fix: retain the presentation lifetime through hide completion and notify Closed once when disabled unexpectedly. Added coverage for disable during hide as well.

- Final review: self-review, no subagents (user requirement). Reviewed state registration/transitions, synchronous ad/save callbacks, token ownership, frozen outcome/sector mapping, rollback and popup cleanup. No remaining findings.
- External-disable fix: RED in output/skin-roulette/disable-red.txt -> GREEN, including forced disable with a visible prize and forced disable during hide; Closed emitted once and stack restored.
- Final roulette suite: bash tools/verification/run-skin-roulette.sh -> exit 0, 44 assertions. Includes actual menu opening, 300/450 prices, actual retry button, canceled view pause, daily return to Ready, rewarded retry, stale callbacks, insufficient funds and real transaction rollback. Output: output/skin-roulette/final.txt.
- Final scene suite after production changes: bash tools/verification/run-scene-transitions.sh -> exit 0, 122 checks across contracts/main/game/fill/missing-scene/missing-registration. Output: output/skin-roulette/scene-regression-final.txt.
- No commits, no scene/prefab serialization changes, no base state-machine changes. Structural decisions follow the latest chat instruction, with one ResultState covering the instant result while presenter/view perform visuals.
