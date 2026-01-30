using LSM;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Achievement_NotifySlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title_txt;
    
    public void Setting(E_Achievements_Code _code)
    {
        title_txt.text = Linker_Achievement.Get_AchievementName(_code);
        StartCoroutine(IE_Notify_Scale(1f));
        StartCoroutine(IE_NotifyIcon_Scale(0.5f));
    }

    private IEnumerator IE_Notify_Scale(float t)
    {
        float start_time = Time.realtimeSinceStartup;
        float scale_time = 0.3f;
        float destT = t * scale_time;

        float destT_scale = t * scale_time * 0.8f;
        float destT_scale_return = t * scale_time * 0.2f;

        RectTransform rect = this.GetComponent<RectTransform>();
        float start_height = rect.rect.height, base_width = rect.rect.width;


        float timer_d = 0;
        while(timer_d < destT)
        {
            timer_d = Time.realtimeSinceStartup - start_time;
            float _value = Mathf.Lerp(0f, start_height,(timer_d / destT));

            float _value_size;
            if (timer_d < destT_scale)
            { _value_size = Mathf.Lerp(0.5f, 1.2f, (timer_d / destT_scale)); }
            else
            { _value_size = Mathf.Lerp(1.2f, 1f, ((timer_d - destT_scale) / destT_scale_return)); }


            rect.sizeDelta = new Vector2(base_width, _value);
            this.transform.localScale = new Vector3(1f, _value_size, 1f);


            //this.transform.localScale = new Vector3(1f,_value,1f);
            yield return null;
        }
        yield return new WaitForSeconds(t*(1-scale_time));
        Destroy(this.gameObject);
    }

    private IEnumerator IE_NotifyIcon_Scale(float t)
    {
        float start_time = Time.realtimeSinceStartup;
        float timer_d = 0;
        while(timer_d < t)
        {
            timer_d = Time.realtimeSinceStartup -start_time;
            float _value = Mathf.Lerp(2f, 1f, (timer_d / t));
            icon.transform.localScale = Vector3.one * _value;
            yield return null;
        }
    }
}
