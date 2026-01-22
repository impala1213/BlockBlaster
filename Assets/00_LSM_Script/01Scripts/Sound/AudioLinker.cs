using UnityEngine;
using LSM;

public static class AudioLinker
{
    /// <summary>
    /// BGM 혹은 SFX의 오디오를 지정할때 사용.
    /// </summary>
    /// <param name="_type">LSM.E_SoundType.(BGM, SFX)</param>
    /// <param name="_name">코드.</param>
    public static void Audio_Set(E_SoundType _type, string _name)
    { SoundManager.Audio_Event_Set(_type, _name); }

    /// <summary>
    /// Master, BGM, SFX 믹서의 뮤트 설정.
    /// </summary>
    /// <param name="_type">LSM.E_SoundType.(Master, BGM, SFX) </param>
    /// <param name="_mute">true = 뮤트 / false = 뮤트해제</param>
    public static void Audio_Mute(E_SoundType _type, bool _mute) 
    { SoundManager.Audio_Event_Mute(_type, _mute); }

    /// <summary>
    /// Master, BGM, SFX 믹서의 볼륨 조정.
    /// </summary>
    /// <param name="_type">LSM.E_SoundType.(Master, BGM, SFX)</param>
    /// <param name="_volume">0f~1f 값</param>
    public static void Audio_Volume(E_SoundType _type, float _volume)
    { SoundManager.Audio_Event_Volume(_type, _volume); }

    /// <summary>
    /// Master, BGM, SFX 믹서의 볼륨 값 반환.
    /// </summary>
    /// <param name="_type">LSM.E_SoundType.(Master, BGM, SFX)</param>
    /// <returns>0f~1f 값.</returns>
    public static float Audio_GetVolume(E_SoundType _type) 
    { return SoundManager.Get_AudioVolume(_type); }
    /// <summary>
    /// Master, BGM, SFX 믹서의 뮤트 여부 반환
    /// </summary>
    /// <param name="_type">LSM.E_SoundType.(Master, BGM, SFX)</param>
    /// <returns></returns>
    public static bool Audio_GetMute(E_SoundType _type)
    { return SoundManager.Get_AudioMute(_type); }
    /// <summary>
    ///  BGM = 현재 재생중인 이름, SFX = 가장 마지막 사운드 이름
    /// </summary>
    /// <param name="_type">LSM.E_SoundType.(BGM, SFX)</param>
    /// <returns></returns>
    public static string Audio_GetCode(E_SoundType _type)
    { return SoundManager.Get_AudioCode(_type); }

}
