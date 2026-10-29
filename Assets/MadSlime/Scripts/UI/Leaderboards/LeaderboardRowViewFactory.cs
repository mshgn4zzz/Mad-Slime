using MadSlime.Leaderboards.Data;
using MadSlime.Validation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MadSlime.UI.Leaderboards
{
    public class LeaderboardRowViewFactory
    {
        private readonly LeaderboardRowView _prefab;

        private readonly IObjectResolver _resolver;

        public LeaderboardRowViewFactory(IObjectResolver resolver, LeaderboardRowView prefab)
        {
            ArgumentGuard.CheckNotNull(resolver, nameof(resolver));
            ArgumentGuard.CheckNotNull(prefab, nameof(prefab));

            _resolver = resolver;
            _prefab = prefab;
        }

        public LeaderboardRowView Create(Transform parent, LeaderboardEntryData entry, bool isOwn)
        {
            ArgumentGuard.CheckAssigned(_resolver, nameof(LeaderboardRowViewFactory), nameof(_resolver));
            ArgumentGuard.CheckAssigned(_prefab, nameof(LeaderboardRowViewFactory), nameof(_prefab));
            ArgumentGuard.CheckAssigned(parent, nameof(LeaderboardRowViewFactory), nameof(parent));

            LeaderboardRowView row = _resolver.Instantiate(_prefab, parent);

            row.Initialize(entry, isOwn);
            return row;
        }
    }
}
