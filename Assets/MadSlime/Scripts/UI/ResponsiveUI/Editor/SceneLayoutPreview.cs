using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.ResponsiveUI
{
    [InitializeOnLoad]
    internal static class SceneLayoutPreview
    {
        private static List<ResponsiveCanvasLayout> _layouts = new List<ResponsiveCanvasLayout>();
        private static List<ResponsiveCanvasLayout> _removedLayouts = new List<ResponsiveCanvasLayout>();
        private static Dictionary<ResponsiveCanvasLayout, bool> _orientations =
            new Dictionary<ResponsiveCanvasLayout, bool>();
        private static bool _isHierarchyDirty = true;

        static SceneLayoutPreview()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnEditorUpdate()
        {
            ApplyCurrentOrientation();
        }

        public static void ApplyCurrentOrientation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Vector2 size = Handles.GetMainGameViewSize();

            if (size.x <= 0f || size.y <= 0f)
            {
                return;
            }

            if (_isHierarchyDirty)
            {
                CollectLayouts();
                _isHierarchyDirty = false;
            }

            bool isLandscape = size.x > size.y;
            bool isChanged = false;

            foreach (ResponsiveCanvasLayout layout in _layouts)
            {
                if (layout == null)
                {
                    continue;
                }

                if (_orientations.TryGetValue(layout, out bool wasLandscape) && wasLandscape == isLandscape)
                {
                    continue;
                }

                ApplyProfile(layout, isLandscape);
                _orientations[layout] = isLandscape;
                isChanged = true;
            }

            if (isChanged)
            {
                Canvas.ForceUpdateCanvases();
                SceneView.RepaintAll();
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }

        private static void CollectLayouts()
        {
            _layouts.Clear();
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.isLoaded == false)
                {
                    continue;
                }

                if (prefabStage != null && scene == prefabStage.scene)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();

                foreach (GameObject root in roots)
                {
                    _layouts.AddRange(root.GetComponentsInChildren<ResponsiveCanvasLayout>(true));
                }
            }

            RemoveClosedLayouts();
        }

        private static void RemoveClosedLayouts()
        {
            _removedLayouts.Clear();

            foreach (KeyValuePair<ResponsiveCanvasLayout, bool> entry in _orientations)
            {
                if (entry.Key == null || _layouts.Contains(entry.Key) == false)
                {
                    _removedLayouts.Add(entry.Key);
                }
            }

            foreach (ResponsiveCanvasLayout layout in _removedLayouts)
            {
                _orientations.Remove(layout);
            }

            _removedLayouts.Clear();
        }

        private static void ApplyProfile(ResponsiveCanvasLayout layout, bool isLandscape)
        {
            if (layout.TryGetComponent(out RoulettePopupLayout roulette))
            {
                roulette.ApplyOrientation(isLandscape, false);
                return;
            }

            layout.ApplyOrientation(isLandscape);
        }

        private static void OnHierarchyChanged()
        {
            _isHierarchyDirty = true;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            _isHierarchyDirty = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            _orientations.Clear();
            _isHierarchyDirty = true;
        }

        private static void OnBeforeAssemblyReload()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            _layouts.Clear();
            _removedLayouts.Clear();
            _orientations.Clear();
        }
    }
}
