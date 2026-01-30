using LSM;
using System;
using UnityEngine;

namespace LSM
{
    public class Linker_Achievement
    {
        /// <summary>
        /// 모든 업적의 '값'이 변경 될 때 알림을 주는 옵저버 패턴.
        /// </summary>
        /// <param name="_mod">LSM.E_Achievements_Code</param>
        /// <param name="_ob">I_Observer</param>
        public static void Subscribe(E_Achievements_Code _mod, I_Observer _ob)
        { AchievementsManager.Achievement_Event_Subscribe((int)_mod, _ob);  }
        /// <summary>
        /// 모든 업적의 '값'이 변경 될 때 알림을 주는 옵저버 패턴 구독 취소.
        /// </summary>
        /// <param name="_mod">LSM.E_Achievements_Code</param>
        /// <param name="_ob">I_Observer</param>
        public static void UnSubscribe(E_Achievements_Code _mod, I_Observer _ob)
        { AchievementsManager.Achievement_Event_UnSubscribe((int)_mod, _ob); }



        /// <summary>
        /// _code에 맞는 업적의 '레벨'이 변화할때 실행되는 델리게이트 추가 함수.
        /// </summary>
        /// <param name="_code">LSM.E_Achievements_Code</param>
        /// <param name="_action">매개변수로 int값을 받는 함수. 변화 후의 레벨의 값이 들어감.</param>
        public static void Add_LevelChange(E_Achievements_Code _code, Action<int> _action)
        {
            AchievementsManager.Achievement_Event_LevelChange(_code, _action, true);
        }
        /// <summary>
        /// _code에 맞는 업적의 '레벨'이 변화할때 실행되는 델리게이트 제거 함수.
        /// </summary>
        /// <param name="_code">LSM.E_Achievements_Code</param>
        /// <param name="_action">매개변수로 int값을 받는 함수. 변화 후의 레벨의 값이 들어감.</param>
        public static void Remove_LevelChange(E_Achievements_Code _code, Action<int> _action)
        { AchievementsManager.Achievement_Event_LevelChange(_code, _action, false); }



        /// <summary>
        /// _code에 맞는 업적의 값을 추가하는 함수.
        /// </summary>
        /// <param name="_code">LSM.E_Achievements_Code</param>
        /// <param name="_value">더하는 값.</param>
        public static void Add_AchievementValue(E_Achievements_Code _code, int _value)
        { AchievementsManager.Achievement_Event_SetValue(_code, _value, false); }

        /// <summary>
        /// _code에 맞는 업적의 값을 세팅하는 함수.
        /// </summary>
        /// <param name="_code">LSM.E_Achievements_Code</param>
        /// <param name="_value">세팅할 값.</param>
        public static void Set_AchievementValue(E_Achievements_Code _code, int _value)
        { AchievementsManager.Achievement_Event_SetValue(_code, _value, true); }




        /// <summary>
        /// 업적의 개수를 반환.
        /// </summary>
        /// <returns></returns>
        public static int Get_AchievementLength()
        { return AchievementsManager.Achievement_Event_GetLength(); }


        /// <summary>
        /// _code에 맞는 업적의 값을 받아오는 함수.
        /// </summary>
        /// <param name="_code">LSM.E_Achievements_Code</param>
        /// <returns></returns>
        public static int Get_AchievementValue(E_Achievements_Code _code)
        { return AchievementsManager.Achievement_Event_Get(_code).CurValue; }
        /// <summary>
        /// _code에 맞는 업적의 이름을 받아오는 함수.
        /// </summary>
        /// <param name="_code">LSM.E_Achievements_Code</param>
        /// <returns></returns>
        public static string Get_AchievementName(E_Achievements_Code _code)
        { return AchievementsManager.Achievement_Event_Get(_code).data._name; }
        /// <summary>
        /// _code에 맞는 업적의 현재 레벨을 받아오는 함수.
        /// </summary>
        /// <param name="_code">LSM.E_Achievements_Code</param>
        /// <returns></returns>
        public static int Get_AchievementLevel(E_Achievements_Code _code)
        { return AchievementsManager.Achievement_Event_Get(_code).CurLevel; }

        public static int Get_AchievementRequireLevel(E_Achievements_Code _code)
        { return AchievementsManager.Achievement_Event_LevelRequireScore(_code); }

        public static int Get_AchievementCurrentScore(E_Achievements_Code _code)
        {
            int result = Get_AchievementValue(_code) - (Get_AchievementRequireLevel(_code) * Get_AchievementLevel(_code));
            return result;
                }
    }

}