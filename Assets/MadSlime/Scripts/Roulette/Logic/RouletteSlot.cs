using MadSlime.Skins.Data;

namespace MadSlime.Roulette.Logic
{
    public class RouletteSlot
    {
        public RouletteSlot(SkinItem skin, int coins, float weight, bool isHidden)
        {
            Skin = skin;
            Coins = coins;
            Weight = weight;
            IsHidden = isHidden;
        }

        public SkinItem Skin { get; }

        public int Coins { get; }

        public float Weight { get; }

        public bool IsHidden { get; }
    }
}
