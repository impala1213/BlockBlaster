using UnityEngine;

namespace LSM
{
    [CreateAssetMenu(fileName = "SoundClip", menuName = "ScriptableObject/LSM/SoundClip")]
    public class SO_SoundClip : ScriptableObject
    {
        public E_SoundClip_Type _clipType;
        public string _code;
        public AudioClip _clip;
    }

}