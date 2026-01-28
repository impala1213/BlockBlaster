using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SocialPlatforms.Impl;

namespace LSM {

    public enum E_Achievements_Code
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
        private Dictionary<E_Achievements_Code, C_Achievements> dict_acheivement;

        #region ---------- Delegate
        private static Action<E_Achievements_Code, int, bool> a_Set_Achievement;
        
        private static Action<int, I_Observer> a_subscribe;
        private static Action<int, I_Observer> a_unsubscribe;

        private static Action<E_Achievements_Code, Action<int>, bool> a_add_levelchange;

        private delegate C_Achievements d_getValue(E_Achievements_Code _code);
        private static event d_getValue D_Get;

        private delegate int d_getLength();
        private static event d_getLength D_GetLength;

        private delegate int d_getRequire(E_Achievements_Code _code);
        private static event d_getRequire D_GetRequire;


        #endregion

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
            Init_();

            a_subscribe = Subscribe;
            a_unsubscribe = UnSubscribe;
            D_Get = Get_Achievement;
            D_GetLength = ()=>System.Enum.GetValues(typeof(E_Achievements_Code)).Length;
            D_GetRequire = Get_AchievementLevelRequire;

            a_Set_Achievement = Add_Achievement;
            a_add_levelchange = Add_LevelChange;

            
        }

        private void Init_()
        {
            dict_acheivement = new Dictionary<E_Achievements_Code, C_Achievements>();

            if (achievementData.Count != System.Enum.GetValues(typeof(E_Achievements_Code)).Length)
            { Debug.LogError($"AchievementManager -> ScriptableObject != Achievement Types"); }

            for (int i = 0; i < achievementData.Count; i++)
            {
                // ToDo. 세이브 파일이 존재한다면 여기서 리셋할 것.
                C_Achievements d_class = new C_Achievements(achievementData[i]);
                dict_acheivement.Add(d_class.data._type, d_class);
            }

            observers = new List<I_Observer>[System.Enum.GetValues(typeof(E_Achievements_Code)).Length];
            for (int i = 0; i < observers.Length; i++)
            { observers[i] = new List<I_Observer>(); }
        }


        #region ---------- Event용 메소드
        public static void Achievement_Event_Subscribe(int _mod, I_Observer _ob)
        { a_subscribe?.Invoke(_mod, _ob); }
        public static void Achievement_Event_UnSubscribe(int _mod, I_Observer _ob)
        { a_unsubscribe?.Invoke(_mod, _ob); }

        public static void Achievement_Event_SetValue(E_Achievements_Code _mod, int _value, bool is_reset)
        { a_Set_Achievement?.Invoke(_mod, _value, is_reset); }
        public static C_Achievements Achievement_Event_Get(E_Achievements_Code _code)
        { return D_Get?.Invoke(_code); }
        public static int Achievement_Event_GetLength()
        { return D_GetLength.Invoke(); }
        public static void Achievement_Event_LevelChange(E_Achievements_Code _code, Action<int> d_action, bool b)
        {a_add_levelchange(_code, d_action, b); }
        public static int Achievement_Event_LevelRequireScore(E_Achievements_Code _code)
        { return D_GetRequire.Invoke(_code); }
        
        #endregion


        /// <summary>
        /// 업적의 종류, 값, 초기화 여부(true = 초기화, false = 추가)
        /// </summary>
        public void Add_Achievement(E_Achievements_Code mod, int v, bool is_reset)
        {
            C_Achievements d_class = dict_acheivement[mod];
            d_class.CurValue = is_reset ? v : v + d_class.CurValue;
            Notify();
        }
        
        public C_Achievements Get_Achievement(E_Achievements_Code _code)
        {
            return dict_acheivement[_code];
        }

        public void Add_LevelChange(E_Achievements_Code _mod, Action<int> d_action, bool b)
        {
            if (b)
            { dict_acheivement[_mod].Add_LevelChange(d_action); }
            else
            { dict_acheivement[_mod].Remove_LevelChange(d_action); }
        }

        public int Get_AchievementLevelRequire(E_Achievements_Code _code)
        {
            C_Achievements d_class = Get_Achievement(_code);
            return d_class.CurLevel * d_class.data._invert;
        }
    }
}