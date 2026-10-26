using System;
using System.Collections.Generic;
using UnityEngine;
namespace Upgrades
{
[Serializable]
public class UpgradeEntry
{
    [Tooltip("Тип ступенчатой прокачки.")]
    [SerializeField] private UpgradeType _type;

    [Tooltip("Иконка прокачки на плашке.")]
    [SerializeField] private Sprite _icon;

    [Tooltip("Цена первой ступени.")]
    [SerializeField, Min(0)] private int _baseCost = 200;

    [Tooltip("Множитель цены каждой следующей ступени. 2 = удвоение.")]
    [SerializeField, Min(1)] private int _costMultiplier = 2;

    [Tooltip("Прирост эффекта за КАЖДУЮ ступень отдельно (доля от 1), по порядку. " +
        "Число значений обязано совпадать с MaxSteps.")]
    [SerializeField, Min(0f)] private List<float> _stepValues = new List<float>();

    [Tooltip("Максимальное число ступеней.")]
    [SerializeField, Min(1)] private int _maxSteps = 5;

    public UpgradeType Type => _type;
    public Sprite Icon => _icon;
    public int BaseCost => _baseCost;
    public int CostMultiplier => _costMultiplier;
    public IReadOnlyList<float> StepValues => _stepValues;
    public int MaxSteps => _maxSteps;

}
}
