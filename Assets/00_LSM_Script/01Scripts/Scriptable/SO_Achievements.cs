using System;
using UnityEngine;

namespace LSM
{

    /// <summary>
    /// 업적 클래스.
    /// 코드, 이름, 레벨, 현재 값
    /// </summary>
    [CreateAssetMenu(fileName = "Acheivements", menuName = "ScriptableObject/LSM/Achievements")]
    public class SO_Achievements:ScriptableObject
    {
        public string _name;
        public E_Acheivements_Code _type;
        public int _invert;
    }

    /// <summary>
    /// 현재 플레이어의 업적을 담당하는 클래스.
    /// </summary>
    [Serializable]
    public class C_Achievements
    {
        public SO_Achievements data;
        private int _level;
        private int _cur;
        private Action<int> levelChange;

        public int CurValue{
            get => _cur;
            set
            {
                _cur = value;
                int d_level = Mathf.FloorToInt(_cur / data._invert);
                if (d_level != _level) { LevelChange(); }
                _level = d_level;
            }
        }

        public C_Achievements(SO_Achievements _data)
        {
            data = _data;
            _level = 0;
            _cur = 0;
        }

        public void LevelChange()
        {
            levelChange?.Invoke(_level);
        }

        public void Add_LevelChange(Action<int> _method)
        {
            levelChange -= _method;
            levelChange += _method;
        }
        public void Remove_LevelChange(Action<int> _method)
        {
            levelChange -= _method;
        }

    }
}