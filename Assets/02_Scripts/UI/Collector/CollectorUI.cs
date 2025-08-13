using _02_Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class CollectorUI : MonoBehaviour, IUIInterface
{
    public List<Button> buttons = new List<Button>();
    [SerializeField] private Button exitBtn;
    [SerializeField] private GameObject itemTooltip;
    public List<CollectorSlot> slots; //이건 모두 있을 예정
    public List<GameObject> skillList = new List<GameObject>();
    public List<GameObject> runeList = new List<GameObject>();
    public List<GameObject> itemList = new List<GameObject>();
    public List<GameObject> achievementList = new List<GameObject>();

    private void Awake()
    {
        int cnt = 0;
        foreach (var button in buttons)
        {
            int index = cnt;
            button.onClick.AddListener(() => SelectBtn(index));
            cnt++;
        }

        exitBtn.onClick.AddListener(CloseUI);
    }
    private void OnEnable()
    {
        Time.timeScale = 0f;
        HideSlotAll();
        foreach (var slot in slots)
        {
            if (slot.itemData != null && CollectorManager.Instance.itemDataDic.ContainsKey(slot.itemData.displayName))
            {
                slot.icon.sprite = slot.itemData.icon;
                slot.GetComponent<SlotTooltipEventPublisher>().enabled = true;
            }
            else if (slot.achievementData != null && CollectorManager.Instance.achievDataDic.ContainsKey(slot.achievementData.displayName))
            {
                slot.icon.sprite = slot.achievementData.icon;
                slot.GetComponent<SlotTooltipEventPublisher>().enabled = true;
            }
            else
            {
                slot.GetComponent<SlotTooltipEventPublisher>().enabled = false;
            }
        }
        SelectBtn(0);
    }

    private void OnDisable()
    {
        Time.timeScale = 1.0f;
    }

    public void HideSlotAll()
    {
        foreach (var slot in skillList)
        {
            slot.SetActive(false);
        }
        foreach (var slot in runeList)
        {
            slot.SetActive(false);
        }
        foreach (var slot in itemList)
        {
            slot.SetActive(false);
        }
        foreach (var slot in achievementList)
        {
            slot.SetActive(false);
        }
    }

    public void SelectBtn(int index)
    {
        HideSlotAll();
        switch (index)
        {
            case 0:
                foreach (var slot in skillList)
                {
                    slot.SetActive(true);
                }
                break;
            case 1:
                foreach (var slot in runeList)
                {
                    slot.SetActive(true);
                }
                break;
            case 2:
                foreach (var slot in itemList)
                {
                    slot.SetActive(true);
                }
                break;
            case 3:
                foreach (var slot in achievementList)
                {
                    slot.SetActive(true);
                }
                break;
        }
    }

    //public void ExitBtn()
    //{
    //    gameObject.SetActive(false);
    //}
    public void OpenUI()
    {
        if (!UIManager.Instance.RegisterUI(this))
            return;
        gameObject.SetActive(true);
    }
    public void CloseUI()
    {
        gameObject.SetActive(false);
        itemTooltip.SetActive(false);
        UIManager.Instance.UnRegisterUI(this);
    }
}
