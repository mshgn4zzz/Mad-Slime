using Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
namespace DI
{
    internal static class SceneButtonSoundInjector
    {
        public static void Inject(IObjectResolver container, Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                UIButtonSound[] buttons = roots[i].GetComponentsInChildren<UIButtonSound>(true);

                for (int j = 0; j < buttons.Length; j++)
                {
                    container.Inject(buttons[j]);
                }
            }
        }
    }
}
