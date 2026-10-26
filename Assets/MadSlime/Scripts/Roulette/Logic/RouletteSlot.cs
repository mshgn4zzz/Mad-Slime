using Skins;
namespace Roulette
{
    public class RouletteSlot
    {
        public SkinItem Skin { get; }
        public int Coins { get; }
        public float Weight { get; }
        public bool IsHidden { get; }

        public RouletteSlot(SkinItem skin, int coins, float weight, bool isHidden)
        {
            Skin = skin;
            Coins = coins;
            Weight = weight;
            IsHidden = isHidden;
        }
    }
}
