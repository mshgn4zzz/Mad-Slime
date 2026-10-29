using System;
using MadSlime.Items.Data;
using MadSlime.Items.View;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MadSlime.Items.Logic
{
    public class ItemFactory
    {
        public Item Create(Item prefab, ItemDefinition definition, Vector3 position, float scale, Transform parent)
        {
            Item item = Object.Instantiate(prefab, position, Quaternion.identity, parent);

            item.Initialize(definition);

            if (item.TryGetComponent(out ItemBody body) == false)
            {
                throw new InvalidOperationException($"{prefab.name}: ItemBody is missing.");
            }

            body.Initialize(position, scale);

            return item;
        }
    }
}
