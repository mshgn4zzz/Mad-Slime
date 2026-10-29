using MadSlime.CameraSystem.Data;
using UnityEditor;
using UnityEngine;

namespace MadSlime.CameraSystem.Editor
{
    [CustomEditor(typeof(CameraConfig))]
    public class CameraConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            SerializedProperty axes = serializedObject.FindProperty("_shakeAxes");
            Vector2 value = axes.vector2Value;

            axes.vector2Value = new Vector2(Mathf.Max(0f, value.x), Mathf.Max(0f, value.y));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
