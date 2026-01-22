using LSM;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Sound_Controller 
{
    SoundManager soundManager;

    public void Init_(SoundManager _manager)
    {
        soundManager = _manager;
    }


    #region ---------- 사용 메소드
    /// <summary>
    /// 이름 혹은 코드로 지정할 예정
    /// </summary>
    public void Setting_Audio(E_SoundType _type, string _code)
    {
        if (_type == E_SoundType.Master)
        {
            Debug.LogError("SoundManager Error -> AudioSet - Dont Use \'Master\' Type");
            return;
        }

        SO_SoundClip d_source = null;
        I_AudioSource d_audioSource = null;
        switch (_type)
        {
            case E_SoundType.BGM:
                if (!soundManager.Dict_BGM.ContainsKey(_code))
                { Debug.LogError($"SoundManager Error -> {_code} can Not Found!"); return; }
                d_source = soundManager.Dict_BGM[_code];
                d_audioSource = soundManager.BGM_Channel;
                break;
            case E_SoundType.SFX:
                if (!soundManager.Dict_SFX.ContainsKey(_code))
                { Debug.LogError($"SoundManager Error -> {_code} can Not Found!"); return; }
                d_source = soundManager.Dict_SFX[_code];
                d_audioSource = soundManager.SFX_Channel;
                break;
        }

        if (d_source == null)
        { return; }
        d_audioSource?.Audio_Setting(d_source);
    }

    /// <summary>
    /// BGM, SFX를 음소거
    /// </summary>
    public void Mute_Volume(E_SoundType _type, bool b)
    {
        I_AudoMixer d_mixer = Get_TypeMixer(_type);
        d_mixer?.Audio_Mute(b);
    }

    /// <summary>
    /// BGM, SFX의 볼륨 값을 지정. 0~1 사이 값.
    /// </summary>
    public void VolumeSet(E_SoundType _type, float _v)
    {
        _v = Mathf.Clamp(_v, 0, 1f);
        float volume_ = Set_Volume_Mod(_v, 1);
        //float volume_ = ((VOLUME_MAX - VOLUME_MIN) * _v) + VOLUME_MIN;

        I_AudoMixer d_mixer = Get_TypeMixer(_type);
        d_mixer.Audio_VolumeSet(volume_);
    }

    public string Get_AudioCode_Method(E_SoundType _type)
    {
        if (_type == E_SoundType.Master)
        {
            Debug.LogError("SoundManager Error -> GetAudioCode - Dont Use \'Master\' Type");
            return "";
        }

        I_AudioSource d_audioSource = null;
        switch (_type)
        {
            case E_SoundType.BGM:
                d_audioSource = soundManager.BGM_Channel;
                break;
            case E_SoundType.SFX:
                d_audioSource = soundManager.SFX_Channel;
                break;
        }
        if (d_audioSource == null)
        { return ""; }
        return d_audioSource.CurAudioCode;
    }

    public float Get_AudioVolume_Method(E_SoundType _type)
    {
        return Set_Volume_Mod(Get_TypeMixer(_type)._Volume, 0);
    }

    public bool Get_AudioMute_Method(E_SoundType _type)
    { return Get_TypeMixer(_type)._IsMute; }
    #endregion

    #region ---------- 유틸

    /// <summary>
    /// 오디오 믹서의 볼륨 조절.
    /// </summary>
    public void SetMixerVolume(E_SoundType _type, float _v)
    {
        string float_set_code = "";
        switch (_type)
        {
            case E_SoundType.Master:
                float_set_code = "Master_V";
                break;
            case E_SoundType.BGM:
                float_set_code = "BGM_V";
                break;
            case E_SoundType.SFX:
                float_set_code = "SFX_V";
                break;
        }

        soundManager.Mixer.SetFloat(float_set_code, _v);
    }

    /// <summary>
    /// 타입에 맞는 I_AudioMixer 반환.
    /// </summary>
    private I_AudoMixer Get_TypeMixer(E_SoundType _type)
    {
        I_AudoMixer d_mixer = null;
        switch (_type)
        {
            case E_SoundType.BGM: d_mixer = soundManager.BGM_Channel; break;
            case E_SoundType.SFX: d_mixer = soundManager.SFX_Channel; break;
            case E_SoundType.Master: d_mixer = soundManager.Master_Channel; break;
        }
        return d_mixer;
    }

    /// <summary>
    /// 볼륨을 변경하는 메소드 0f~1f <-> -80f~20f
    /// </summary>
    /// <param name="_v">Volume</param>
    /// <param name="mod"> 0 = 0f~1f / 1 = -80f~20f </param>
    /// <returns></returns>
    private float Set_Volume_Mod(float _v, int mod)
    {
        if (mod == 0)
        { return (_v - SoundManager.VOLUME_MIN) / (SoundManager.VOLUME_MAX - SoundManager.VOLUME_MIN); }
        else if (mod == 1)
        { return ((SoundManager.VOLUME_MAX - SoundManager.VOLUME_MIN) * _v) + SoundManager.VOLUME_MIN; }
        return _v;
    }
    #endregion
}
