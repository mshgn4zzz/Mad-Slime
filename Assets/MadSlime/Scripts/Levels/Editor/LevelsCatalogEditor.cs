using MadSlime.Levels.Data;
using MadSlime.Validation;
using UnityEditor;

namespace MadSlime.Levels.Editor
{
    [CustomEditor(typeof(LevelsCatalog))]
    public class LevelsCatalogEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            LevelsCatalog catalog = (LevelsCatalog)target;

            ArgumentGuard.CheckNotNull(catalog, nameof(catalog));

            foreach (LevelRange range in catalog.Ranges)
            {
                ArgumentGuard.CheckAssigned(range, catalog.name, nameof(catalog.Ranges));
                ArgumentGuard.CheckPositive(range.FromLevel, catalog.name, nameof(range.FromLevel));
                ArgumentGuard.CheckOrder(range.FromLevel, range.ToLevel, catalog.name, nameof(catalog.Ranges));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
