using LSM;
using UnityEngine;

public class Test_Achievement : MonoBehaviour, LSM.I_Observer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log($"Achievement Length {Linker_Achievement.Get_AchievementLength()}");


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Linker_Achievement.Add_AchievementValue(E_Achievements_Code.ClearBlock, 200);
            Debug.Log("Clearblock + 200");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Linker_Achievement.Add_AchievementValue(E_Achievements_Code.ClearBlock, -200);
            Debug.Log("Clearblock - 200");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Linker_Achievement.Set_AchievementValue(E_Achievements_Code.ClearBlock, 100);
            Debug.Log("Clearblock Set 100");
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log($" ClearBlock = {Linker_Achievement.Get_AchievementValue(E_Achievements_Code.ClearBlock)}" +
                $"\nLevel = {Linker_Achievement.Get_AchievementLevel(E_Achievements_Code.ClearBlock)}");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log($" Score = {Linker_Achievement.Get_AchievementValue(E_Achievements_Code.Score)}"+
                $"\nLevel = {Linker_Achievement.Get_AchievementLevel(E_Achievements_Code.Score)}");
        }


        if (Input.GetKeyDown(KeyCode.Z))
        {
            Linker_Achievement.Add_AchievementValue(E_Achievements_Code.Score, 200);
            Debug.Log("Score +200");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Linker_Achievement.Add_AchievementValue(E_Achievements_Code.Score, -200);
            Debug.Log("Score -200");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Linker_Achievement.Set_AchievementValue(E_Achievements_Code.ClearBlock, 100);
            Debug.Log("Score Set 100");
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Linker_Achievement.Add_LevelChange(E_Achievements_Code.ClearBlock, Debug_GetLevel_clear);
            Debug.Log("LevelChange Action (ClearBlock) +");
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            Linker_Achievement.Remove_LevelChange(E_Achievements_Code.ClearBlock, Debug_GetLevel_clear);
            Debug.Log("LevelChange Action (ClearBlock) -");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Linker_Achievement.Add_LevelChange(E_Achievements_Code.Score, Debug_GetLevel_score);
            Debug.Log("LevelChange Action (Score) +");
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            Linker_Achievement.Remove_LevelChange(E_Achievements_Code.Score, Debug_GetLevel_score);
            Debug.Log("LevelChange Action (Score) -");
        }


        if (Input.GetKeyDown(KeyCode.O))
        {
            RemoveSubject(0);
            Debug.Log("ValueChange Observer -");
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddSubject(0);
            Debug.Log("ValueChange Observer +");
        }
    }

    public void Debug_GetLevel_clear(int n)
    {Debug.Log($"<color=green>ClearBlock LevelUP! = {n}</color>");}

    public void Debug_GetLevel_score(int n)
    { Debug.Log($"<color=green>Score LevelUP! = {n}</color>"); }

    public void AddSubject(int mod)
    {
        Linker_Achievement.Subscribe((E_Achievements_Code)mod, this);
    }

    public void RemoveSubject(int mod)
    {
        Linker_Achievement.UnSubscribe((E_Achievements_Code)mod, this);
    }

    public void Notify()
    {
        Debug.Log($"Value Change!!");
    }
}
