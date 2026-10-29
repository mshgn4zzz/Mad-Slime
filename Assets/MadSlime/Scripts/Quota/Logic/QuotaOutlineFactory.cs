using MadSlime.Items.View;
using UnityEngine;

namespace MadSlime.Quota.Logic
{
    public class QuotaOutlineFactory
    {
        public Outline GetOrCreate(Item item, Color color, float width)
        {
            if (item.TryGetComponent(out Outline outline) == false)
            {
                outline = item.gameObject.AddComponent<Outline>();
            }

            outline.Prepare(color, width);
            return outline;
        }
    }
}
