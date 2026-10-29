# Roulette State Machine and Presenter Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans in the current chat, without subagents. Do not commit unrelated work.

**Goal:** Fix skin roulette opening and rebuild the presenter/model interaction using the event-driven FillUiPresenter scheme requested by the user.

**Architecture:** RouletteStateMachine derives from BaseStateMachine<IRouletteState>. Separate states calculate/commit an attempt, confirm persistence and publish its result. RoulettePresenter subscribes to the machine, forwards UI commands, owns the prize window and performs shutdown. RouletteView owns wheel animation and the post-wheel pause. No Flow class.

**Tech Stack:** Existing Unity 2022.3, C#, VContainer, UniTask, uGUI, DOTween.

**Spec:** Current chat, 2026-10-28. The user replaced the initial proposal with the existing FillUiPresenter event-driven pattern: synchronous model, presentation outside the machine, view-owned delays, skin close request handled by presenter.

## Constraints

- Reuse existing BaseStateMachine/IState without changing them.
- Each popup owns separate state instances; no presenter enum or duplicate lifecycle state.
- Model.Spin remains synchronous. Save confirmation is persistence work, not a visual delay.
- No Spinning/PresentingPrize/Closing/Hidden states in the model machine. ResultState emits data immediately and accepts notification of presentation completion.
- Preserve pricing, reward transaction, assets, prefab component references and unrelated uncommitted changes.
- Work without subagents. Test only in the isolated verification copy; preserve the user's saves.

## States and Public Contract

States live under Assets/MadSlime/Scripts/Roulette/Logic/StateMachine and implement IRouletteState : IState.

- RouletteReadyState: clear completed attempt data, refresh model slots, notify presenter to rebuild the displayed wheel.
- RouletteWaitingForAdState: emit an advertising request with an entry-bound completion callback; accept granted/rejected result. Presenter invokes AdScheduler.
- RouletteCommittingSpinState: call model.Spin exactly once, capture outcome, emit committed/rejected/failure.
- RouletteConfirmingSaveState: confirm existing committed attempt; cancellation belongs to this state entry.
- RouletteSaveFailedState: expose confirmation retry without a new transaction.
- RouletteResultState: immediately emit ResultReady(outcome); no animation, delay, view dependency or waiting task.

Machine commands: Start(), TrySpin(), TryAdvertisement(), RequestClose(), CompletePresentation(), Stop(), Dispose().
Machine events: Ready, Changed, AdvertisementRequested(Action<bool>), ResultReady(RouletteOutcome), CloseRequested, Failed(Exception).
Machine capabilities are computed from CurrentState and model eligibility: IsRunning, IsReady, HasSaveError, CanSpin, CanRequestAdvertisement, CanClose.

Transitions:
- Stopped -> Ready: Start.
- Ready -> CommittingSpin: eligible ordinary attempt.
- Ready -> WaitingForAd: eligible advertising attempt.
- WaitingForAd -> CommittingSpin on grant; -> Ready on no grant.
- CommittingSpin -> ConfirmingSave on success; -> Ready on rejected purchase.
- CommittingSpin exception: report failure and request presenter shutdown; transaction rollback stays in SavesCore.
- ConfirmingSave -> Result on success; -> SaveFailed on failure.
- SaveFailed -> ConfirmingSave on retry; keep original outcome/sectors/payment.
- Result + CompletePresentation: daily -> Ready; skin -> CloseRequested event.
- Ready/SaveFailed + accepted manual close -> CloseRequested event.
- Any active state -> stopped via Stop; Exit cancels tasks and invalidates ad callbacks.

## Presenter/View/Menu

- RoulettePreparation constructs each machine and passes it to presenter.Setup, alongside advertising/audio/spawner/existing popup stack dependencies.
- Presenter.Show displays view and starts machine; ShowAndSpin accepts a freshly created window, shows/starts it and requests one attempt.
- Presenter handles Ready by building the wheel and Changed by updating controls through stateless RouletteDisplayBuilder.
- On ResultReady presenter plays spin sound and calls view.PresentResultAsync(slotIndex, token). View subscribes to wheel completion, starts the animation and performs Config.WinDwellSeconds delay. View unsubscribes/cancels cleanly.
- After successful view presentation, presenter creates/shows the prize popup. Prize closure pops/releases it, then calls machine.CompletePresentation.
- Machine.CloseRequested calls presenter.Hide, which stops machine, cancels presentation and hides view. View.Closed is relayed to menu.
- External disable/destroy also stops work, releases owned prize and informs menu so input cannot remain blocked. Normal hide/release must not duplicate Closed.
- MenuNavigation owns opening/registration/release of roulette windows and collection refresh. Push roulette before starting it; release prize before parent popup. Remove MenuRouletteFlow and all its callers.
- No other presenter/state-machine migrations.

## Implementation Tasks

### Task 1: Machine and Behavioral Checks

Files: new IRouletteState, RouletteSessionData, RouletteStateMachine and six state files under Roulette/Logic/StateMachine; tools/verification/RouletteStateMachineChecks.cs.

- [x] Reproduce original failure in real Menu scene with sufficient funds.
- [x] Add failing checks for the machine/public contract before implementation.
- [x] Implement states/events and original-outcome save retry.
- [x] Verify one charge/grant, frozen sectors, duplicate rejection, no-grant advertisement, late ad/save callbacks and insufficient funds.

### Task 2: Presenter/View/Menu

Files: RoulettePresenter.cs, RouletteView.cs, RouletteDisplayBuilder.cs, RouletteSoundFeedback.cs, RoulettePreparation.cs, MenuNavigation.cs; delete MenuRouletteFlow.cs/.meta. Preserve MonoBehaviour .meta GUIDs and add .meta for new assets.

- [x] Move visual timing to view and adapt presenter to machine events.
- [x] Move prize ownership into presenter and migrate menu away from Flow.
- [x] Verify real Menu opening: first attempt 300, next 450; successful animation/prize; skin closes, daily remains ready.
- [x] Verify actual retry button, cancellation during view pause, external disable while prize is visible, and stack restoration.

### Task 3: Verification and Review

- [x] Run bash tools/verification/run-skin-roulette.sh: all assertions pass; no Unity script errors or unobserved exceptions.
- [x] Run bash tools/verification/run-scene-transitions.sh: contracts/menu/game/fill/error-path checks pass; report unrelated failures.
- [x] Perform self-review without subagents; inspect actual transitions, subscription ownership, synchronous callbacks, shutdown order and saved-outcome retry.
- [x] Inspect diff and report changes/checks. Leave work uncommitted.
