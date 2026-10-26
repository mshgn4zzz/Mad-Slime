using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Game;
using Scriptables;
using ShapeFill;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Upgrades;
using YG;

public static class ProgressionFillChecks
{
    private static readonly List<string> Failures = new List<string>();
    private static int _passed;
    private const string Output = "output/progression-fill";

    public static void Run()
    {
        Directory.CreateDirectory(Output);
        Check("New player starts at zero", () => Require(new SavesYG().Balance == 0, "new save has non-zero balance"));
        Check("Upgrade budget reached on win 50", Economy);
        Check("Location ranges and cycle", Catalog);
        Check("Exact resolution and rectangular aspect", Sampling);
        Check("All art fits portrait and landscape including pulse", Framing);
        Check("Spawn schedule finishes at every frame rate", Schedule);
        string report = $"Passed: {_passed}; Failed: {Failures.Count}\n" + string.Join("\n", Failures);
        File.WriteAllText(Output + "/checks.txt", report);
        Debug.Log(report);
        EditorApplication.Exit(Failures.Count == 0 ? 0 : 1);
    }

    public static void Render()
    {
        Directory.CreateDirectory(Output);
        EditorSceneManager.OpenScene("Assets/MadSlime/Scenes/Fill.unity", OpenSceneMode.Single);
        var grid = UnityEngine.Object.FindObjectOfType<GridBuilder>();
        var spawner = grid.GetComponent<CubeSpawner>();
        var camera = Camera.main;
        spawner.Initialize();
        foreach (string art in new[] { "Robot", "FrogKing" })
        {
            foreach (bool portrait in new[] { false, true })
            {
                int width = portrait ? 720 : 1280;
                int height = portrait ? 1280 : 720;
                var target = new RenderTexture(width, height, 24);
                camera.targetTexture = target;
                camera.aspect = width / (float)height;
                grid.SetShapeTexture(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MadSlime/Resources/FillArt/" + art + ".png"));
                grid.Build();
                var spawned = new List<GameObject>();
                foreach (var cell in grid.BorderCells)
                    spawned.Add(spawner.Spawn(grid.GridToWorld(cell.x, cell.y), Quaternion.identity, grid.CellSize, Color.black).gameObject);
                foreach (var cell in grid.FillCells)
                    spawned.Add(spawner.Spawn(grid.GridToWorld(cell.x, cell.y), Quaternion.identity, grid.CellSize, grid.GetPixelColor(cell.x, cell.y)).gameObject);
                camera.Render();
                RenderTexture.active = target;
                var image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                File.WriteAllBytes(Output + "/" + art + (portrait ? "-portrait" : "-landscape") + ".png", image.EncodeToPNG());
                foreach (var go in spawned) UnityEngine.Object.DestroyImmediate(go);
                RenderTexture.active = null; camera.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(target);
            }
        }
        EditorApplication.Exit(0);
    }

    private static void Economy()
    {
        var upgrades = AssetDatabase.LoadAssetAtPath<UpgradesConfig>("Assets/MadSlime/Scriptables/Upgrades/UpgradesConfig.asset");
        var rewards = AssetDatabase.LoadAssetAtPath<RewardConfig>("Assets/MadSlime/Scriptables/AD/RewardConfig.asset");
        int cost = 0;
        foreach (var upgrade in upgrades.Upgrades)
        {
            int step = upgrade.BaseCost;
            for (int i = 0; i < upgrade.MaxSteps; i++) { cost += step; step *= upgrade.CostMultiplier; }
        }
        foreach (var perk in upgrades.Perks) cost += perk.Cost;
        Require(cost == 393000, "unexpected full-upgrade cost " + cost);
        int earned = 0;
        for (int i = 1; i <= 49; i++) earned += rewards.GetReward(i);
        Require(earned < cost, "already maxed before 50");
        earned += rewards.GetReward(50);
        Require(earned >= cost, "50 wins earn only " + earned);
        File.WriteAllText(Output + "/economy.txt", "Cost: " + cost + "\nEarned at 50: " + earned);
    }

    private static void Catalog()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<LevelsCatalog>("Assets/MadSlime/Scriptables/Levels/LevelsCatalog.asset");
        var resolver = new LevelConfigResolver(catalog);
        string[] locations = { "Table", "Room", "Cosmetics", "ToyStore", "School", "Office", "Bank", "Backrooms", "BlockGarden", "Forest", "Parking", "City", "Space", "Universe" };
        Require(catalog.Ranges.Count == locations.Length, "location count");
        int[] starts = { 1, 4, 7, 10, 13, 16, 19, 23, 27, 31, 35, 39, 43, 47 };
        int[] ends = { 3, 6, 9, 12, 15, 18, 22, 26, 30, 34, 38, 42, 46, 50 };
        for (int i = 0; i < locations.Length; i++)
        {
            Require(catalog.Ranges[i].FromLevel == starts[i] && catalog.Ranges[i].ToLevel == ends[i], "incorrect range " + i);
            for (int level = starts[i]; level <= ends[i]; level++)
                Require(resolver.GetConfigFor(level).name == locations[i] + "LevelConfig", "wrong location at level " + level);
        }
        Require(resolver.GetConfigFor(51) == resolver.GetConfigFor(1), "cycle does not restart");
    }

