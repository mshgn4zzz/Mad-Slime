using Skins;
namespace Roulette
{
    public class RoulettePrize
    {
        public SkinItem Skin { get; }
        public int Coins { get; }

        public RoulettePrize(SkinItem skin, int coins)
        {
            Skin = skin;
            Coins = coins;
        }
    }
}
