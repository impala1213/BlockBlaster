using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameScene_UI : MonoBehaviour
{
    [SerializeField] GameObject settingObj;
    [SerializeField] SettingsModal setting_modal;
    CanvasGroup setting_group;

    bool isEnable;
    [Header("Button")]
    [SerializeField] private Button retryBtn, settingModalBtn, quitBtn;
    [SerializeField] private Button setting_Quit;
    [SerializeField] private Button settingBtn_Enable;

    [SerializeField] private Button background_btn;

    [Header("UI")]
    [SerializeField] private GameObject doubleCheckUi;
    [SerializeField] private Button dc_background, dc_nope, dc_yap;

    private string lobbySceneName = "Lobby";

    IEnumerator ie_ui;

    private void Awake()
    {
        setting_group = settingObj.GetComponent<CanvasGroup>();

        settingBtn_Enable.onClick.AddListener(()=> Ui_Enable(true));

        retryBtn.onClick.AddListener(()=>Ui_Enable(false));
        background_btn.onClick.AddListener(() => Ui_Enable(false));

        settingModalBtn.onClick.AddListener(() =>
        {
            Ui_Enable(false);
            setting_modal.Show();
        });
        setting_Quit.onClick.AddListener(() =>
        {
            setting_modal.Hide();
            Ui_Enable(true);
        });

        quitBtn.onClick.AddListener(() => StartCoroutine(IE_DC_Enable()));
        dc_background.onClick.AddListener(()=> doubleCheckUi.SetActive(false));
        dc_nope.onClick.AddListener(() => doubleCheckUi.SetActive(false));

        dc_yap.onClick.AddListener(() => SceneManager.LoadScene(lobbySceneName));

        isEnable = false;
    }

    public void Ui_Enable(bool  b)
    {
        if (ie_ui != null) { StopCoroutine(ie_ui); }
        ie_ui = IE_UIEnable(b);
        StartCoroutine(ie_ui);
    }

    IEnumerator IE_UIEnable(bool b)
    {
        settingObj.SetActive(true);
        float start_t = b ? 0 : 1;
        float dest_t = b ? 1 : 0;

        float startTime = Time.realtimeSinceStartup;
        float timer_d = 0;

        float base_time = 0.5f;

        while(timer_d < base_time)
        {
            timer_d = Time.realtimeSinceStartup - startTime;
            float value_d = Mathf.Lerp(start_t, dest_t, timer_d/base_time);
            setting_group.alpha = value_d;
            yield return null;
        }
        setting_group.alpha = b?1:0;

        settingObj.SetActive(b);
    }

    IEnumerator IE_DC_Enable()
    {
        doubleCheckUi.SetActive(true);
        float start_t = 0 ;
        float dest_t = 1;

        float startTime = Time.realtimeSinceStartup;
        float timer_d = 0;

        float base_time = 0.25f;

        while (timer_d < base_time)
        {
            timer_d = Time.realtimeSinceStartup - startTime;
            float value_d = Mathf.Lerp(start_t, dest_t, timer_d / base_time);
            doubleCheckUi.transform.localScale = new Vector3(1f,value_d,1f);
            yield return null;
        }
        doubleCheckUi.transform.localScale = new Vector3(1f, 1f, 1f);

    }
}
