using LSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Achievement_SlotList : MonoBehaviour
{
    [SerializeField] private GameObject pannel, backGround;
    [SerializeField] private Button achieve_btn;

    [Header("Achievement Slots")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform content;
    [Header("Achievement Notify")]
    [SerializeField] private GameObject notify_Level;
    [SerializeField] private Transform notify_parent;

    List<Achievement_Slot> slots;



    private IEnumerator Start()
    {
        yield return null;
        slots = new List<Achievement_Slot>();
        for (int i = 0; i < Linker_Achievement.Get_AchievementLength(); i++) {

            GameObject d_obj = GameObject.Instantiate(slotPrefab, content.GetComponent<RectTransform>());
            //d_obj.transform.parent = content;

            Achievement_Slot d_ctrl = d_obj.GetComponent<Achievement_Slot>();

            d_ctrl.Setting_((E_Achievements_Code)i);
            slots.Add(d_ctrl);

            int j = i;
            Linker_Achievement.Add_LevelChange((E_Achievements_Code)i, 
                (g)=>LevelChange((E_Achievements_Code)j, g));
        }
        backGround.GetComponent<Button>().onClick.AddListener(() => pannel.SetActive(false));
        achieve_btn.onClick.AddListener(()=>Enable_Obj(true));
    }

    public void Enable_Obj(bool b)
    {
        pannel.SetActive(b);
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(b);
        }
    }

    public void LevelChange(E_Achievements_Code _code,int _level)
    {
        GameObject _obj = GameObject.Instantiate(notify_Level, notify_parent.GetComponent<RectTransform>());
        //_obj.transform.parent = notify_parent;
        _obj.GetComponent<Achievement_NotifySlot>().Setting(_code);
        _obj.transform.SetAsFirstSibling();
    }
}
