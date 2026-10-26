using System;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using ShapeFill;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FillCameraChecks
{
    private static int passed, failed;
    private static readonly System.Text.StringBuilder Report = new System.Text.StringBuilder();
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/MadSlime/Scenes/Fill.unity");
        var grid = UnityEngine.Object.FindObjectOfType<GridBuilder>();
        var camera = Camera.main;
        var fitter = UnityEngine.Object.FindObjectOfType<GridCameraFitter>();
        if (grid == null || camera == null || fitter == null)
            throw new Exception($"Fill scene setup missing: grid={grid}, camera={camera}, fitter={fitter}");
        foreach (float aspect in new[] { 9f/16, 16f/9, 1f, 9f/21, 21f/9 })
        foreach (float initialDistance in new[] { 2f, 100f, 4000f })
        {
            camera.aspect = aspect;
            grid.Build();
            camera.transform.position = grid.transform.position - camera.transform.forward * initialDistance;
            fitter.Fit(grid);
            Check($"scene aspect={aspect}, start={initialDistance}", () => Verify(grid, camera));
        }
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/MadSlime/Resources/FillArt" }))
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
            foreach (float aspect in new[] { 9f/16, 16f/9, 9f/21, 21f/9 })
            foreach (bool orthographic in new[] { false, true })
            {
                camera.aspect = aspect; camera.orthographic = orthographic;
                grid.SetShapeTexture(texture); grid.Build(); fitter.Fit(grid);
                Check($"art={texture.name}, aspect={aspect}, ortho={orthographic}", () => Verify(grid,camera));
            }
        }
        camera.orthographic = false;
        Check("small translated rectangular floor, rotated shape, rebuild", () =>
        {
            var texture = new Texture2D(80, 40);
            var pixels = new Color[3200];
            for(int y=5;y<25;y++) for(int x=50;x<75;x++) pixels[x+y*80]=Color.white;
            texture.SetPixels(pixels); texture.Apply();
            grid.SetShapeTexture(texture);
            grid.transform.rotation=Quaternion.Euler(90,37,0);
            var floor=new Bounds(new Vector3(85,0,-42),new Vector3(18,.1f,9));
            grid.SetPlayArea(floor);
            for(int repeat=0;repeat<3;repeat++)
            {
                grid.Build();
                camera.aspect=9f/16; fitter.Fit(grid); Verify(grid,camera);
                foreach(var cell in grid.BorderCells)
                for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2)
                {
                    var p=grid.transform.position+(grid.GridToWorld(cell.x,cell.y)-grid.transform.position)*1.1f
                        +new Vector3(x,0,z)*grid.CellSize*.55f;
                    if(p.x<floor.min.x || p.x>floor.max.x || p.z<floor.min.z || p.z>floor.max.z)
                        throw new Exception("pulse crosses floor: "+p);
                }
                camera.aspect=21f/9;
                typeof(GridCameraFitter).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(fitter,null);
                Verify(grid,camera);
            }
            UnityEngine.Object.DestroyImmediate(texture);
        });
        Check("resize during pulse/FOV kick preserves resting fit", () =>
        {
            grid.SetShapeTexture(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MadSlime/Resources/FillArt/Robot.png"));
            camera.aspect=9f/16; camera.fieldOfView=60f; grid.Build(); fitter.Fit(grid);
            grid.transform.localScale=Vector3.one*1.06f; camera.fieldOfView=69f; camera.aspect=16f/9;
            typeof(GridCameraFitter).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(fitter,null);
            grid.transform.localScale=Vector3.one; camera.fieldOfView=60f;
            Verify(grid,camera);
        });
        Directory.CreateDirectory("output/camera-fit");
        File.WriteAllText("output/camera-fit/checks.txt", $"Passed: {passed}; Failed: {failed}\n" + Report);
        Debug.Log(Report.ToString());
        EditorApplication.Exit(failed == 0 ? 0 : 1);
    }
    private const string LivePending = "FillCameraChecks.LivePending";
    public static void Live()
    {
        EditorSceneManager.OpenScene("Assets/MadSlime/Scenes/Fill.unity");
        SessionState.SetBool(LivePending,true);
        EditorApplication.EnterPlaymode();
    }
    [InitializeOnLoadMethod]
    private static void ResumeLive()
    {
        EditorApplication.update += WaitForLive;
    }
    private static void WaitForLive()
    {
        if(!SessionState.GetBool(LivePending,false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        SessionState.SetBool(LivePending,false); EditorApplication.update-=WaitForLive;
        VerifyLive().Forget();
    }
    private static async UniTaskVoid VerifyLive()
    {
        Application.runInBackground=true; Application.targetFrameRate=60;
        var errors=new List<string>();
        Application.LogCallback onLog=(message,stack,type)=> { if(type==LogType.Exception || type==LogType.Error) errors.Add(message); };
        Application.logMessageReceived+=onLog;
        try
        {
            var grid=UnityEngine.Object.FindObjectOfType<GridBuilder>();
            var shape=UnityEngine.Object.FindObjectOfType<ShapeFillOrchestrator>();
            var camera=Camera.main;
            bool completed=false; shape.FillCompleted+=p=>completed=true;
            float started=Time.realtimeSinceStartup;
            int frame=Time.frameCount;
            while(Time.realtimeSinceStartup-started<4f) await UniTask.Yield();
            if(Time.frameCount-frame<10 || grid.BorderCells.Count==0) throw new Exception("scene did not prepare/advance");
            Verify(grid,camera);
            shape.Accelerate(); shape.Accelerate();
            while(!completed && Time.realtimeSinceStartup-started<18f) await UniTask.Yield();
            if(!completed) throw new Exception("fill did not finish");
            float pulseStarted=Time.realtimeSinceStartup;
            var renderers=grid.GetComponentsInChildren<MeshRenderer>();
            while(Time.realtimeSinceStartup-pulseStarted<1.5f)
            {
                foreach(var renderer in renderers)
                {
                    Bounds bounds=renderer.bounds;
                    for(int x=-1;x<=1;x+=2) for(int y=-1;y<=1;y+=2) for(int z=-1;z<=1;z+=2)
                    {
                        var p=camera.WorldToViewportPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z)));
                        if(p.z<=camera.nearClipPlane || p.x<.079f || p.x>.921f || p.y<.119f || p.y>.881f)
                            throw new Exception("actual cube renderer clipped during completion: "+p);
                    }
                }
                await UniTask.Yield();
            }
            Verify(grid,camera);
            if(errors.Count>0) throw new Exception(string.Join("; ",errors));
            Directory.CreateDirectory("output/camera-fit");
            File.WriteAllText("output/camera-fit/live.txt",$"PASS: actual Fill scene prepared, framed and completed; frames={Time.frameCount-frame}; runtime errors=0");
            EditorApplication.Exit(0);
        }
        catch(Exception e)
        {
            Directory.CreateDirectory("output/camera-fit");
            File.WriteAllText("output/camera-fit/live.txt","FAIL: "+e+"\n"+string.Join("\n",errors));
            Debug.LogException(e); EditorApplication.Exit(1);
        }
    }

    public static void Render()
    {
        EditorSceneManager.OpenScene("Assets/MadSlime/Scenes/Fill.unity");
        var grid = UnityEngine.Object.FindObjectOfType<GridBuilder>();
        var camera = Camera.main;
        var fitter = UnityEngine.Object.FindObjectOfType<GridCameraFitter>();
        var spawner = grid.GetComponent<CubeSpawner>(); spawner.Initialize();
        Directory.CreateDirectory("output/camera-fit");
        foreach(string art in new[]{"Robot","FrogKing"})
        foreach(bool portrait in new[]{true,false})
        {
            var rt=new RenderTexture(portrait?720:1280,portrait?1280:720,24);
            camera.targetTexture=rt; camera.aspect=rt.width/(float)rt.height;
            grid.SetShapeTexture(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MadSlime/Resources/FillArt/"+art+".png"));
            grid.Build(); fitter.Fit(grid);
            var cubes=new List<GameObject>();
            foreach(var cell in grid.BorderCells)
                cubes.Add(spawner.Spawn(grid.GridToWorld(cell.x,cell.y),Quaternion.identity,grid.CellSize,Color.black).gameObject);
            foreach(var cell in grid.FillCells)
                cubes.Add(spawner.Spawn(grid.GridToWorld(cell.x,cell.y),Quaternion.identity,grid.CellSize,grid.GetPixelColor(cell.x,cell.y)).gameObject);
            camera.Render(); RenderTexture.active=rt;
            var image=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0); image.Apply();
            File.WriteAllBytes("output/camera-fit/"+art+(portrait?"-portrait":"-landscape")+".png",image.EncodeToPNG());
            foreach(var cube in cubes) UnityEngine.Object.DestroyImmediate(cube);
            RenderTexture.active=null; camera.targetTexture=null;
            UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(rt);
        }
        EditorApplication.Exit(0);
    }

    private static void Verify(GridBuilder grid, Camera camera)
    {
        float minX=1, maxX=0, minY=1, maxY=0;
        var cells = new List<Vector2Int>(grid.BorderCells); cells.AddRange(grid.FillCells);
        foreach (var cell in cells)
        for(int x=-1;x<=1;x+=2) for(int y=-1;y<=1;y+=2) for(int z=-1;z<=1;z+=2)
        {
            // Actual cubes are axis aligned in world space. Include the final shape pulse.
            var world = grid.transform.position + (grid.GridToWorld(cell.x,cell.y)-grid.transform.position)*1.1f
                + new Vector3(x,y,z)*grid.CellSize*.55f;
            var p = camera.WorldToViewportPoint(world);
            if(p.z<=camera.nearClipPlane || p.z>=camera.farClipPlane || p.x<.079f || p.x>.921f || p.y<.119f || p.y>.881f)
                throw new Exception("cube clipped: " + p + ", camera="+camera.transform.position);
            minX=Mathf.Min(minX,p.x); maxX=Mathf.Max(maxX,p.x); minY=Mathf.Min(minY,p.y); maxY=Mathf.Max(maxY,p.y);
        }
        if (Mathf.Max((maxX-minX)/.84f,(maxY-minY)/.76f) < .995f)
            throw new Exception($"shape too small: width={maxX-minX}, height={maxY-minY}");
    }
    private static void Check(string name, Action action)
    {
        try { action(); passed++; Report.AppendLine("PASS: "+name); }
        catch(Exception e) { failed++; Report.AppendLine("FAIL: "+name+": "+e.Message); }
    }
}
