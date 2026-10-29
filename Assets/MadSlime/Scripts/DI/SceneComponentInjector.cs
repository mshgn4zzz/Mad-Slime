using MadSlime.Audio.View;
using MadSlime.UI.HUD;
using MadSlime.UI.Localization;
using MadSlime.UI.Shop;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace MadSlime.DI
{
    internal static class SceneComponentInjector
    {
        public static void Inject(IObjectResolver container, Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                InjectComponents<UIButtonSound>(container, root);
                InjectComponents<LocalizedText>(container, root);
                InjectComponents<LanguageSwitcher>(container, root);
                InjectComponents<LevelLabelUI>(container, root);
                InjectComponents<SkinRarityGroupView>(container, root);
            }
        }

        private static void InjectComponents<T>(IObjectResolver container, GameObject root) where T : Component
        {
            foreach (T component in root.GetComponentsInChildren<T>(true))
            {
                container.Inject(component);
            }
        }
    }
}
