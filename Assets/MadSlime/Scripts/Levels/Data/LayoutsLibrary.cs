using System.Collections.Generic;
using UnityEngine;
namespace Levels
{
    [CreateAssetMenu(menuName = "Mad Slime/Layouts Library", fileName = "NewLayoutsLibrary")]
    public class LayoutsLibrary : ScriptableObject
    {
        [SerializeField] private List<LayoutSet> _layouts = new List<LayoutSet>();

        public IReadOnlyList<LayoutSet> Layouts => _layouts;
    }
}
