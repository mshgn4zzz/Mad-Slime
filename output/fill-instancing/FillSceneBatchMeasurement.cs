using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using MadSlime.DI;
using MadSlime.GameFSM;
using MadSlime.GameFSM.Interfaces;
using MadSlime.GameFSM.Scenes;
using MadSlime.ShapeFill.Logic;
using MadSlime.ShapeFill.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Profiling;
using VContainer;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class FillSceneBatchMeasurement
{
    const string Key = "FillSceneBatchMeasurement.Run";
    const string Report = "output/fill-instancing/full-scene.txt";
    static int stage, frame = -1, count, samples;
    static double deadline, readyAt;
    static long sum, min = long.MaxValue, max;
    static Material material;
    static ProfilerRecorder batches;
    static ShapeFiller filler;
    static readonly List<string> Errors = new List<string>();
    static FillSceneBatchMeasurement()
    {
        if (!SessionState.GetBool(Key, false)) return;
        deadline = EditorApplication.timeSinceStartup + 120;
        EditorApplication.update += Tick;
        Application.logMessageReceived += (message, trace, kind) => { if (kind == LogType.Exception || kind == LogType.Error || kind == LogType.Assert) Errors.Add(message + "\n" + trace); };
    }
    public static void Run()
    {
        Directory.CreateDirectory("output/fill-instancing");
        File.WriteAllText(Report, "Actual Fill scene, Menu -> Game -> Fill, full fill, native gameplay and UI.\n");
        SessionState.SetBool(Key, true);
        EditorSceneManager.OpenScene("Assets/MadSlime/Scenes/Menu.unity");
        EditorApplication.EnterPlaymode();
    }
    static void Invoke(object target, string method) { target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null); }
    static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        try
        {
            Application.runInBackground = true;
            if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Timeout at stage " + stage);
            if (Errors.Count > 0) throw new Exception(string.Join("\n", Errors));
            var project = Object.FindObjectOfType<ProjectLifetimeScope>();
            if (project == null || project.Container == null) return;
            var scenes = project.Container.Resolve<SceneStateMachine>();
            if (scenes.Failure != null) throw scenes.Failure;
            if (scenes.IsTransitioning) return;
            var state = (ISceneState)scenes.GetType().BaseType.GetProperty("CurrentState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(scenes);
            if (stage == 0 && state is MenuSceneState)
            { stage = 1; Invoke(state, "OnPlayRequested"); }
            else if (stage == 1 && state is GameSceneState)
            { stage = 2; Invoke(state, "OnFillRequested"); }
            else if (stage == 2 && state is FillSceneState)
            {
                var camera = Camera.main;
                camera.targetTexture = new RenderTexture(1024,768,24);
                camera.targetTexture.Create();
                foreach (var canvas in Object.FindObjectsOfType<Canvas>())
                {
                    if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = camera.nearClipPlane + 1f; }
                }
                var grid = Object.FindObjectOfType<GridBuilder>();
                filler = Object.FindObjectOfType<ShapeFiller>();
                var bounds = grid.GetFormBounds();
                filler.ShowResult(new FillResult(grid.FillCells.Count, 0, grid.FillCells.Count, bounds.center, bounds.size));
                filler.Accelerate(); filler.Accelerate();
                material = AssetDatabase.LoadAssetAtPath<Material>("Assets/MadSlime/Resources/Materials/FlyingCubePlastic.mat");
                material.enableInstancing = false;
                batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
                stage = 3; readyAt = EditorApplication.timeSinceStartup + 8;
            }
            else if (stage == 3 && EditorApplication.timeSinceStartup > readyAt && filler.GetFormFillFraction() >= 1f)
            {
                count = Object.FindObjectsOfType<FlyingCube>().Length;
                File.AppendAllText(Report, "Unity=" + Application.unityVersion + " GPU=" + SystemInfo.graphicsDeviceName + " API=" + SystemInfo.graphicsDeviceType + "\nCubes=" + count + "\n");
                stage = 4;
            }
            else if (stage == 4 || stage == 6)
            {
                if (frame == Time.frameCount) return;
                frame = Time.frameCount;
                long value = batches.LastValue;
                sum += value; min = Math.Min(min,value); max = Math.Max(max,value); samples++;
                if (samples < 60) return;
                File.AppendAllText(Report, "instancing=" + material.enableInstancing + " batchesMean=" + sum/(double)samples + " min=" + min + " max=" + max + " samples=" + samples + "\n");
                Capture(material.enableInstancing ? "full-scene-after" : "full-scene-before");
                if (stage == 6) { Finish(0); return; }
                material.enableInstancing = true;
                stage = 5; readyAt = EditorApplication.timeSinceStartup + 2;
                sum = max = samples = 0; min = long.MaxValue;
            }
            else if (stage == 5 && EditorApplication.timeSinceStartup > readyAt) stage = 6;
        }
        catch (Exception ex) { File.AppendAllText(Report, "FAIL: " + ex + "\n"); Finish(1); }
    }
    static void Capture(string label)
    {
        var camera = Camera.main;
        var previous = camera.targetTexture;
        var active = RenderTexture.active;
        var rt = new RenderTexture(1024, 768, 24);
        camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
        var image = new Texture2D(1024,768,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1024,768),0,0); image.Apply();
        File.WriteAllBytes("output/fill-instancing/" + label + ".png", image.EncodeToPNG());
        camera.targetTexture = previous; RenderTexture.active = active;
        Object.DestroyImmediate(image); rt.Release(); Object.DestroyImmediate(rt);
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Key,false); EditorApplication.update -= Tick;
        if (material != null) material.enableInstancing = true;
        batches.Dispose(); EditorApplication.Exit(code);
    }
}
