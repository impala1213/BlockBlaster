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
        public const float VOLUME_MIN = -50, VOLUME_MAX = 10;
        public const int MAX_SFX_CHANNELING = 10, MAX_BGM_CHANNELING = 1;

        private const string DEFAULT_AUDIOCLIP = "Default_Blank";

        [SerializeField] private AudioMixer mixer;



        // bgm, sfx를 담당하는 채널들.
        private BGM_Channel bgm_channel;
        private SFX_Channel sfx_channel;
        private Master_Channel master_channel;

        [Header("AudioClips")]
        [SerializeField] private SO_SoundClip default_clip;

        // 오디오 클립을 저장하는 dictionary
        Dictionary<string, SO_SoundClip> dict_bgm, dict_sfx;

        [Header("AudioSource Prefab")]
        [SerializeField] private GameObject bgm_prefab;
        [SerializeField] private GameObject sfx_prefab;

        // 사운드 조작하는 클래스
        Sound_Controller soundCtrl;

        // 델리게이트
        private static event Action<E_SoundType, string> A_Audio_Set;
        private static event Action<E_SoundType, bool> A_Audio_Mute;
        private static event Action<E_SoundType, float> A_Audio_Volume;

        // 볼륨의 값, 뮤트, 노래제목
        private delegate float get_Audio_volume(E_SoundType _type);
        private static event get_Audio_volume D_GetAudioVolume;

        private delegate bool get_Audio_mute(E_SoundType _type);
        private static event get_Audio_mute D_GetAudioMute;

        private delegate string get_Audio_code(E_SoundType _type);
        private static event get_Audio_code D_GetAudioCode;


        // 음악 불러오기.
        private string soundClip_Path = "Scriptable/LSM/SoundClip/";


        #region
        public AudioMixer Mixer => mixer;
        public BGM_Channel BGM_Channel => bgm_channel;
        public SFX_Channel SFX_Channel => sfx_channel;
        public Master_Channel Master_Channel => master_channel;
        public Dictionary<string, SO_SoundClip> Dict_BGM => dict_bgm;
        public Dictionary<string, SO_SoundClip> Dict_SFX => dict_sfx;
        public Sound_Controller SoundCtrl => soundCtrl;
        #endregion

        #region ---------- 설정
        private void Awake()
        {
            Init_();
            Init_Delegate();
            Init_SoundClip();
            DontDestroyOnLoad(this.gameObject);
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

            soundCtrl = new Sound_Controller();
            soundCtrl.Init_(this);

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

        }

        private void Init_Delegate()
        {
            // 델리게이트 적용
            A_Audio_Set = soundCtrl.Setting_Audio;
            A_Audio_Mute = soundCtrl.Mute_Volume;
            A_Audio_Volume = soundCtrl.VolumeSet;

            D_GetAudioVolume = soundCtrl.Get_AudioVolume_Method;
            D_GetAudioMute = soundCtrl.Get_AudioMute_Method;
            D_GetAudioCode = soundCtrl.Get_AudioCode_Method;
        }

        //[SerializeField]SO_SoundClip[] d_clips;
        private void Init_SoundClip()
        {
            SO_SoundClip[] d_clips = Resources.LoadAll<SO_SoundClip>(soundClip_Path);
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
            soundCtrl.VolumeSet(E_SoundType.BGM, 0.5f);
            soundCtrl.VolumeSet(E_SoundType.SFX, 0.5f);
            soundCtrl.VolumeSet(E_SoundType.Master, 0.5f);
        }
        #endregion

        #region ---------- Event용 메소드

        public static void Audio_Event_Set(E_SoundType _type, string _code)
        { A_Audio_Set?.Invoke(_type, _code); }
        public static void Audio_Event_Mute(E_SoundType _type, bool _mute)
        { A_Audio_Mute?.Invoke(_type, _mute); }
        public static void Audio_Event_Volume(E_SoundType _type, float _volume)
        { A_Audio_Volume?.Invoke(_type, _volume); }
        public static string Get_AudioCode(E_SoundType _type)
        { return D_GetAudioCode(_type); }
        public static float Get_AudioVolume(E_SoundType _type)
        { return D_GetAudioVolume(_type); }
        public static bool Get_AudioMute(E_SoundType _type)
        { return D_GetAudioMute(_type); }

        #endregion

    }
}