using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace LSM
{
    public enum E_SoundType
    {
        Master, BGM, SFX
    }
    public enum E_SoundClip_Type
    { BGM, SFX }

    public class SoundManager : MonoBehaviour
    {
        private const float VOLUME_MIN = -50, VOLUME_MAX = 10;
        private const int MAX_SFX_CHANNELING = 10, MAX_BGM_CHANNELING = 1;

        private const string DEFAULT_AUDIOCLIP = "Default_Blank";

        [SerializeField] private AudioMixer mixer;



        // bgm, sfx를 담당하는 채널들.
        BGM_Channel bgm_channel;
        SFX_Channel sfx_channel;
        Master_Channel master_channel;

        [Header("AudioClips")]
        [SerializeField] private SO_SoundClip default_clip;

        // 오디오 클립을 저장하는 dictionary
        Dictionary<string, SO_SoundClip> dict_bgm, dict_sfx;

        [Header("AudioSource Prefab")]
        [SerializeField] private GameObject bgm_prefab;
        [SerializeField] private GameObject sfx_prefab;

        // 델리게이트
        private static event Action<E_SoundType, string> Audio_Set;
        private static event Action<E_SoundType, bool> Audio_Mute;
        private static event Action<E_SoundType, float> Audio_Volume;

        // 볼륨의 값, 뮤트, 노래제목
        private delegate float get_Audio_volume(E_SoundType _type);
        private static event get_Audio_volume GetAudioVolume;

        private delegate bool get_Audio_mute(E_SoundType _type);
        private static event get_Audio_mute GetAudioMute;

        private delegate string get_Audio_code(E_SoundType _type);
        private static event get_Audio_code GetAudioCode;


        // 음악 불러오기.
        private string soundClip_Path = "Scriptable/LSM/SoundClip/";


        #region ---------- 설정
        private void Awake()
        {
            Init_();
            Init_SoundClip();
        }

        private void Start()
        {
            Reset_();
        }

        private void Init_()
        {
            bgm_channel = new BGM_Channel();
            sfx_channel = new SFX_Channel();
            master_channel = new Master_Channel();
            bgm_channel.Init_(this, MAX_BGM_CHANNELING);
            sfx_channel.Init_(this, MAX_SFX_CHANNELING);
            master_channel.Init_(this);

            // 사운드 채널링을 위한 오브젝트 생성.
            for (int i = 0; i < MAX_BGM_CHANNELING; i++)
            {
                GameObject d_obj = GameObject.Instantiate(bgm_prefab);
                d_obj.transform.SetParent(transform);
                bgm_channel.Add_AudioSource(d_obj.GetComponent<AudioSource>());
            }

            for (int i = 0; i < MAX_SFX_CHANNELING; i++)
            {
                GameObject d_obj = GameObject.Instantiate(sfx_prefab);
                d_obj.transform.SetParent(transform);
                sfx_channel.Add_AudioSource(d_obj.GetComponent<AudioSource>());
            }

            // 델리게이트 적용
            Audio_Set = Setting_Audio;
            Audio_Mute = Mute_Volume;
            Audio_Volume = VolumeSet;

            GetAudioVolume = Get_AudioVolume_Method;
            GetAudioMute = Get_AudioMute_Method;
            GetAudioCode = Get_AudioCode_Method;
        }

        [SerializeField]SO_SoundClip[] d_clips;
        private void Init_SoundClip()
        {
            d_clips = Resources.LoadAll<SO_SoundClip>(soundClip_Path);
            dict_bgm = new Dictionary<string, SO_SoundClip>();
            dict_sfx = new Dictionary<string, SO_SoundClip>();

            foreach(var d in d_clips)
            {
                if (d._code.Equals(DEFAULT_AUDIOCLIP))
                { continue; }

                switch (d._clipType)
                {
                    case E_SoundClip_Type.BGM:
                        dict_bgm.Add(d._code, d);
                        break;
                    case E_SoundClip_Type.SFX:
                        dict_sfx.Add(d._code, d);
                        break;
                }
            }
        }

        private void Reset_()
        {
            // ToDo. PlayerPrefs 혹은 그외 기능에서 볼륨을 설정했다면 그에 따라 값을 초기화.
            VolumeSet(E_SoundType.BGM, 0.5f);
            VolumeSet(E_SoundType.SFX, 0.5f);
            VolumeSet(E_SoundType.Master, 0.5f);
        }
        #endregion

        #region ---------- Event용 메소드

        public static void Audio_Event_Set(E_SoundType _type, string _code)
        { Audio_Set?.Invoke(_type, _code); }
        public static void Audio_Event_Mute(E_SoundType _type, bool _mute)
        { Audio_Mute?.Invoke(_type, _mute); }
        public static void Audio_Event_Volume(E_SoundType _type, float _volume)
        { Audio_Volume?.Invoke(_type, _volume); }
        public static string Get_AudioCode(E_SoundType _type)
        { return GetAudioCode(_type); }
        public static float Get_AudioVolume(E_SoundType _type)
        { return GetAudioVolume(_type); }
        public static bool Get_AudioMute(E_SoundType _type)
        { return GetAudioMute(_type); }

        #endregion

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
                    if (!dict_bgm.ContainsKey(_code))
                    { Debug.LogError($"SoundManager Error -> {_code} can Not Found!"); return; }
                    d_source = dict_bgm[_code];
                    d_audioSource = bgm_channel;
                    break;
                case E_SoundType.SFX:
                    if (!dict_sfx.ContainsKey(_code))
                    { Debug.LogError($"SoundManager Error -> {_code} can Not Found!"); return; }
                    d_source = dict_sfx[_code];
                    d_audioSource = sfx_channel;
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
            float volume_ = ((VOLUME_MAX - VOLUME_MIN) * _v) + VOLUME_MIN;

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
                    d_audioSource = bgm_channel;
                    break;
                case E_SoundType.SFX:
                    d_audioSource = sfx_channel;
                    break;
            }
            if (d_audioSource == null)
            { return ""; }
            return d_audioSource.CurAudioCode;
        }

        public float Get_AudioVolume_Method(E_SoundType _type)
        { return Get_TypeMixer(_type)._Volume; }

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

            mixer.SetFloat(float_set_code, _v);
        }

        /// <summary>
        /// 타입에 맞는 I_AudioMixer 반환.
        /// </summary>
        private I_AudoMixer Get_TypeMixer(E_SoundType _type)
        {
            I_AudoMixer d_mixer = null;
            switch (_type)
            {
                case E_SoundType.BGM: d_mixer = bgm_channel; break;
                case E_SoundType.SFX: d_mixer = sfx_channel; break;
                case E_SoundType.Master: d_mixer = master_channel; break;
            }
            return d_mixer;
        }
        #endregion
    }
}