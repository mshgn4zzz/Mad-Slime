using System.IO;
using UnityEditor;
using UnityEngine;
namespace Items.Editor
{
    public static class ItemFactoryTools
    {
        public const int LowestFreeLayer = 9;
        public const int HighestFreeLayer = 31;
        public const float LightIntensity = 1.2f;
        public const float CameraDistance = 10f;

        public static int ResolveFreeLayer()
        {
            for (int i = LowestFreeLayer; i <= HighestFreeLayer; i++)
            {
                if (LayerMask.LayerToName(i) == "")
                {
                    return i;
                }
            }

            return -1;
        }

        public static void SetLayerRecursively(GameObject root, int layer)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)
            {
                children[i].gameObject.layer = layer;
            }
        }

        public static void EnsureSpriteImport(string iconPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;

            if (importer == null)
            {
                Debug.LogWarning($"[ItemFactoryTools] No TextureImporter for '{iconPath}'.");
                return;
            }

            if (importer.textureType == TextureImporterType.Sprite)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

        public static string GetFolderPath(DefaultAsset folder)
        {
            string path = AssetDatabase.GetAssetPath(folder);

            if (File.Exists(path) == true)
            {
                path = Path.GetDirectoryName(path);
            }

            return path;
        }

        public static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) == true || AssetDatabase.IsValidFolder(folder) == true)
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string leaf = Path.GetFileName(folder);

            if (string.IsNullOrEmpty(parent) == false && AssetDatabase.IsValidFolder(parent) == false)
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static Texture2D RenderToTexture(GameObject modelPrefab, int iconLayer, int iconSize,
            IconRenderSettings settings)
        {
            GameObject renderRoot = new GameObject("IconRenderRoot");
            renderRoot.layer = iconLayer;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, renderRoot.transform);
            SetLayerRecursively(instance, iconLayer);

            GameObject cameraGO = new GameObject("IconCamera");
            cameraGO.transform.SetParent(renderRoot.transform);
            cameraGO.layer = iconLayer;

            Camera cam = cameraGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.orthographic = true;
            cam.aspect = 1f;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;
            cam.cullingMask = 1 << iconLayer;

            GameObject lightGO = new GameObject("IconLight");
            lightGO.transform.SetParent(renderRoot.transform);
            lightGO.layer = iconLayer;

            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(settings.LightEuler);
            light.intensity = LightIntensity;
            light.cullingMask = 1 << iconLayer;
            light.shadows = LightShadows.None;

            FitCamera(cam, instance, settings);

            if (settings.RelightToCamera == true)
            {
                light.transform.rotation = cam.transform.rotation;
            }

            RenderTexture rt = new RenderTexture(iconSize, iconSize, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;
            cam.targetTexture = rt;
            RenderTexture.active = rt;
            cam.Render();

            Texture2D tex = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0f, 0f, iconSize, iconSize), 0, 0);
            tex.Apply();

            RenderTexture.active = null;
            cam.targetTexture = null;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(renderRoot);

            return tex;
        }

        private static void FitCamera(Camera cam, GameObject instance, IconRenderSettings settings)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                cam.transform.position = new Vector3(-2f, 2f, -2f);
                cam.transform.LookAt(instance.transform.position);
                cam.orthographicSize = 1f;
                return;
            }

            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            cam.transform.position = bounds.center + settings.CameraOffset.normalized * CameraDistance;
            cam.transform.LookAt(bounds.center);

            if (settings.FitByProjectiveBounds == true)
            {
                Vector3 right = cam.transform.right;
                Vector3 up = cam.transform.up;
                Vector3 extents = bounds.extents;
                float halfWidth = Mathf.Abs(right.x) * extents.x
                    + Mathf.Abs(right.y) * extents.y + Mathf.Abs(right.z) * extents.z;
                float halfHeight = Mathf.Abs(up.x) * extents.x
                    + Mathf.Abs(up.y) * extents.y + Mathf.Abs(up.z) * extents.z;
                cam.orthographicSize = Mathf.Max(halfHeight, halfWidth / cam.aspect) * settings.OrthographicPadding;
            }
            else
            {
                cam.orthographicSize = bounds.extents.magnitude * settings.OrthographicPadding;
            }
        }
    }
}
