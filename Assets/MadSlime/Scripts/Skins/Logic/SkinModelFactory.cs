using UnityEngine;

namespace MadSlime.Skins.Logic
{
    public class SkinModelFactory
    {
        public GameObject Create(GameObject prefab, Transform parent)
        {
            return Object.Instantiate(prefab, parent);
        }
    }
}
