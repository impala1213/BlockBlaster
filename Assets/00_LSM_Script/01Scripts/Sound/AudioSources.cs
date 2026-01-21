using UnityEngine;
using System.Collections.Generic;


public interface I_AudoSource
{
    public SoundManager manager { get;  }
    public List<AudioSource> AudioSources { get; }
    public bool _IsMute { get; }
    public float _Volume { get; }

    public void Init_(SoundManager _manager);
    public void Add_AudioSource(AudioSource _as);
    public void Audio_Mute(bool b);
    public void Audio_VolumeSet(float v);
    public void Audio_Setting(AudioClip _clip);
}

public class BGM_Channel : I_AudoSource
{
    public SoundManager manager { get; private set; }
    public List<AudioSource> AudioSources { get; private set; }
    public bool _IsMute { get; private set; }
    public float _Volume { get; private set; }

    public void Init_(SoundManager _manager)
    {
        AudioSources = new List<AudioSource>();
        manager = _manager;
        _IsMute = false;
        _Volume = 0.5f;
    }

    public void Audio_Mute(bool b)
    {
        _IsMute = b;
        manager.SetMixerVolume(E_SoundType.BGM, _IsMute? -80f :_Volume);
    }

    public void Audio_Setting(AudioClip _clip)
    {
        AudioSources[0].clip = _clip;
    }

    public void Audio_VolumeSet(float v)
    {
        _Volume = v;
        manager.SetMixerVolume(E_SoundType.BGM, _Volume);
    }

    public void Add_AudioSource(AudioSource _as)
    {
        AudioSources.Add(_as);
    }
}

public class SFX_Channel : I_AudoSource
{
    public SoundManager manager { get; private set; }
    public List<AudioSource> AudioSources { get; private set; }
    public bool _IsMute { get; private set; }
    public float _Volume { get; private set; }

    int channeling_idx = 0;

    public void Init_(SoundManager _manager)
    {
        AudioSources = new List<AudioSource>();
        manager = _manager;
        _IsMute = false;
        _Volume = 0.5f;
        channeling_idx = 0;
    }

    public void Audio_Mute(bool b)
    {
        _IsMute = b;
        manager.SetMixerVolume(E_SoundType.SFX, _IsMute ? -80f : _Volume);
    }

    public void Audio_Setting(AudioClip _clip)
    {
        channeling_idx = (channeling_idx+1) % AudioSources.Count;
        AudioSources[channeling_idx].clip = _clip;
    }

    public void Audio_VolumeSet(float v)
    {
        _Volume = v;
        manager.SetMixerVolume(E_SoundType.SFX,_Volume);
    }

    public void Add_AudioSource(AudioSource _as)
    {
        AudioSources.Add(_as);
    }
}


