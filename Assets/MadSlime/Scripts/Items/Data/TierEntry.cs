using System;
using UnityEngine;
namespace Items
{
[Serializable]
public class TierEntry
{
    [Tooltip("Тир, к которому относится строка.")]
    [SerializeField] private SizeTier _tier;

    [Tooltip("Масштаб предмета этого тира при спавне.")]
    [SerializeField] private float _scale = 1f;

    [Tooltip("Масса предмета тира: сколько массы получит игрок при поглощении.")]
    [SerializeField] private int _mass = 1;

    public SizeTier Tier => _tier;
    public float Scale => _scale;
    public int Mass => _mass;
}
}
