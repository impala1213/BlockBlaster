using UnityEngine;
using System.Collections.Generic;

namespace LSM
{
    #region ---------- 인터페이스
    public interface I_AudoMixer
    {
        public SoundManager manager { get; }
        public bool _IsMute { get; }
        public float _Volume { get; }

        public void Init_(SoundManager _manager);
        public void Audio_Mute(bool b);
        public void Audio_VolumeSet(float v);

    }

    public interface I_AudioSource
    {
        public AudioSource[] AudioSources { get; }
        public SO_SoundClip[] Cur_SoundClip { get; }
        public string CurAudioCode { get; }

        public void Init_(SoundManager _manager, int _num);
        public void Add_AudioSource(AudioSource _as);
        public void Audio_Setting(SO_SoundClip _clip);
    }

    #endregion


    public class Master_Channel : I_AudoMixer
    {
        public SoundManager manager { get; private set; }
        public bool _IsMute { get; private set; }
        public float _Volume { get; private set; }



        public void Init_(SoundManager _manager)
        {
            manager = _manager;
            _IsMute = false;
            _Volume = 0.5f;
        }

        public void Audio_Mute(bool b)
        {
            _IsMute = b;
            manager.SoundCtrl.SetMixerVolume(E_SoundType.Master, _IsMute ? -80f : _Volume);
        }

        public void Audio_VolumeSet(float v)
        {
            _Volume = v;
            manager.SoundCtrl.SetMixerVolume(E_SoundType.Master, _Volume);
        }

    }

    public class BGM_Channel : I_AudoMixer, I_AudioSource
    {
        public SoundManager manager { get; private set; }
        public AudioSource[] AudioSources { get; private set; }
        public SO_SoundClip[] Cur_SoundClip { get; private set; }
        public bool _IsMute { get; private set; }
        public float _Volume { get; private set; }

        public string CurAudioCode { get { return Cur_SoundClip[0]._code; } }
        public void Init_(SoundManager _manager)
        { 
            manager = _manager;
            _IsMute = false;
            _Volume = 0.5f;
        }

        public void Init_(SoundManager _manager, int _num)
        {
            Init_(_manager);
            AudioSources = new AudioSource[_num];
            Cur_SoundClip = new SO_SoundClip[_num];
        }

        public void Audio_Mute(bool b)
        {
            if (_IsMute && !b)  // 뮤트였다가 뮤트가 풀리면 재생.
            { AudioSources[0].Play(); }
            _IsMute = b;
            manager.SoundCtrl.SetMixerVolume(E_SoundType.BGM, _IsMute ? -80f : _Volume);
            if (_IsMute)
            { AudioSources[0].Stop(); }
        }

        public void Audio_Setting(SO_SoundClip _clip)
        {
            if (Cur_SoundClip[0]!=null &&_clip._code.Equals(Cur_SoundClip[0]._code))
            { return; }
            Cur_SoundClip[0] = _clip;
            AudioSources[0].clip = Cur_SoundClip[0]._clip;
            AudioSources[0].Play();
        }

        public void Audio_VolumeSet(float v)
        {
            _Volume = v;
            manager.SoundCtrl.SetMixerVolume(E_SoundType.BGM, _Volume);
        }

        public void Add_AudioSource(AudioSource _as)
        {
            AudioSources[0]=_as;
        }
    }

    public class SFX_Channel : I_AudoMixer, I_AudioSource
    {
        public SoundManager manager { get; private set; }
        public AudioSource[] AudioSources { get; private set; }
        public SO_SoundClip[] Cur_SoundClip { get; private set; }
        public bool _IsMute { get; private set; }
        public float _Volume { get; private set; }

        public string CurAudioCode { get { return Cur_SoundClip[channeling_idx]._code; } }

        int channeling_idx = 0;

        public void Init_(SoundManager _manager)
        {
            manager = _manager;
            _IsMute = false;
            _Volume = 0.5f;
            channeling_idx = 0;
        }

        public void Init_(SoundManager _manager, int _num)
        {
            Init_(_manager);
            AudioSources = new AudioSource[_num];
            Cur_SoundClip = new SO_SoundClip[_num];
        }


        public void Audio_Mute(bool b)
        {
            _IsMute = b;
            manager.SoundCtrl.SetMixerVolume(E_SoundType.SFX, _IsMute ? -80f : _Volume);
        }

        public void Audio_Setting(SO_SoundClip _clip)
        {
            channeling_idx = (channeling_idx + 1) % AudioSources.Length;

            Cur_SoundClip[channeling_idx] = _clip;
            AudioSources[channeling_idx].clip = Cur_SoundClip[channeling_idx]._clip;
            AudioSources[channeling_idx].Play();
        }

        public void Audio_VolumeSet(float v)
        {
            _Volume = v;
            manager.SoundCtrl.SetMixerVolume(E_SoundType.SFX, _Volume);
        }

        public void Add_AudioSource(AudioSource _as)
        {
            AudioSources[channeling_idx++] = _as;
        }
    }


}