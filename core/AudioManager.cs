using System;
using ClasicosMenteActiva.core;
using Godot;

/// <summary>
///     Gestor de audio (plan Fase 3 §3.3 · spec §8.3).
///     Controla buses BGM y SFX, volumen y silencio total. No usa sonidos bruscos.
/// </summary>
public partial class AudioManager : Node
{
    private AudioStreamPlayer _bgmPlayer;

    public bool IsMuted { get; private set; }

    public float BgmVolume { get; private set; } = Constants.DefaultBgmVolume;

    public float SfxVolume { get; private set; } = Constants.DefaultSfxVolume;

    public override void _Ready()
    {
        _bgmPlayer = GetNodeOrNull<AudioStreamPlayer>("BGMPlayer");
        if (_bgmPlayer == null)
        {
            _bgmPlayer = new AudioStreamPlayer { Name = "BGMPlayer" };
            AddChild(_bgmPlayer);
        }

        ApplyVolume();
    }

    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp(volume, 0f, 1f);
        ApplyVolume();
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp(volume, 0f, 1f);
    }

    public void SetMuted(bool muted)
    {
        IsMuted = muted;
        ApplyVolume();
    }

    public void PlayBgm(AudioStream stream, bool loop = true)
    {
        if (IsMuted || stream == null) return;

        _bgmPlayer.Stream = stream;
        _bgmPlayer.Set("loop", loop);
        _bgmPlayer.Play();
    }

    public void StopBgm()
    {
        _bgmPlayer.Stop();
    }

    public void PlaySfx(AudioStream stream)
    {
        if (IsMuted || stream == null) return;

        var player = new AudioStreamPlayer
        {
            Stream = stream,
            VolumeDb = DbLinearToDb(SfxVolume)
        };
        AddChild(player);
        player.Finished += () => player.QueueFree();
        player.Play();
    }

    private void ApplyVolume()
    {
        if (_bgmPlayer != null) _bgmPlayer.VolumeDb = IsMuted ? -80.0f : DbLinearToDb(BgmVolume);
    }

    private static float DbLinearToDb(float linear)
    {
        if (linear <= 0f) return -80.0f;
        return (float)(Math.Log10(linear) * 20.0);
    }
}