using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum E_SoundType
{
    BGM, SFX
}

public class SoundManager : MonoBehaviour
{
    private const float VOLUME_MIN = -50, VOLUME_MAX = 10;
    private const int MAX_SFX_CHANNELING = 10, MAX_BGM_CHANNELING = 1;

    [SerializeField] private AudioMixer mixer;


    // bgm, sfx를 담당하는 채널들.
    BGM_Channel bgm_channel; 
    SFX_Channel sfx_channel;

    [Header("AudioClips")]
    [SerializeField] private List<AudioClip> bgmList;
    [SerializeField] private List<AudioClip> sfxList;

    // 오디오 클립을 저장하는 dictionary
    Dictionary<string, AudioClip> dict_bgm, dict_sfx;

    [Header("AudioSource Prefab")]
    [SerializeField] private GameObject bgm_prefab;
    [SerializeField] private GameObject sfx_prefab;

    private void Awake()
    {
        Init_();

    }

    private void Start()
    {
        Reset_();
    }

    private void Init_()
    {
        bgm_channel = new BGM_Channel();
        sfx_channel = new SFX_Channel();
        bgm_channel.Init_(this);
        sfx_channel.Init_(this);

        for (int i = 0; i < MAX_BGM_CHANNELING; i++) 
        {
            GameObject d_obj = GameObject.Instantiate(bgm_prefab);
            d_obj.transform.SetParent(transform);
            bgm_channel.Add_AudioSource(d_obj.GetComponent<AudioSource>());
        }

        for(int i = 0; i < MAX_SFX_CHANNELING; i++)
        {
            GameObject d_obj = GameObject.Instantiate(sfx_prefab);
            d_obj.transform.SetParent(transform);
            sfx_channel.Add_AudioSource(d_obj.GetComponent<AudioSource>());
        }

        
    }

    private void Reset_()
    {
        VolumeSet(E_SoundType.BGM, 0.5f);
        VolumeSet(E_SoundType.SFX, 0.5f);
    }

    /// <summary>
    /// 이름 혹은 코드로 지정할 예정
    /// </summary>
    public void Setting_BGM(string _name)
    {
        if (!dict_bgm.ContainsKey(_name)) 
        { Debug.LogError($"SoundManager Error -> {_name} can Not Found!"); }
        bgm_channel.Audio_Setting(dict_bgm[_name]);
    }

    /// <summary>
    /// BGM, SFX를 음소거
    /// </summary>
    public void Mute_Volume(E_SoundType _type, bool b)
    {
        switch (_type)
        {
            case E_SoundType.BGM: bgm_channel.Audio_Mute(b); break;
            case E_SoundType.SFX: sfx_channel.Audio_Mute(b); break;
        }
    }

    /// <summary>
    /// BGM, SFX의 볼륨 값을 지정. 0~1 사이 값.
    /// </summary>
    public void VolumeSet(E_SoundType _type, float _v)
    {
        _v = Mathf.Clamp(_v, 0, 1f);
        float volume_ = ((VOLUME_MAX - VOLUME_MIN) * _v) + VOLUME_MIN;
        Debug.Log($"{_type} {volume_}");
        switch (_type)
        {
            case E_SoundType.BGM: bgm_channel.Audio_VolumeSet(volume_); break;
            case E_SoundType.SFX: sfx_channel.Audio_VolumeSet(volume_); break;
        }
    }

    public void SetMixerVolume(E_SoundType _type, float _v)
    {
        switch (_type)
        {
            case E_SoundType.BGM:
                mixer.SetFloat("BGM_V",_v);
                mixer.GetFloat("BGM_V", out float d);
                Debug.Log($"{d}");
                break;
            case E_SoundType.SFX:
                mixer.SetFloat("SFX_V", _v);
                break;
        }
    }
}
