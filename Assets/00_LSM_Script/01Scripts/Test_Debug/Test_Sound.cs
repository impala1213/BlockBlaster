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
            AudioLinker.Audio_Set(LSM.E_SoundType.SFX,"sfx_01");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            AudioLinker.Audio_Set(LSM.E_SoundType.SFX, "sfx_02");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            AudioLinker.Audio_Set(LSM.E_SoundType.SFX, "sfx_03");
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            AudioLinker.Audio_Set(LSM.E_SoundType.BGM, "bgm_01");
        }


        if (Input.GetKeyDown(KeyCode.Z))
        {
            AudioLinker.Audio_MuteToggle(LSM.E_SoundType.Master);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            AudioLinker.Audio_MuteToggle(LSM.E_SoundType.BGM);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            AudioLinker.Audio_MuteToggle(LSM.E_SoundType.SFX);
        }

        if (Input.GetKeyDown(KeyCode.M))
        { AudioLinker.Audio_VolumeAlpha(LSM.E_SoundType.Master, 0.1f); }
        if (Input.GetKeyDown(KeyCode.N))
        { AudioLinker.Audio_VolumeAlpha(LSM.E_SoundType.Master, -0.1f); }
    }
}
