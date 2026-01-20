using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

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
        private List<Achievements> achievementData;

        string ResourcePath = "Scriptable/LSM/Ahievements/";

        private List<I_Observer>[] observers;
        private Dictionary<E_Acheivements_Code, int> dict_acheivement;

        private static Action<int, bool> add_ClearBlock;
        private static Action<int, bool> add_Score;

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

        private void Awake()
        {
            achievementData = Resources.LoadAll<Achievements>(ResourcePath).ToList();
            dict_acheivement = new Dictionary<E_Acheivements_Code, int>();

            add_ClearBlock += (int _value, bool is_reset) =>
            {
                dict_acheivement[E_Acheivements_Code.ClearBlock] = is_reset ? _value : dict_acheivement[E_Acheivements_Code.ClearBlock] + _value;
            };
            add_Score += (int _value, bool is_reset) =>
            {
                dict_acheivement[E_Acheivements_Code.Score] = is_reset ? _value : dict_acheivement[E_Acheivements_Code.ClearBlock] + _value;
            };
        }


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