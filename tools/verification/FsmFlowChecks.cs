using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        EditorSceneManager.OpenScene("Assets/MadSlime/Scenes/Menu.unity", OpenSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }

    public static void RunSmoke()
    {
        SessionState.SetBool("MadSlime.FsmChecks.Smoke", true);
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
        SceneNavigator navigator = Find<SceneNavigator>();
        if (navigator == null || navigator.IsTransitioning) return null;
        T state = Find<T>();
        ProjectLifetimeScope scope = Find<ProjectLifetimeScope>();
        if (scope == null || ReferenceEquals(GetCurrent(scope.Container.Resolve<SceneStateMachine>()), state) == false) return null;
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
        _results.Add("UNITY ERROR: " + message + "\n" + stackTrace);
        Finish(1);
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
