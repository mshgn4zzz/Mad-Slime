using System;

namespace MadSlime.Saves.Interfaces
{
    public interface ISavesReadiness
    {
        event Action Prepared;

        bool IsReady { get; }
    }
}
