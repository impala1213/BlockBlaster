using LSM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Achievement_Slot : MonoBehaviour, LSM.I_Observer
{
    C_Achievements data;
    [SerializeField] private TextMeshProUGUI title_txt;
    [SerializeField] private TextMeshProUGUI lv_txt;
    [SerializeField] private TextMeshProUGUI value_txt;

    [SerializeField] private RectTransform fill_background;
    [SerializeField] private Image cur_img;

    Vector2 baseSize;

    private E_Achievements_Code ac_code;

    private void Start()
    {
        baseSize = new Vector2(fill_background.rect.width, fill_background.rect.height);
        //cur_img.rectTransform.sizeDelta = new Vector2(fill_background.rect.width * 0.5f, fill_background.rect.height);
        Setting_(E_Achievements_Code.ClearBlock);
    }

    public void Setting_(E_Achievements_Code _code)
    {
        ac_code = _code;
        title_txt.text = Linker_Achievement.Get_AchievementName(ac_code);
        lv_txt.text = $"LV. {Linker_Achievement.Get_AchievementLevel(ac_code)}";

        int cur_v = Linker_Achievement.Get_AchievementValue(ac_code);
        int require_v = Linker_Achievement.Get_AchievementRequireLevel(ac_code);
        value_txt.text = $"{cur_v} / {require_v}";
        float level_rate = (float)cur_v / (float)require_v;
        cur_img.rectTransform.sizeDelta = new Vector2(baseSize.x * level_rate, baseSize.y);

        AddSubject((int)ac_code);
        Linker_Achievement.Add_LevelChange(ac_code, Refresh_Level);
    }

    public void Refresh_CurValue()
    {
        d
    }

    public void Refresh_Level(int _level)
    {
        
    }

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
        Refresh_CurValue();
    }
}