    private static void Sampling()
    {
        var go = new GameObject("Grid sampling check");
        var texture = new Texture2D(1254, 627);
        try
        {
            var pixels = new Color[1254 * 627];
            for (int y = 0; y < 627; y++)
                for (int x = 0; x < 1254; x++) pixels[x + y * 1254] = new Color(x / 1253f, y / 626f, 0f, 1f);
            texture.SetPixels(pixels); texture.Apply();
            var grid = go.AddComponent<GridBuilder>();
            Set(grid, "_gridResolution", 96); Set(grid, "_cellSize", 2f);
            grid.SetShapeTexture(texture); grid.Build();
            Require(grid.Width == 96 && grid.Height == 48, $"got {grid.Width}x{grid.Height}, expected 96x48");
            Require(Mathf.Max(grid.Width, grid.Height) * grid.CellSize <= 64.001f, "world footprint grew with detail");
            Require(grid.GetPixelColor(0, 0).r > 0f, "samples edge rather than center");
            Require(grid.BorderCells.Count + grid.FillCells.Count == 96 * 48, "lost opaque cells");
        }
        finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(texture); }
    }

    private static void Framing()
    {
        EditorSceneManager.OpenScene("Assets/MadSlime/Scenes/Fill.unity", OpenSceneMode.Single);
        var grid = UnityEngine.Object.FindObjectOfType<GridBuilder>();
        var camera = Camera.main;
        Require(grid != null && camera != null, "scene setup missing");
        Require(new SerializedObject(grid).FindProperty("_gridResolution").intValue == 96, "fill scene still uses coarse grid");
        int count = 0;
        foreach (string asset in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/MadSlime/Resources/FillArt" }))
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(asset));
            foreach (float aspect in new[] { 9f / 16f, 16f / 9f, 3f / 4f, 4f / 3f })
            {
                camera.aspect = aspect;
                grid.SetShapeTexture(texture); grid.Build();
                Require(grid.Width == 96 && grid.Height == 96, texture.name + " resolution");
                float halfX = grid.Width * grid.CellSize * .5f * 1.1f;
                float halfY = grid.Height * grid.CellSize * .5f * 1.1f;
                for (int x = -1; x <= 1; x += 2)
                    for (int y = -1; y <= 1; y += 2)
                        for (int z = -1; z <= 1; z += 2)
                        {
                            var p = camera.WorldToViewportPoint(grid.transform.TransformPoint(new Vector3(x * halfX, y * halfY, z * grid.CellSize)));
                            Require(p.z > camera.nearClipPlane && p.x >= .079f && p.x <= .921f && p.y >= .119f && p.y <= .881f,
                                texture.name + " clipped at aspect " + aspect + ": " + p);
                        }
                Require(grid.FillCells.Count > 0, texture.name + " no fill cells");
            }
            count++;
        }
        File.WriteAllText(Output + "/framing.txt", count + " artworks, four aspect ratios, 10% pulse margin");
    }

    private static void Schedule()
    {
        var type = typeof(GridBuilder).Assembly.GetType("ShapeFill.CubeSpawnSchedule");
        Require(type != null, "duration-based spawn schedule is missing");
        var method = type.GetMethod("GetTargetCount", BindingFlags.Public | BindingFlags.Static);
        Func<float, float, int, int> due = (elapsed, duration, count) => (int)method.Invoke(null, new object[] { elapsed, duration, count });
        Require(due(0f, 8f, 4000) == 0, "spawned before the time window");
        Require(due(.8f, 8f, 4000) == 400, "incorrect progress");
        Require(due(8f, 8f, 4000) == 4000 && due(20f, 8f, 4000) == 4000, "misses or overfills target");
        Require(due(-1f, 8f, 4000) == 0 && due(0f, 0f, 4000) == 4000, "window boundaries");
        foreach (int fps in new[] { 15, 30, 60, 144 })
        {
            int previous = 0;
            for (int frame = 1; frame <= fps * 8; frame++)
            {
                int count = due(frame / (float)fps, 8f, 4000);
                Require(count >= previous, "schedule moved backwards");
                Require(count - previous <= Mathf.CeilToInt(4000f / (fps * 8)) + 1, "all cubes spawned in a single burst");
                previous = count;
            }
            Require(previous == 4000, "unfinished at fps " + fps);
        }
    }

    private static void Check(string name, Action action)
    {
        try { action(); _passed++; Debug.Log("PASS: " + name); }
        catch (Exception e) { Failures.Add("FAIL: " + name + ": " + e); Debug.LogError(Failures[Failures.Count - 1]); }
    }
    private static void Require(bool ok, string message) { if (!ok) throw new Exception(message); }
    private static void Set(object target, string field, object value)
    {
        target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
