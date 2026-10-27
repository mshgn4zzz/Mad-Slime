using System;

namespace Game
{
    public interface IGameVisibility
    {
        event Action Hidden;

        event Action Shown;
    }
}
