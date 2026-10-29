using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadSlime.Upgrades.Data
{
    [Serializable]
    public class UpgradeEntry
    {
        [SerializeField] private UpgradeType _type;

        [SerializeField] private Sprite _icon;

        [SerializeField, Min(0)] private int _baseCost = 200;

        [SerializeField, Min(1)] private int _costTimes = 2;

        [SerializeField, Min(0f)] private List<float> _stepValues = new List<float>();

        [SerializeField, Min(1)] private int _maxSteps = 5;

        public UpgradeType Type => _type;

        public Sprite Icon => _icon;

        public int BaseCost => _baseCost;

        public int CostMultiplier => _costTimes;

        public IReadOnlyList<float> StepValues => _stepValues;

        public int MaxSteps => _maxSteps;
    }
}
