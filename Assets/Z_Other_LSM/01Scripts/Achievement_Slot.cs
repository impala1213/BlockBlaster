using LSM;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


// 처음 업적을 열면 값 세팅.
// 이후 레벨업 시 레벨만큼 꽉채워졌다가 풀어졌다가를 반복.
// 레벨업 시 아이콘이 커졌다 작아지기.
public class Achievement_Slot : MonoBehaviour, LSM.I_Observer
{
    C_Achievements data;
    [SerializeField] private TextMeshProUGUI title_txt;
    [SerializeField] private TextMeshProUGUI lv_txt;
    [SerializeField] private TextMeshProUGUI value_txt;

    [SerializeField] private GameObject levelUp_Icon;

    [SerializeField] private RectTransform fill_background;
    [SerializeField] private Image cur_img;

    Vector2 baseSize;

    private E_Achievements_Code ac_code;

    private int pre_level, cur_level;
    private int pre_v, cur_v;

    IEnumerator ie_scaleAnim;

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
        
        // 처음 시작하는 경우 전부 최근 경험치 및 레벨로 전환.
        pre_v = cur_v = Linker_Achievement.Get_AchievementCurrentScore(ac_code);
        pre_level = cur_level = Linker_Achievement.Get_AchievementLevel(ac_code);

        Refresh_CurValue();
        //Linker_Achievement.Add_LevelChange(ac_code, Refresh_Level);
    }

    // 모든 값을 천천히 올라가게.
    private void OnEnable()
    {
        //cur_v = Linker_Achievement.Get_AchievementCurrentScore(ac_code);
        cur_level = Linker_Achievement.Get_AchievementLevel(ac_code);
        StartCoroutine(CurLevelAnim());
        StartCoroutine(CurValueAnim());
        AddSubject((int)ac_code);



    }

    private void OnDisable()
    {
        StopCoroutine(CurLevelAnim());
        StopCoroutine(CurValueAnim());
        RemoveSubject((int)ac_code);
    }

    // 첫 실행 시에는 곧바로 값을 지정.
    // 두번째 이후 부터는 레벨업 할 경우 횟수만큼 꽉 채우기.
    private IEnumerator CurValueAnim()
    {
        while (true)
        {
            float d_n = ((float)cur_v - pre_v) * 0.2f;
            int sign_ = d_n < 0 ? -1 : 1;
            int alpha_ = Mathf.CeilToInt(Mathf.Abs(d_n));
            pre_v += alpha_ * sign_;
            if (Mathf.Abs(pre_v - cur_v) <= cur_v * 0.1f || Mathf.Abs(pre_v - cur_v) < 1)
            { pre_v = cur_v; }
            //Debug.Log($"{cur_v}, {pre_v}");
            int require_v = Linker_Achievement.Get_AchievementRequireLevel(ac_code);
            value_txt.text = $"{pre_v} / {require_v}";
            float level_rate = (float)pre_v / (float)require_v;
            cur_img.rectTransform.sizeDelta = new Vector2(baseSize.x * level_rate, baseSize.y);
            yield return new WaitForSeconds(0.05f);
        }
    }

    // i레벨부터 j레벨까지.
    private IEnumerator CurLevelAnim()
    {
        do
        {
            if (pre_level < cur_level)
            {
                cur_v = 0;
                pre_v = 0;
                yield return null;
                cur_v = Linker_Achievement.Get_AchievementRequireLevel(ac_code);
                pre_level++;
                lv_txt.text = $"LV. {pre_level}";
                Level_Up();
                yield return null;
            }
            else
            {
                pre_v = 0;
                cur_v = Linker_Achievement.Get_AchievementCurrentScore(ac_code);
                pre_level = Linker_Achievement.Get_AchievementLevel(ac_code);
                lv_txt.text = $"LV. {Linker_Achievement.Get_AchievementLevel(ac_code)}";
                break;
            }

            yield return new WaitUntil(() => pre_v >= cur_v);
        } while (pre_level <= cur_level);
    }

    public void Refresh_CurValue()
    {
        cur_v = Linker_Achievement.Get_AchievementCurrentScore(ac_code);
        //int require_v = Linker_Achievement.Get_AchievementRequireLevel(ac_code);

        //value_txt.text = $"{cur_v} / {require_v}";
        //float level_rate = (float)cur_v / (float)require_v;
        //cur_img.rectTransform.sizeDelta = new Vector2(baseSize.x * level_rate, baseSize.y);
    }

    public void Level_Up()
    {
        if (ie_scaleAnim != null)
        { StopCoroutine(ie_scaleAnim); }
        ie_scaleAnim = IE_SizeAnim(0.25f, levelUp_Icon, 1f, 1.5f);
        StartCoroutine(ie_scaleAnim);
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
        if(gameObject.activeSelf)
            Refresh_CurValue();
    }

    private IEnumerator IE_SizeAnim(float _time, GameObject _obj,float start_sc, float dest_sc)
    {
        float start_time = Time.realtimeSinceStartup;
        float timer_d = 0;

        bool isAlpha = true;

        float half_time = _time / 2f;
        while(timer_d <= _time)
        {
            float d_start = isAlpha? start_sc : dest_sc;
            float d_dest = isAlpha? dest_sc : start_sc;

            timer_d = Time.realtimeSinceStartup - start_time;
            float timer_rate = ((timer_d - (isAlpha?0:half_time)) / half_time);

            _obj.transform.localScale = Vector3.one * (Mathf.Lerp(d_start, d_dest, timer_rate));

            if (isAlpha && timer_d >= half_time)
            { isAlpha = false; }
            yield return null;
        }
        _obj.transform.localScale = Vector3.one * start_sc;
    }
}
