using UnityEditor;
using UnityEngine;

namespace Detection
{
    [CustomEditor(typeof(GenericOverlapDetector<>), true)]
    public class GenericOverlapDetectorEditor : Editor
    {
        private void OnSceneGUI()
        {
            SerializedProperty radius = serializedObject.FindProperty("_radius");

            if (radius == null)
            {
                return;
            }

            SerializedProperty color = serializedObject.FindProperty("_gizmoColor");

            if (color != null)
            {
                Handles.color = color.colorValue;
            }

            Transform transform = ((Component)target).transform;
            float newRadius = Handles.RadiusHandle(Quaternion.identity, transform.position, radius.floatValue);

            if (Mathf.Approximately(newRadius, radius.floatValue) == false)
            {
                radius.floatValue = newRadius;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
