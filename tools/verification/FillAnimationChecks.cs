using System;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Scriptables;
using ShapeFill;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FillAnimationChecks
{
    private const string Pending = "MadSlime.FillAnimationChecks.Pending";

    public static void Run()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Pending, true);
        EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    private static void Resume()
    {
        EditorApplication.update += WaitForPlay;
    }

    private static void WaitForPlay()
    {
        if (!SessionState.GetBool(Pending, false) || !EditorApplication.isPlaying || EditorApplication.isCompiling)
            return;
        SessionState.SetBool(Pending, false);
        EditorApplication.update -= WaitForPlay;
        VerifyAsync().Forget();
    }

    private static async UniTaskVoid VerifyAsync()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
        var report = new System.Text.StringBuilder();
        try
        {
            foreach (int taps in new[] { 0, 1, 2, 3 })
            {
                var root = new GameObject("Fill timing check"); root.SetActive(false);
                var grid = root.AddComponent<GridBuilder>();
                var spawner = root.AddComponent<CubeSpawner>();
                var filler = root.AddComponent<ShapeFiller>();
                var parent = new GameObject("Cubes").transform; parent.SetParent(root.transform, false);
                Set(grid, "_gridResolution", 96);
                grid.SetShapeTexture(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MadSlime/Resources/FillArt/Robot.png"));
                Set(spawner, "_cubePrefab", AssetDatabase.LoadAssetAtPath<FlyingCube>("Assets/MadSlime/Resources/Prefabs/FlyingCube.prefab"));
                Set(spawner, "_cubesParent", parent);
                var config = AssetDatabase.LoadAssetAtPath<FillConfig>("Assets/MadSlime/Scriptables/Fill/FillConfig.asset");
                filler.Construct(grid, spawner, config);
                root.SetActive(true);
                filler.Initialize(); filler.BuildShape();
                float started = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - started < 1f) await UniTask.Yield();
                Require(parent.childCount == grid.BorderCells.Count, "border cascade did not finish in its window");
                bool done = false;
                float percent = -1f;
                filler.FillCompleted += value => { done = true; percent = value; };
                int frames = Time.frameCount;
                started = Time.realtimeSinceStartup;
                filler.ShowResult(new FillResult(grid.FillCells.Count, 0, grid.FillCells.Count));
                for (int i = 0; i < Mathf.Min(taps, 2); i++) filler.Accelerate();
                if (taps == 3)
                {
                    float stopAt = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - stopAt < .05f) await UniTask.Yield();
                    filler.StopFill();
                    int count = parent.childCount;
                    stopAt = Time.realtimeSinceStartup;
                    while (Time.realtimeSinceStartup - stopAt < .6f) await UniTask.Yield();
                    Require(parent.childCount == count && !done, "instant fill kept spawning after stop");
                    report.AppendLine("PASS: instant cancellation");
                }
                else
                {
                    float limit = taps == 0 ? 12f : taps == 1 ? 6f : 2f;
                    while (!done && Time.realtimeSinceStartup - started < limit) await UniTask.Yield();
                    float elapsed = Time.realtimeSinceStartup - started;
                    Require(done && percent == 1f, "fill did not complete at taps=" + taps);
                    Require(parent.childCount == grid.BorderCells.Count + grid.FillCells.Count, "wrong total cube count");
                    Require(Time.frameCount - frames > 1, "player loop did not advance");
                    foreach (var animator in parent.GetComponentsInChildren<CubeFlightAnimator>())
                        Require(!animator.enabled, "landed cube still runs Update");
                    report.AppendLine($"PASS: taps={taps}, cubes={parent.childCount}, time={elapsed:F3}s, frames={Time.frameCount - frames}");
                }
                UnityEngine.Object.Destroy(root);
                await UniTask.Yield();
            }
            File.WriteAllText("output/progression-fill/animation.txt", report.ToString());
            Debug.Log(report.ToString());
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            report.AppendLine("FAIL: " + e);
            File.WriteAllText("output/progression-fill/animation.txt", report.ToString());
            Debug.LogError(report.ToString());
            EditorApplication.Exit(1);
        }
    }

    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Set(object target, string field, object value)
    {
        target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
