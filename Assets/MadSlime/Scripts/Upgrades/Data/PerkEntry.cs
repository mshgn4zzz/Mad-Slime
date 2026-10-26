using System;
using UnityEngine;
namespace Upgrades
{
[Serializable]
public class PerkEntry
{
    [Tooltip("Тип одноразовой покупки.")]
    [SerializeField] private PerkType _type;

    [Tooltip("Иконка перка на плашке.")]
    [SerializeField] private Sprite _icon;

    [Tooltip("Цена. Одноразовые покупки стоят очень дорого.")]
    [SerializeField, Min(0)] private int _cost = 3000;

    [Tooltip("Сколько даёт перк. Амбиции — прибавка к тиру притягиваемых предметов. " +
        "Адреналин — множитель скорости. Нюх — значение не использует.")]
    [SerializeField, Min(0f)] private float _value;

    public PerkType Type => _type;
    public Sprite Icon => _icon;
    public int Cost => _cost;
    public float Value => _value;
}
}
