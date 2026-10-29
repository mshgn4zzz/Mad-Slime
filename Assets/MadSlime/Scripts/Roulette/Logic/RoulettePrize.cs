using MadSlime.Skins.Data;

namespace MadSlime.Roulette.Logic
{
    public class RoulettePrize
    {
        public RoulettePrize(SkinItem skin, int coins)
        {
            Skin = skin;
            Coins = coins;
        }

        public SkinItem Skin { get; }

        public int Coins { get; }
    }
}
