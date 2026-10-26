using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UI.ResponsiveUI
{
    [InitializeOnLoad]
    internal static class PrefabLayoutPreview
    {
        public enum PreviewSource
        {
            GameView,
            Portrait,
            Landscape
        }

        private const float PortraitWidth = 1080f;
        private const float PortraitHeight = 1920f;
        private const float ResizeThresholdSquared = 0.5f;
        private const string SpinButtonProperty = "_spinButton";
        private const string AdButtonProperty = "_adButton";

        private static PrefabStage _stage;
        private static PrefabStage _pendingStage;
        private static ResponsiveCanvasLayout _layout;
        private static PreviewSource _source;
        private static bool _isLandscape;
        private static bool _isWithoutControls;
        private static bool _isSaving;
        private static Vector2 _size;
        private static List<PrefabCanvasSurface> _surfaces = new List<PrefabCanvasSurface>();
        private static Dictionary<GameObject, bool> _rouletteControls = new Dictionary<GameObject, bool>();

        public static bool IsLandscape => _isLandscape;

        static PrefabLayoutPreview()
        {
            PrefabStage.prefabStageOpened += OnStageOpened;
            PrefabStage.prefabSaving += OnBeforeSave;
            PrefabStage.prefabSaved += OnAfterSave;
            PrefabStage.prefabStageClosing += OnStageClosing;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.update += OnEditorUpdate;
            Undo.undoRedoPerformed += OnUndoRedo;

            PrefabStage currentStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (currentStage != null)
            {
                OnStageOpened(currentStage);
            }
        }

        public static PreviewSource GetSource(ResponsiveCanvasLayout layout)
        {
            if (_stage != null && _stage == PrefabStageUtility.GetPrefabStage(layout.gameObject))
            {
                return _source;
            }

            return PreviewSource.GameView;
        }

        public static void Configure(ResponsiveCanvasLayout layout, PreviewSource source, bool withoutControls,
            bool forceProfile = false)
        {
            PrefabStage stage = PrefabStageUtility.GetPrefabStage(layout.gameObject);

            if (stage == null || _isSaving)
            {
                return;
            }

            Vector2 size = GetSurfaceSize(source);
            bool isLandscape = size.x > size.y;
            bool isOpened = _stage != stage;
            bool shouldApplyProfile = isOpened || forceProfile || _isLandscape != isLandscape || _source != source ||
                (_layout == layout && _isWithoutControls != withoutControls);
            bool isResized = isOpened || (_size - size).sqrMagnitude > ResizeThresholdSquared;

            if (isOpened)
            {
                OpenStage(stage);
            }

            _layout = layout;
            _source = source;
            _size = size;
            _isLandscape = isLandscape;
            _isWithoutControls = withoutControls;

            if (shouldApplyProfile)
            {
                ApplyProfiles();
            }

            if (isResized == false && shouldApplyProfile == false)
            {
                return;
            }

            RefreshSurfaces();
            FrameSurface();
        }

        private static Vector2 GetSurfaceSize(PreviewSource source)
        {
            Vector2 size;

            switch (source)
            {
                case PreviewSource.Landscape:
                    size = new Vector2(PortraitHeight, PortraitWidth);
                    break;
                case PreviewSource.Portrait:
                    size = new Vector2(PortraitWidth, PortraitHeight);
                    break;
                default:
                    size = Handles.GetMainGameViewSize();
                    break;
            }

            if (size.x <= 0f || size.y <= 0f)
            {
                size = new Vector2(PortraitWidth, PortraitHeight);
            }

            Vector2 reference = new Vector2(PortraitWidth, PortraitHeight);

            if (size.x > size.y)
            {
                reference = new Vector2(PortraitHeight, PortraitWidth);
            }

            float scale = Mathf.Min(size.x / reference.x, size.y / reference.y);
            return size / scale;
        }

        private static void OpenStage(PrefabStage stage)
        {
            Clear();
            _stage = stage;
            HashSet<Canvas> seenCanvases = new HashSet<Canvas>();
            Canvas[] canvases = stage.prefabContentsRoot.GetComponentsInChildren<Canvas>(true);

            foreach (Canvas canvas in canvases)
            {
                Canvas[] parents = canvas.GetComponentsInParent<Canvas>(true);

                for (int i = parents.Length - 1; i >= 0; i--)
                {
                    if (seenCanvases.Add(parents[i]))
                    {
                        _surfaces.Add(new PrefabCanvasSurface(parents[i]));
                    }
                }
            }
        }

        private static void ApplyProfiles()
        {
            Undo.RegisterFullObjectHierarchyUndo(_stage.prefabContentsRoot,
                "Edit orientation profile");
            ResponsiveCanvasLayout[] profiles =
                _stage.prefabContentsRoot.GetComponentsInChildren<ResponsiveCanvasLayout>(true);

            foreach (ResponsiveCanvasLayout profile in profiles)
            {
                if (profile.TryGetComponent(out RoulettePopupLayout roulette))
                {
                    RememberControls(roulette);
                    roulette.ApplyOrientation(_isLandscape, _isWithoutControls);
                }
                else
                {
                    profile.ApplyOrientation(_isLandscape);
                }
            }

            ApplyControlsPreview();
        }

        private static void OnStageOpened(PrefabStage stage)
        {
            _pendingStage = stage;
            EditorApplication.delayCall -= OnStageReady;
            EditorApplication.delayCall += OnStageReady;
        }

        private static void OnStageReady()
        {
            EditorApplication.delayCall -= OnStageReady;
            PrefabStage stage = _pendingStage;
            _pendingStage = null;

            if (stage == null || stage != PrefabStageUtility.GetCurrentPrefabStage() || _stage == stage)
            {
                return;
            }

            ResponsiveCanvasLayout[] profiles =
                stage.prefabContentsRoot.GetComponentsInChildren<ResponsiveCanvasLayout>(true);

            if (profiles.Length > 0)
            {
                Configure(profiles[0], PreviewSource.GameView, false);
            }
        }

        private static void OnEditorUpdate()
        {
            if (_stage == null || _layout == null || _isSaving || _source != PreviewSource.GameView ||
                EditorApplication.isPlayingOrWillChangePlaymode || _stage != PrefabStageUtility.GetCurrentPrefabStage())
            {
                return;
            }

            Configure(_layout, PreviewSource.GameView, _isWithoutControls);
        }

        private static void OnUndoRedo()
        {
            if (_stage != null)
            {
                RefreshSurfaces();
            }
        }

        private static void RefreshSurfaces()
        {
            foreach (PrefabCanvasSurface surface in _surfaces)
            {
                surface.Show(_size);
            }

            Canvas.ForceUpdateCanvases();
            SceneView.RepaintAll();
        }

        private static void FrameSurface()
        {
            SceneView view = SceneView.lastActiveSceneView;

            if (view == null || _surfaces.Count == 0)
            {
                return;
            }

            view.in2DMode = true;
            Vector3 size = new Vector3(_size.x, _size.y, 1f);
            view.Frame(new Bounds(_surfaces[0].Canvas.transform.position, size), false);
        }

        private static void RememberControls(RoulettePopupLayout roulette)
        {
            SerializedObject serialized = new SerializedObject(roulette);
            RememberControl(serialized.FindProperty(SpinButtonProperty));
            RememberControl(serialized.FindProperty(AdButtonProperty));
        }

        private static void RememberControl(SerializedProperty property)
        {
            RectTransform rect = property.objectReferenceValue as RectTransform;

            if (rect == null || _rouletteControls.ContainsKey(rect.gameObject))
            {
                return;
            }

            _rouletteControls.Add(rect.gameObject, rect.gameObject.activeSelf);
        }

        private static void RestoreControls()
        {
            foreach (KeyValuePair<GameObject, bool> entry in _rouletteControls)
            {
                if (entry.Key != null)
                {
                    entry.Key.SetActive(entry.Value);
                }
            }
        }

        private static void ApplyControlsPreview()
        {
            foreach (KeyValuePair<GameObject, bool> entry in _rouletteControls)
            {
                if (entry.Key != null)
                {
                    entry.Key.SetActive(_isWithoutControls == false && entry.Value);
                }
            }
        }

        private static void OnBeforeSave(GameObject root)
        {
            if (_stage == null || _stage.prefabContentsRoot != root)
            {
                return;
            }

            _isSaving = true;
            RestoreControls();
            RestoreSurfaces();
        }

        private static void OnAfterSave(GameObject root)
        {
            if (_stage == null || _stage.prefabContentsRoot != root)
            {
                return;
            }

            _isSaving = false;
            ApplyControlsPreview();
            RefreshSurfaces();
        }

        private static void OnStageClosing(PrefabStage stage)
        {
            if (_stage == stage)
            {
                Clear();
            }
        }

        private static void RestoreSurfaces()
        {
            foreach (PrefabCanvasSurface surface in _surfaces)
            {
                surface.Restore();
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            PrefabStage.prefabStageOpened -= OnStageOpened;
            PrefabStage.prefabSaving -= OnBeforeSave;
            PrefabStage.prefabSaved -= OnAfterSave;
            PrefabStage.prefabStageClosing -= OnStageClosing;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.delayCall -= OnStageReady;
            Undo.undoRedoPerformed -= OnUndoRedo;
            _pendingStage = null;
            Clear();
        }

        private static void Clear()
        {
            RestoreControls();
            _rouletteControls.Clear();
            RestoreSurfaces();
            _surfaces.Clear();
            _stage = null;
            _layout = null;
            _isSaving = false;
        }
    }
}
