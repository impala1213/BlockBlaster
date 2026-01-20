using UnityEngine;

namespace LSM
{

    /// <summary>
    /// 업적 클래스.
    /// 코드, 이름, 레벨, 현재 값
    /// </summary>
    [CreateAssetMenu(fileName = "Acheivements", menuName = "ScriptableObject/LSM/Achievements")]
    public class Achievements:ScriptableObject
    {
        public string _code;
        public string _name;
        public E_Acheivements_Code _type;
        public int _level;
        public int _cur;
        public int _invert;
    }
}