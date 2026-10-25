namespace Saves
{
    public interface IAudioStorage
    {
        int MusicVolume { get; }

        int SfxVolume { get; }

        void SetVolumes(int musicVolume, int sfxVolume);
    }
}
