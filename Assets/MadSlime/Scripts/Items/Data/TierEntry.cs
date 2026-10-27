using System;
using UnityEngine;

namespace Items
{
[Serializable]
public class TierEntry
{
    [SerializeField] private SizeTier _tier;

    [SerializeField] private float _scale = 1f;

    [SerializeField] private int _mass = 1;

    public SizeTier Tier => _tier;

    public float Scale => _scale;

    public int Mass => _mass;
}
}
