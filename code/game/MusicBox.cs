using Sandbox;
using System;

namespace GGame;

public class MusicBox : BaseNetworkable {
    private readonly float lerpSpeed = 0.05f;
    public static MusicBox Current {get; set;}
    public Sound SongLooping;
    public Sound SongActive;

    public MusicBox() {
        if (Current is not null) return;
        Current = this;
    }

    public void SetLooping(string song) {
        SongLooping = Sound.FromScreen(song);
    }

    public async void LerpToActive(string song) {
        float volume = 1;

        for (int i = 0; i < 50; i++) {
            volume = MathX.Lerp(volume, 0, lerpSpeed);
            SongLooping.SetVolume(volume);
            await GameTask.DelayRealtime(10);
        }
        
        SongActive.SetVolume(volume);
        SongActive = Sound.FromScreen(song);

        for (int i = 0; i < 50; i++) {
            volume = MathX.Lerp(volume, 0, lerpSpeed);
            SongLooping.SetVolume(volume);
            await GameTask.DelayRealtime(10);
        }
    }

    public async void LerpToLooping() {
        float volume = 1;

        for (int i = 0; i < 50; i++) {
            volume = MathX.Lerp(volume, 0, lerpSpeed);
            SongActive.SetVolume(volume);
            await GameTask.DelayRealtime(10);
        }

        SongActive.Stop();

        for (int i = 0; i < 50; i++) {
            volume = MathX.Lerp(volume, 1, lerpSpeed);
            SongLooping.SetVolume(volume);
            await GameTask.DelayRealtime(10);
        }
    }
}

