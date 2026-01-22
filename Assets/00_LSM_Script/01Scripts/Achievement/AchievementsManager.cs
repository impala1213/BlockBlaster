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

        private static Action<int, bool> a_add_ClearBlock;
        private static Action<int, bool> a_add_Score;

        private static Action<int, I_Observer> a_subscribe;
        private static Action<int, I_Observer> a_unsubscribe;

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

            a_add_ClearBlock += (int _value, bool is_reset) =>
            {
                C_Achievements d_class = dict_acheivement[E_Acheivements_Code.ClearBlock];
                d_class.CurValue = is_reset ? _value:d_class.CurValue + _value;
            };
            a_add_Score += (int _value, bool is_reset) =>
            {
                C_Achievements d_class = dict_acheivement[E_Acheivements_Code.Score];
                d_class.CurValue = is_reset ? _value : d_class.CurValue + _value;
            };
        }


        #region ---------- Event용 메소드
        public static void Achievement_Event_Subscribe(int _mod, I_Observer _ob)
        { a_subscribe?.Invoke(_mod, _ob); }
        public static void Achievement_Event_UnSubscribe(int _mod, I_Observer _ob)
        { a_unsubscribe?.Invoke(_mod, _ob); }
        public static void Achievement_Event_AddClearBlock(int _v, bool _isSet)
        { a_add_ClearBlock(_v, _isSet); }
        public static void Achievement_Event_AddScore(int _v, bool _isSet)
        { a_add_Score(_v,_isSet); }

        #endregion


        /// <summary>
        /// 업적의 종류, 값, 초기화 여부(true = 초기화, false = 추가)
        /// </summary>
        public static void Add_Achievement(E_Acheivements_Code mod, int v, bool is_reset)
        {
            switch (mod)
            {
                case E_Acheivements_Code.ClearBlock:
                    a_add_ClearBlock(v,is_reset);
                    break;
                case E_Acheivements_Code.Score:
                    a_add_Score(v,is_reset);
                    break;
            }
        }
        
    }
}