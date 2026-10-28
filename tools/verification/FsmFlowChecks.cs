using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using DI;
using Economy;
using Game;
using Items;
using Leaderboards;
using Levels;
using PlayerInput;
using Quota;
using Saves;
using ShapeFill;
using UI.Fill;
using UI.Menu;
using UI.Windows;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public class FsmFlowChecks
{
    private const string ActiveKey = "MadSlime.FsmChecks.Active";
    private const string FinishedKey = "MadSlime.FsmChecks.Finished";
    private static int _phase;
    private static int _passed;
    private static double _phaseStarted;
    private static List<string> _results = new List<string>();
    private static int _levelBefore;
    private static int _balanceBefore;
    private static int _rewardBefore;
    private static int _reportsBefore;
    private static int _finishedCount;
    private static FillSessionData _session;
    private static FillStateMachine _fillMachine;
    private static FillResult _initialResult;
    private static UniTask _navigationChecks;
    private static MenuSessionData _menuSession;
    private static int _menuHandle;

    static FsmFlowChecks()
    {
        if (SessionState.GetBool(ActiveKey, false))
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += OnLog;
            _phaseStarted = EditorApplication.timeSinceStartup;
        }
    }

    public static void Run()
    {
        Directory.CreateDirectory("output/fsm");
        foreach (string file in Directory.GetFiles("output/fsm")) File.Delete(file);
        PlayerSettings.productName = "MadSlime FSM Verification";
        PlayerSettings.runInBackground = true;
        EditorSettings.enterPlayModeOptionsEnabled = false;
        foreach (string path in Directory.GetFiles("Assets", "SavesEditorYG2.json", SearchOption.AllDirectories))
        {
            File.Delete(path);
        }
        SessionState.SetBool(ActiveKey, true);
        SessionState.SetBool(FinishedKey, false);
        string bootScene = SessionState.GetString("MadSlime.FsmChecks.BootScene", "Menu");
        EditorSceneManager.OpenScene("Assets/MadSlime/Scenes/" + bootScene + ".unity", OpenSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }

    public static void RunSmoke()
    {
        SessionState.SetBool("MadSlime.FsmChecks.Smoke", true);
        Run();
    }

    public static void RunGameBoot()
    {
        SessionState.SetString("MadSlime.FsmChecks.BootScene", "Game");
        Run();
    }

    public static void RunFillBoot()
    {
        SessionState.SetString("MadSlime.FsmChecks.BootScene", "Fill");
        Run();
    }

    public static void RunMissingScene()
    {
        SessionState.SetBool("MadSlime.FsmChecks.MissingScene", true);
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        EditorBuildSettings.scenes = new[] { scenes[0], scenes[2] };
        Run();
    }

    public static void RunMissingRegistration()
    {
        SessionState.SetBool("MadSlime.FsmChecks.MissingRegistration", true);
        Run();
    }

    private static void Tick()
    {
        if (SessionState.GetBool(FinishedKey, false))
        {
            if (EditorApplication.isPlaying == false && EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                EditorApplication.Exit(SessionState.GetInt("MadSlime.FsmChecks.ExitCode", 1));
            }
            return;
        }
        if (EditorApplication.isPlaying == false)
        {
            return;
        }
        try
        {
            if (EditorApplication.timeSinceStartup - _phaseStarted > 90)
            {
                throw new Exception("Timeout in phase " + _phase + ", active scene " + SceneManager.GetActiveScene().name);
            }
            if (SessionState.GetBool("MadSlime.FsmChecks.MissingRegistration", false))
            {
                if (_phase == 0)
                {
                    if (Ready<MenuSceneState>() == null) return;
                    SceneManager.sceneLoaded += DisableGameRegistration;
                    _navigationChecks = Find<ProjectLifetimeScope>().Container.Resolve<SceneStateMachine>().ChangeStateAsync(SceneId.Game);
                    Next();
                    return;
                }

                if (_navigationChecks.Status == UniTaskStatus.Pending) return;
                SceneManager.sceneLoaded -= DisableGameRegistration;
                Expect<InvalidOperationException>(() => _navigationChecks.GetAwaiter().GetResult(), "Missing state registration times out with an exception");
                SceneStateMachine failedRegistration = Find<ProjectLifetimeScope>().Container.Resolve<SceneStateMachine>();
                Require(failedRegistration.IsStopped && failedRegistration.Failure.Message.Contains("register")
                    && Find<ProjectLifetimeScope>().Container.Resolve<SceneNavigator>().IsStopped,
                    "Registration timeout permanently stops machine and navigator");
                Require(GetCurrent(failedRegistration) == null
                    && GetCurrent(Find<GameLifetimeScope>().Container.Resolve<GameplayStateMachine>()) == null,
                    "Unregistered scene never enters its gameplay state");
                Finish(0);
                return;
            }
            if (SessionState.GetBool("MadSlime.FsmChecks.MissingScene", false))
            {
                if (_phase == 0)
                {
                    if (Ready<MenuSceneState>() == null) return;
                    _navigationChecks = Find<ProjectLifetimeScope>().Container.Resolve<SceneStateMachine>().ChangeStateAsync(SceneId.Game);
                    Next();
                    return;
                }

                if (_navigationChecks.Status == UniTaskStatus.Pending) return;
                Expect<Exception>(() => _navigationChecks.GetAwaiter().GetResult(), "Missing scene load error propagates");
                SceneStateMachine failedMachine = Find<ProjectLifetimeScope>().Container.Resolve<SceneStateMachine>();
                Require(failedMachine.IsStopped && failedMachine.Failure != null
                    && Find<ProjectLifetimeScope>().Container.Resolve<SceneNavigator>().IsStopped,
                    "Real Unity scene loading failure stops machine and navigator");
                Require(GetCurrent(failedMachine) == null && SceneManager.sceneCount == 1,
                    "Loading failure clears state without starting a fallback scene");
                Finish(0);
                return;
            }
            string bootScene = SessionState.GetString("MadSlime.FsmChecks.BootScene", "Menu");
            if (bootScene != "Menu")
            {
                if (bootScene == "Game" && Ready<GameSceneState>() == null) return;
                if (bootScene == "Fill" && Ready<FillSceneState>() == null) return;
                SceneStateMachine bootMachine = Find<ProjectLifetimeScope>().Container.Resolve<SceneStateMachine>();
                Require(bootMachine.CurrentSceneId.ToString() == bootScene, "Direct boot enters declared scene " + bootScene);
                if (bootScene == "Game")
                {
                    Require(GetCurrent(Find<GameLifetimeScope>().Container.Resolve<GameplayStateMachine>()) is WaitingForStartState,
                        "Direct Game boot waits for the first movement");
                    Raise(Find<PlayerInputReader>(), "MovementKeyPressed");
                    Require(GetCurrent(Find<GameLifetimeScope>().Container.Resolve<GameplayStateMachine>()) is PlayingState,
                        "Direct Game boot starts playing after movement");
                }
                YG.YG2.SetFocusWindowGame(true);
                Require(Time.timeScale == 1f && AudioListener.pause == false, "Direct boot is not paused: " + bootScene);
                Require(SceneManager.sceneCount == 1, "Direct boot does not load fallback scenes: " + bootScene);
                Finish(0);
                return;
            }
            switch (_phase)
            {
                case 0:
                    if (Ready<MenuSceneState>() == null) return;
                    DataChecks();
                    Require(GetCurrent(Find<MenuLifetimeScope>().Container.Resolve<MenuStateMachine>()) is MenuRunningState,
                        "Menu enters MenuRunningState");
                    Raise(Find<MenuNavigation>(), "PlayRequested");
                    Next();
                    break;
                case 1:
                    if (Ready<GameSceneState>() == null) return;
                    StartLoss();
                    Next();
                    break;
                case 2:
                    Accelerate();
                    if (Find<FailMenu>() == null) return;
                    CaptureFill();
                    if (SessionState.GetBool("MadSlime.FsmChecks.Smoke", false))
                    {
                        Require(Find<Rewarder>().IsWin == false, "Real adapters reach losing Fill result");
                        Finish(0);
                        return;
                    }
                    Require(_session.IsRescueUsed == false && _session.StartMode == FillStartMode.Initial,
                        "Initial loss keeps rescue unused");
                    Require(FsmLeaderboardService.Reports == 1 && FsmLeaderboardService.LastScore >= 1,
                        "Game completion commits and reports collected items before Fill");
                    FsmAdsService.Outcome = 1;
                    ClickRescue();
                    Require(_session.IsRescueUsed == false && GetCurrent(_fillMachine) is ResultState,
                        "Ad cancellation does not enter rescue");
                    Require(Get<Button>(Find<FailMenu>(), "_rescueButton").interactable,
                        "Ad cancellation unlocks rescue button");
                    FsmAdsService.Outcome = 2;
                    ClickRescue();
                    Require(_session.IsRescueUsed == false && GetCurrent(_fillMachine) is ResultState,
                        "Ad failure does not grant rescue");
                    FsmAdsService.Outcome = 0;
                    ClickRescue();
                    Next();
                    break;
                case 3:
                    Accelerate();
                    if (Find<WinMenu>() == null) return;
                    Require(_session.IsRescueUsed && _session.StartMode == FillStartMode.AfterRescue,
                        "Rewarded rescue updates shared session data");
                    Require(ReferenceEquals(_initialResult, _session.InitialResult) && _initialResult.IsWin == false,
                        "Rescue preserves original losing FillResult");
                    Require(Find<Rewarder>().IsWin && YG.YG2.saves.CurrentLevel == _levelBefore + 1,
                        "Rescue commits win and advances exactly one level");
                    Require(YG.YG2.saves.Balance - _balanceBefore == Find<Rewarder>().GrantedTotal - _rewardBefore,
                        "Rescue grants only reward top-up");
                    Require(FsmLeaderboardService.Reports == 1, "Fill and rescue do not report leaderboard");
                    Find<FillSceneState>().ReloadLevel();
                    Next();
                    break;
                case 4:
                    if (Ready<GameSceneState>() == null) return;
                    _reportsBefore = FsmLeaderboardService.Reports;
                    Find<GameSceneState>().ExitToMenu();
                    Next();
                    break;
                case 5:
                    if (Ready<MenuSceneState>() == null) return;
                    Require(FsmLeaderboardService.Reports == _reportsBefore + 1,
                        "Exit from waiting Game to Menu reports session once");
                    Require(Find<Pauser>().IsPaused() == false, "Menu is unpaused after leaving waiting Game");
                    Raise(Find<MenuNavigation>(), "PlayRequested");
                    Next();
                    break;
                case 6:
                    if (Ready<GameSceneState>() == null) return;
                    StartWin();
                    Next();
                    break;
                case 7:
                    Accelerate();
                    if (Find<WinMenu>() == null) return;
                    Require(Find<Rewarder>().IsWin, "Quota completion produces direct victory");
                    Require(YG.YG2.saves.CurrentLevel == _levelBefore + 1, "Direct victory advances exactly one level");
                    Require(_finishedCount == 1, "Game completion emits SessionFinished once");
                    Find<FillSceneState>().ReloadLevel();
                    Next();
                    break;
                case 8:
                    if (Ready<GameSceneState>() == null) return;
                    StartLoss();
                    Next();
                    break;
                case 9:
                    Accelerate();
                    if (Find<FailMenu>() == null) return;
                    CaptureFill();
                    FsmSaveConfirmation.FailNext = true;
                    ClickRescue();
                    Next();
                    break;
                case 10:
                    if (Find<FailMenu>() == null || _session.IsRescueUsed == false) return;
                    Require(GetCurrent(_fillMachine) is RescuingPrepareState,
                        "Failed rescue save does not transition to filling");
                    Require(_session.StartMode == FillStartMode.Initial, "Failed save does not enable rescue continuation");
                    Require(Get<Button>(Find<FailMenu>(), "_rescueButton").gameObject.activeSelf == false,
                        "Failed rescue save offers replay without second rescue");
                    Find<FillSceneState>().ReloadLevel();
                    Next();
                    break;
                case 11:
                    if (Ready<GameSceneState>() == null) return;
                    StartLoss();
                    Next();
                    break;
                case 12:
                    Accelerate();
                    if (Find<FailMenu>() == null) return;
                    CaptureFill();
                    FsmSaveConfirmation.WaitNext = true;
                    ClickRescue();
                    Next();
                    break;
                case 13:
                    if (GetCurrent(_fillMachine) is RescuingPrepareState == false) return;
                    Require(_session.IsRescueUsed, "Rescue is marked before waiting for save confirmation");
                    Find<FillSceneState>().ExitToMenu();
                    Next();
                    break;
                case 14:
                    if (Ready<MenuSceneState>() == null) return;
                    Require(_session.StartMode == FillStartMode.Initial,
                        "Leaving rescue cancels confirmation without starting filling");
                    Require(Find<Pauser>().IsPaused() == false, "Pause requests are balanced after rescue cancellation");
                    Raise(Find<MenuNavigation>(), "PlayRequested");
                    Next();
                    break;
                case 15:
                    if (Ready<GameSceneState>() == null) return;
                    StartLoss(false);
                    Next();
                    break;
                case 16:
                    if (Ready<FillSceneState>() == null) return;
                    Require(Find<FillLifetimeScope>().Container.Resolve<FillSessionData>().InitialResult.TargetCubes == 0,
                        "Zero-cube fill completes synchronously without losing result event");
                    Find<FillSceneState>().ExitToMenu();
                    Next();
                    break;
                case 17:
                    if (Ready<MenuSceneState>() == null) return;
                    Require(Find<WinMenu>() == null && Find<FailMenu>() == null,
                        "Leaving Fill cancels pending result UI");
                    Raise(Find<MenuNavigation>(), "PlayRequested");
                    Next();
                    break;
                case 18:
                    if (Ready<GameSceneState>() == null) return;
                    _reportsBefore = FsmLeaderboardService.Reports;
                    Raise(Find<PlayerInputReader>(), "MovementKeyPressed");
                    Find<GameSceneState>().PauseByRequest();
                    Require(Find<Pauser>().IsPaused(), "Playing state accepts pause request");
                    Find<GameSceneState>().ResumeByRequest();
                    Require(Find<Pauser>().IsPaused() == false, "Playing state resumes after pause");
                    Find<GameSceneState>().ExitToMenu();
                    Next();
                    break;
                case 19:
                    if (Ready<MenuSceneState>() == null) return;
                    Require(FsmLeaderboardService.Reports == _reportsBefore + 1,
                        "Exit from PlayingState reports session exactly once");
                    Require(Find<Pauser>().IsPaused() == false, "Playing exit leaves Menu unpaused");
                    Raise(Find<MenuNavigation>(), "PlayRequested");
                    Next();
                    break;
                case 20:
                    if (Ready<GameSceneState>() == null) return;
                    StartLoss(false);
                    QuotaCounter halfQuota = Find<GameLifetimeScope>().Container.Resolve<QuotaCounter>();
                    typeof(QuotaCounter).GetField("_collectedQuotaCount", BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(halfQuota, Math.Max(1, halfQuota.TotalQuotaTarget / 2));
                    Next();
                    break;
                case 21:
                    if (Ready<FillSceneState>() == null) return;
                    FillStateMachine activeFill = Find<FillLifetimeScope>().Container.Resolve<FillStateMachine>();
                    Require(GetCurrent(activeFill) is FillingState, "Nonzero fill remains active before interruption");
                    Find<FillSceneState>().ExitToMenu();
                    Next();
                    break;
                case 22:
                    if (Ready<MenuSceneState>() == null) return;
                    Require(Find<WinMenu>() == null && Find<FailMenu>() == null,
                        "Interrupted filling does not emit a successful completion or result UI");
                    _navigationChecks = CheckSceneContractsAsync();
                    Next();
                    break;
                case 23:
                    if (_navigationChecks.Status == UniTaskStatus.Pending) return;
                    _navigationChecks.GetAwaiter().GetResult();
                    _menuSession = Find<ProjectLifetimeScope>().Container.Resolve<MenuSessionData>();
                    _menuHandle = SceneManager.GetActiveScene().handle;
                    _navigationChecks = Find<MenuLifetimeScope>().Container.Resolve<ResetProgressFlow>().RunAsync(CancellationToken.None);
                    Next();
                    break;
                case 24:
                    if (_navigationChecks.Status == UniTaskStatus.Pending) return;
                    _navigationChecks.GetAwaiter().GetResult();
                    if (Ready<MenuSceneState>() == null) return;
                    Require(SceneManager.GetActiveScene().handle != _menuHandle, "Reset explicitly reloads the same Menu scene");
                    Require(ReferenceEquals(_menuSession, Find<ProjectLifetimeScope>().Container.Resolve<MenuSessionData>()),
                        "Daily roulette session survives Menu restart");
                    Require(Find<ProjectLifetimeScope>().Container.Resolve<ILevelStorage>().CurrentLevel == 1,
                        "Successful reset restores initial level");
                    Find<ProjectLifetimeScope>().Container.Resolve<IBalanceStorage>().SetBalance(321);
                    Find<ProjectLifetimeScope>().Container.Resolve<ILevelStorage>().SetLevelProgress(2, 2);
                    _balanceBefore = Find<Wallet>().Balance;
                    FsmSaveConfirmation.FailNext = true;
                    _navigationChecks = Find<MenuLifetimeScope>().Container.Resolve<ResetProgressFlow>().RunAsync(CancellationToken.None);
                    Next();
                    break;
                case 25:
                    if (_navigationChecks.Status == UniTaskStatus.Pending) return;
                    Expect<InvalidOperationException>(() => _navigationChecks.GetAwaiter().GetResult(), "Reset confirmation error propagates");
                    SceneStateMachine stopped = Find<ProjectLifetimeScope>().Container.Resolve<SceneStateMachine>();
                    Require(stopped.IsStopped && Find<ProjectLifetimeScope>().Container.Resolve<SceneNavigator>().IsStopped,
                        "Reset failure permanently stops scene machine and navigator");
                    Require(Find<ProjectLifetimeScope>().Container.Resolve<IBalanceStorage>().Balance == _balanceBefore,
                        "Failed reset confirmation does not reset progress");
                    Require(Find<ProjectLifetimeScope>().Container.Resolve<ILevelStorage>().CurrentLevel == 2,
                        "Failed reset confirmation preserves level progress");
                    Finish(0);
                    break;
            }
        }
        catch (Exception exception)
        {
            _results.Add("FAIL: " + exception);
            Finish(1);
        }
    }

    private static void StartLoss(bool collect = true)
    {
        IObjectResolver container = Find<GameLifetimeScope>().Container;
        Require(GetCurrent(container.Resolve<GameplayStateMachine>()) is WaitingForStartState,
            "Game prepares and waits for movement");
        _finishedCount = 0;
        Find<GameSceneState>().SessionFinished += OnSessionFinished;
        Find<GameSceneState>().PauseByRequest();
        Find<GameSceneState>().ResumeByRequest();
        Find<Game.Timer>().Setup(0.15f);
        Raise(Find<PlayerInputReader>(), "MovementKeyPressed");
        Require(GetCurrent(container.Resolve<GameplayStateMachine>()) is PlayingState,
            "Movement enters PlayingState");
        if (collect == false)
        {
            container.Resolve<GameplayActivityController>().SetRunning(false);
            container.Resolve<QuotaCounter>().ResetSession();
        }
        if (collect)
        {
            Item item = Find<LevelGenerator>().SpawnedItems[0];
            Raise(Find<Collectables.ItemCollector>(), "ItemCollected", item);
        }
    }

    private static void StartWin()
    {
        _levelBefore = YG.YG2.saves.CurrentLevel;
        _finishedCount = 0;
        Find<GameSceneState>().SessionFinished += OnSessionFinished;
        Raise(Find<PlayerInputReader>(), "MovementKeyPressed");
        QuotaCounter quota = Find<GameLifetimeScope>().Container.Resolve<QuotaCounter>();
        typeof(QuotaCounter).GetField("_collectedQuotaCount", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(quota, quota.TotalQuotaTarget);
        Raise(quota, "QuotaCompleted");
    }

    private static void CaptureFill()
    {
        _session = Find<FillLifetimeScope>().Container.Resolve<FillSessionData>();
        _fillMachine = Find<FillLifetimeScope>().Container.Resolve<FillStateMachine>();
        _initialResult = _session.InitialResult;
        _levelBefore = YG.YG2.saves.CurrentLevel;
        _balanceBefore = YG.YG2.saves.Balance;
        _rewardBefore = Find<Rewarder>().GrantedTotal;
    }

    private static void Accelerate()
    {
        FillTapButton button = Find<FillTapButton>();
        if (button != null) button.OnPointerDown(null);
    }

    private static void ClickRescue()
    {
        Get<Button>(Find<FailMenu>(), "_rescueButton").onClick.Invoke();
    }

    private static T Ready<T>() where T : Object
    {
        T state = Find<T>();
        ProjectLifetimeScope scope = Find<ProjectLifetimeScope>();
        if (scope == null) return null;
        SceneStateMachine machine = scope.Container.Resolve<SceneStateMachine>();
        if (machine.IsTransitioning || machine.IsStopped || ReferenceEquals(GetCurrent(machine), state) == false) return null;
        return state;
    }

    private static T Find<T>() where T : Object
    {
        return Object.FindObjectOfType<T>();
    }

    private static object GetCurrent(object machine)
    {
        return machine.GetType().BaseType.GetProperty("CurrentState", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(machine);
    }

    private static T Get<T>(object target, string field)
    {
        return (T)target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
    }

    private static void Raise(object target, string eventName, params object[] arguments)
    {
        Delegate callback = (Delegate)target.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(target);
        if (callback == null) throw new Exception("No subscriber for " + eventName);
        callback.DynamicInvoke(arguments);
    }

    private static void OnSessionFinished()
    {
        _finishedCount++;
    }

    private static void Require(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        _passed++;
        _results.Add("PASS: " + description);
        File.WriteAllText("output/fsm/progress.txt", string.Join("\n", _results));
    }

    private static void DataChecks()
    {
        FillSessionData data = new FillSessionData();
        Expect<InvalidOperationException>(data.MarkRescueUsed, "Rescue requires prepared session");
        Expect<ArgumentNullException>(() => data.Prepare(null), "Null initial result rejected");
        FillResult loss = new FillResult(0, 0, 10, Vector3.zero, Vector3.one);
        data.Prepare(loss);
        Expect<InvalidOperationException>(data.PrepareRescueContinuation, "Continuation requires marked rescue");
        data.MarkRescueUsed();
        Expect<InvalidOperationException>(data.MarkRescueUsed, "Second rescue rejected");
        data.PrepareRescueContinuation();
        Expect<InvalidOperationException>(data.PrepareRescueContinuation, "Duplicate continuation rejected");
        data.Prepare(loss);
        Require(data.IsRescueUsed == false && data.StartMode == FillStartMode.Initial, "New session resets rescue data");
        data.Prepare(new FillResult(10, 0, 10, Vector3.zero, Vector3.one));
        Expect<InvalidOperationException>(data.MarkRescueUsed, "Winning session rejects rescue");
    }

    private static async UniTask CheckSceneContractsAsync()
    {
        ProjectLifetimeScope scope = Find<ProjectLifetimeScope>();
        Canvas prefab = AssetDatabase.LoadAssetAtPath<Canvas>(AssetDatabase.GUIDToAssetPath("36cd546a12462c565a235f5ca971a629"));
        List<Exception> unobserved = new List<Exception>();
        Action<Exception> observe = exception => unobserved.Add(exception);
        UniTaskScheduler.UnobservedTaskException += observe;

        try
        {
            for (int scenario = 0; scenario < 8; scenario++)
            {
                SceneNavigator navigator = new SceneNavigator();
                SceneStateMachine machine = new SceneStateMachine(navigator,
                    new LoadingScreenController(prefab, scope.transform),
                    scope.Container.Resolve<ISavesReadiness>(), scope.Container.Resolve<Skins.SkinInventory>(),
                    scope.GetCancellationTokenOnDestroy());
                ProbeSceneState state = new ProbeSceneState();
                state.FailPreparation = scenario == 0;
                state.FailEntry = scenario == 1;
                state.FailExit = scenario == 2;
                state.WaitForCancellation = scenario == 7;
                int errorsBefore = unobserved.Count;
                CancellationTokenSource sceneLifetime = new CancellationTokenSource();
                machine.RegisterScene(state, SceneManager.GetActiveScene(), sceneLifetime.Token);
                if (scenario == 7)
                {
                    await UniTask.NextFrame();
                    sceneLifetime.Cancel();
                }
                await UniTask.WaitUntil(() => machine.IsStopped || machine.IsTransitioning == false)
                    .Timeout(TimeSpan.FromSeconds(15));

                if (scenario < 2)
                {
                    Require(ReferenceEquals(machine.Failure, state.Error), "Preparation/entry preserves original exception: " + scenario);
                    Require(unobserved.Count == errorsBefore + 1, "Initial failure is reported once: " + scenario);
                }
                else if (scenario == 7)
                {
                    Require(machine.Failure is OperationCanceledException, "Unexpected scene lifetime cancellation stops preparation");
                }
                else
                {
                    Require(ReferenceEquals(GetCurrent(machine), state), "Scene machine enters through base CurrentState: " + scenario);
                    try
                    {
                        if (scenario == 3)
                        {
                            machine.RegisterScene(new ProbeSceneState(), SceneManager.GetActiveScene(), CancellationToken.None);
                        }
                        else if (scenario == 4)
                        {
                            ProbeSceneState wrong = new ProbeSceneState { Id = SceneId.Fill };
                            machine.RegisterScene(wrong, SceneManager.GetActiveScene(), CancellationToken.None);
                        }
                        else if (scenario == 6)
                        {
                            navigator.Stop();
                            await machine.ChangeStateAsync(SceneId.Game);
                        }
                        else
                        {
                            await machine.ChangeStateAsync(scenario == 5 ? (SceneId)999 : SceneId.Game);
                        }

                        throw new Exception("Navigation contract violation did not throw: " + scenario);
                    }
                    catch (InvalidOperationException) { }
                    catch (ArgumentException) { }
                }

                Require(machine.IsStopped && navigator.IsStopped && machine.Failure != null,
                    "Contract failure stops machine and navigator: " + scenario);
                Require(GetCurrent(machine) == null && state.Exits == 1, "Base clears current state and exits once: " + scenario);
                try
                {
                    await machine.ChangeStateAsync(SceneId.Game);
                    throw new Exception("Stopped machine accepted another transition");
                }
                catch (InvalidOperationException) { }
                Require(machine.IsStopped && navigator.IsStopped, "Failure cannot silently resume navigation: " + scenario);
                machine.Dispose();
                sceneLifetime.Dispose();
            }
        }
        finally
        {
            UniTaskScheduler.UnobservedTaskException -= observe;
        }
    }

    private sealed class ProbeSceneState : ISceneState
    {
        public SceneId Id = SceneId.Menu;
        public bool FailPreparation;
        public bool FailEntry;
        public bool FailExit;
        public bool WaitForCancellation;
        public int Exits;
        public Exception Error = new InvalidOperationException("Injected scene lifecycle error");
        public SceneId SceneId => Id;

        public UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            if (FailPreparation) throw Error;
            if (WaitForCancellation) return UniTask.Delay(TimeSpan.FromSeconds(60), cancellationToken: cancellationToken);
            return UniTask.CompletedTask;
        }

        public void Enter()
        {
            if (FailEntry) throw Error;
        }

        public void Exit()
        {
            Exits++;
            if (FailExit) throw Error;
        }
    }

    private static void Expect<T>(Action action, string description) where T : Exception
    {
        try { action(); }
        catch (T) { Require(true, description); return; }
        throw new Exception(description);
    }

    private static void Next()
    {
        _phase++;
        _phaseStarted = EditorApplication.timeSinceStartup;
        File.WriteAllText("output/fsm/phase.txt", _phase.ToString());
    }

    private static void OnLog(string message, string stackTrace, LogType type)
    {
        if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
        if (SessionState.GetBool("MadSlime.FsmChecks.MissingScene", false)
            && message.Contains("couldn't be loaded")) return;
        _results.Add("UNITY ERROR: " + message + "\n" + stackTrace);
        Finish(1);
    }

    private static void DisableGameRegistration(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game") return;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            GameSceneState state = root.GetComponentInChildren<GameSceneState>();
            if (state != null) state.enabled = false;
        }
    }

    private static void Finish(int code)
    {
        if (SessionState.GetBool(FinishedKey, false)) return;
        SessionState.SetBool(FinishedKey, true);
        SessionState.SetInt("MadSlime.FsmChecks.ExitCode", code);
        File.WriteAllText("output/fsm/results.txt", "Passed: " + _passed + "; Exit code: " + code + "\n" + string.Join("\n", _results));
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.Exit(code);
    }
}
