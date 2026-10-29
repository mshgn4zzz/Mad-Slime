using MadSlime.Items.View;
using UnityEngine;

namespace MadSlime.Quota.Logic
{
    internal struct QuotaTrackedItem
    {
        private GhostAnimator _animator;
        private Collider _collider;
        private QuotaTypeState _type;

        public QuotaTrackedItem(GhostAnimator animator, Collider collider, QuotaTypeState type)
        {
            _animator = animator;
            _collider = collider;
            _type = type;
        }

        public GhostAnimator Animator => _animator;

        public Collider Collider => _collider;

        public QuotaTypeState Type => _type;
    }
}
