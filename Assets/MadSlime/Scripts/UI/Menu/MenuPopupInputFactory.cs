using UnityEngine;

namespace MadSlime.UI.Menu
{
    internal class MenuPopupInputFactory
    {
        public MenuPopupInputState Create(GameObject root)
        {
            if (root.TryGetComponent(out CanvasGroup group) == false)
            {
                group = root.AddComponent<CanvasGroup>();
            }

            return new MenuPopupInputState(group);
        }
    }
}
