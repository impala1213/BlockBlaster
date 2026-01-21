using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SocialPlatforms.Impl;

namespace LSM {

    public enum E_Acheivements_Code
    {
        ClearBlock=0, Score = 1
    }

    /// <summary>
    /// 업적 매니저.
    /// </summary>
    public class AchievementsManager : MonoBehaviour, I_Subject
    {
        [SerializeField]private List<SO_Achievements> achievementData;

        string ResourcePath = "Scriptable/LSM/Achievements/";

        private List<I_Observer>[] observers;
        private Dictionary<E_Acheivements_Code, C_Achievements> dict_acheivement;

        private static Action<int, bool> add_ClearBlock;
        private static Action<int, bool> add_Score;

        #region ---------- Observer. UI와 연결할 것.

        public void Notify()
        {
            foreach (var d in observers)
            {
                for (int i = 0; i < d.Count; i++)
                { d[i].Notify(); }
            }
        }

        public void Subscribe(int mod, I_Observer _observer)
        {
            if (mod > achievementData.Count) { return; }
            observers[mod].Add(_observer);
        }

        public void UnSubscribe(int mod, I_Observer _observer)
        {
            if (mod > achievementData.Count) { return; }
            observers[mod].Remove(_observer);
        }

        #endregion

        private void Awake()
        {
            achievementData = Resources.LoadAll<SO_Achievements>(ResourcePath).ToList();
            dict_acheivement = new Dictionary<E_Acheivements_Code, C_Achievements>();

            add_ClearBlock += (int _value, bool is_reset) =>
            {
                C_Achievements d_class = dict_acheivement[E_Acheivements_Code.ClearBlock];
                d_class.CurValue = is_reset ? _value:d_class.CurValue + _value;
            };
            add_Score += (int _value, bool is_reset) =>
            {
                C_Achievements d_class = dict_acheivement[E_Acheivements_Code.Score];
                d_class.CurValue = is_reset ? _value : d_class.CurValue + _value;
            };
        }


        /// <summary>
        /// 업적의 종류, 값, 초기화 여부(true = 초기화, false = 추가)
        /// </summary>
        public static void Add_Achievement(E_Acheivements_Code mod, int v, bool is_reset)
        {
            switch (mod)
            {
                case E_Acheivements_Code.ClearBlock:
                    add_ClearBlock(v,is_reset);
                    break;
                case E_Acheivements_Code.Score:
                    add_Score(v,is_reset);
                    break;
            }
        }
        
    }
}