using System;
using UnityEngine;

namespace Upgrades
{
[Serializable]
public class PerkEntry
{
    [SerializeField] private PerkType _type;

    [SerializeField] private Sprite _icon;

    [SerializeField, Min(0)] private int _cost = 3000;

    [SerializeField, Min(0f)] private float _boost;

    public PerkType Type => _type;

    public Sprite Icon => _icon;

    public int Cost => _cost;

    public float Value => _boost;
}
}
