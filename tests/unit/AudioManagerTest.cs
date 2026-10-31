using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Fase 3 §3.3 — AudioManager (CORE-AUDIO-01 a CORE-AUDIO-03).
/// Volumen por bus, silencio total.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class AudioManagerTest
{
    private AudioManager _audio;

    [BeforeTest]
    public void BeforeTest()
    {
        _audio = new AudioManager();
    }

    [TestCase]
    public void VolumenBgmYSfxIndependientes() // CORE-AUDIO-01
    {
        _audio.SetBgmVolume(0.3f);
        _audio.SetSfxVolume(0.8f);

        AssertFloat(_audio.BgmVolume).IsEqual(0.3f);
        AssertFloat(_audio.SfxVolume).IsEqual(0.8f);

        _audio.SetBgmVolume(0.9f);
        AssertFloat(_audio.BgmVolume).IsEqual(0.9f);
        AssertFloat(_audio.SfxVolume).IsEqual(0.8f); // SFX no cambió
    }

    [TestCase]
    public void SilencioTotalApagaAudio() // CORE-AUDIO-02
    {
        _audio.SetMuted(true);
        AssertBool(_audio.IsMuted).IsTrue();

        _audio.SetMuted(false);
        AssertBool(_audio.IsMuted).IsFalse();
    }

    [TestCase]
    public void VolumenSeClampaEnRangoValido()
    {
        _audio.SetBgmVolume(-0.5f);
        AssertFloat(_audio.BgmVolume).IsEqual(0.0f);

        _audio.SetBgmVolume(2.0f);
        AssertFloat(_audio.BgmVolume).IsEqual(1.0f);
    }
}
