using System.Collections.Generic;
using MadSlime.UI.ResponsiveUI.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MadSlime.UI.ResponsiveUI.Editor
{
    internal class SceneLayoutPreview
    {
        private List<ResponsiveCanvasLayout> _layouts = new List<ResponsiveCanvasLayout>();
        private List<ResponsiveCanvasLayout> _removedLayouts = new List<ResponsiveCanvasLayout>();
        private Dictionary<ResponsiveCanvasLayout, bool> _orientations =
            new Dictionary<ResponsiveCanvasLayout, bool>();
        private bool _isHierarchyRefreshRequired = true;

        public void ApplyCurrentOrientation()
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

            if (_isHierarchyRefreshRequired)
            {
                CollectLayouts();
                _isHierarchyRefreshRequired = false;
            }

            bool isLandscape = size.x > size.y;
            bool isChanged = false;

            foreach (ResponsiveCanvasLayout layout in _layouts)
            {
                if (layout == null)
                {
                    continue;
                }

                if (_orientations.TryGetValue(layout, out bool isPreviouslyLandscape)
                    && isPreviouslyLandscape == isLandscape)
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

        public void Subscribe()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        public void Unsubscribe()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            _layouts.Clear();
            _removedLayouts.Clear();
            _orientations.Clear();
        }

        private void CollectLayouts()
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

        private void RemoveClosedLayouts()
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

        private void ApplyProfile(ResponsiveCanvasLayout layout, bool isLandscape)
        {
            if (layout.TryGetComponent(out RoulettePopupLayout roulette))
            {
                roulette.ApplyOrientation(isLandscape, false);
                return;
            }

            layout.ApplyOrientation(isLandscape);
        }

        private void OnEditorUpdate()
        {
            ApplyCurrentOrientation();
        }

        private void OnHierarchyChanged()
        {
            _isHierarchyRefreshRequired = true;
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            _isHierarchyRefreshRequired = true;
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            _orientations.Clear();
            _isHierarchyRefreshRequired = true;
        }
    }
}
