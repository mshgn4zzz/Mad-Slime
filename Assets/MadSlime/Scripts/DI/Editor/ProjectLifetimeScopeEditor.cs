using MadSlime.Items.Editor;
using MadSlime.Saves.Editor;
using MadSlime.Skins.Editor;
using UnityEditor;
using UnityEngine;

namespace MadSlime.DI.Editor
{
    [CustomEditor(typeof(ProjectLifetimeScope))]
    public class ProjectLifetimeScopeEditor : UnityEditor.Editor
    {
        private PlayerDataResetTool _reset = new PlayerDataResetTool();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("Open Prop Factory"))
            {
                EditorWindow.GetWindow<ItemPropFactory>("Prop Factory");
            }

            if (GUILayout.Button("Open Skin Factory"))
            {
                EditorWindow.GetWindow<SkinItemFactory>("Skin Factory");
            }

            if (GUILayout.Button("Reset Player Data"))
            {
                _reset.ResetPlayerData();
            }

            if (GUILayout.Button("Reset Player Data With Money"))
            {
                _reset.ResetPlayerDataWithMoney();
            }

            if (GUILayout.Button("Reset Player Data With Money And Skins"))
            {
                _reset.ResetPlayerDataWithMoneyAndSkins();
            }

            if (GUILayout.Button("Unlock All Skins"))
            {
                _reset.UnlockAllSkins();
            }
        }

        private void OnEnable()
        {
            _reset.Subscribe();
        }

        private void OnDisable()
        {
            _reset.Unsubscribe();
        }
    }
}
