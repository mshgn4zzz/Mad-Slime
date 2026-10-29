using MadSlime.Game.Interfaces;
using YG;

namespace MadSlime.Game.Adapters
{
    public class Yg2GameplayReporter : IGameplayReporter
    {
        public void ReportStart()
        {
            YG2.GameplayStart();
        }

        public void ReportStop()
        {
            YG2.GameplayStop();
        }
    }
}
