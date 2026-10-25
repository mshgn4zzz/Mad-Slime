using System;
using Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UI
{
    public class LeaderboardRowViewFactory : MonoBehaviour
    {
        [SerializeField] private LeaderboardRowView _prefab;

        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public LeaderboardRowView Create(Transform parent, LeaderboardEntryData entry, bool isOwn)
        {
            if (_resolver == null || _prefab == null || parent == null)
            {
                throw new InvalidOperationException($"{name}: leaderboard row factory is not configured.");
            }

            LeaderboardRowView row = _resolver.Instantiate(_prefab, parent);
            row.Initialize(entry, isOwn);
            return row;
        }
    }
}
