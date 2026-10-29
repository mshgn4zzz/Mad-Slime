using System;

namespace MadSlime.Game.Interfaces
{
    public interface IGameVisibility
    {
        event Action VisibilitySuspended;

        event Action VisibilityRestored;
    }
}
