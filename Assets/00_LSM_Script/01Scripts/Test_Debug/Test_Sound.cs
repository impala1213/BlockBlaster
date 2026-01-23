using UnityEngine;

public class Test_Sound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Linker_Sound.Audio_Set(LSM.E_SoundType.SFX,"sfx_01");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Linker_Sound.Audio_Set(LSM.E_SoundType.SFX, "sfx_02");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Linker_Sound.Audio_Set(LSM.E_SoundType.SFX, "sfx_03");
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            Linker_Sound.Audio_Set(LSM.E_SoundType.BGM, "bgm_01");
        }


        if (Input.GetKeyDown(KeyCode.Z))
        {
            Linker_Sound.Audio_MuteToggle(LSM.E_SoundType.Master);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Linker_Sound.Audio_MuteToggle(LSM.E_SoundType.BGM);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Linker_Sound.Audio_MuteToggle(LSM.E_SoundType.SFX);
        }

        if (Input.GetKeyDown(KeyCode.M))
        { Linker_Sound.Audio_VolumeAlpha(LSM.E_SoundType.Master, 0.1f); }
        if (Input.GetKeyDown(KeyCode.N))
        { Linker_Sound.Audio_VolumeAlpha(LSM.E_SoundType.Master, -0.1f); }
    }
}
