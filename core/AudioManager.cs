using Godot;
using System;
using ClasicosMenteActiva.core;

/// <summary>
/// Gestor de audio (plan Fase 3 §3.3 · spec §8.3).
/// Controla buses BGM y SFX, volumen y silencio total. No usa sonidos bruscos.
/// </summary>
public partial class AudioManager : Node
{
    private AudioStreamPlayer _bgmPlayer;
    private bool _muted = false;
    private float _bgmVolume = Constants.DefaultBgmVolume;
    private float _sfxVolume = Constants.DefaultSfxVolume;

    public bool IsMuted => _muted;
    public float BgmVolume => _bgmVolume;
    public float SfxVolume => _sfxVolume;

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
        _bgmVolume = Mathf.Clamp(volume, 0f, 1f);
        ApplyVolume();
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp(volume, 0f, 1f);
    }

    public void SetMuted(bool muted)
    {
        _muted = muted;
        ApplyVolume();
    }

    public void PlayBgm(AudioStream stream, bool loop = true)
    {
        if (_muted || stream == null)
        {
            return;
        }

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
        if (_muted || stream == null)
        {
            return;
        }

        var player = new AudioStreamPlayer
        {
            Stream = stream,
            VolumeDb = DbLinearToDb(_sfxVolume),
        };
        AddChild(player);
        player.Finished += () => player.QueueFree();
        player.Play();
    }

    private void ApplyVolume()
    {
        if (_bgmPlayer != null)
        {
            _bgmPlayer.VolumeDb = _muted ? -80.0f : DbLinearToDb(_bgmVolume);
        }
    }

    private static float DbLinearToDb(float linear)
    {
        if (linear <= 0f)
        {
            return -80.0f;
        }
        return (float)(System.Math.Log10(linear) * 20.0);
    }
}
