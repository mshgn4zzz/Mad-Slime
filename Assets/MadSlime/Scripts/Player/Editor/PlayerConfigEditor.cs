using MadSlime.Player.Data;
using MadSlime.Validation;
using UnityEditor;

namespace MadSlime.Player.Editor
{
    [CustomEditor(typeof(PlayerConfig))]
    public class PlayerConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            PlayerConfig config = (PlayerConfig)target;

            ArgumentGuard.CheckNotNull(config, nameof(config));

            foreach (PlayerTierThreshold threshold in config.Thresholds)
            {
                ArgumentGuard.CheckAssigned(threshold, config.name, nameof(config.Thresholds));
                ArgumentGuard.CheckPositive(threshold.ScaleMultiplier, config.name, nameof(threshold.ScaleMultiplier));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
