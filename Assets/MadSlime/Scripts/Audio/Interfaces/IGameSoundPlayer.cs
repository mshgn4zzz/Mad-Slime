using MadSlime.Audio.Data;

namespace MadSlime.Audio.Interfaces
{
    public interface IGameSoundPlayer
    {
        void Play(SfxClip clip);

        void Play(SfxClip clip, float throttleSeconds);

        void Play(SfxClip clip, float pitch, float throttleSeconds);

        void StartLoop(SfxClip clip);

        void StopLoop();
    }
}
