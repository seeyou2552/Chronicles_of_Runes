using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CollectorManager : Singleton<CollectorManager>
{
    public GameObject content;
    [SerializeField]
    private GameObject slotPrefab;
    private List<CollectorSlot> slots = new List<CollectorSlot>();
    public CollectDataListSo collectDataListSo;

    public CollectorUI collectorUI;

    //흠 왜 두개나 들고있어야하는거지?
    public Dictionary<string, ItemData> itemDataDic = new Dictionary<string, ItemData>();
    public Dictionary<string, AchievementData> achievDataDic = new Dictionary<string, AchievementData>();

    private void Start()
    {
        CollectorInit();
    }

    public void CollectorInit()
    {
        foreach (var data in collectDataListSo.skillList.skillData)
        {
            GameObject go = Instantiate(slotPrefab, content.transform);
            collectorUI.skillList.Add(go);
            CollectorSlot slotData = go.GetComponent<CollectorSlot>();
            slots.Add(slotData);

            //조건 달성시만 스프라이트 변경 일단은 임시로 아이콘이름으로 키값
            if (itemDataDic.ContainsKey(data.displayName))
            {
                slotData.icon.sprite = data.icon;
            }
            slotData.itemData = data;
        }

        foreach (var data in collectDataListSo.runeList.runeData)
        {
            GameObject go = Instantiate(slotPrefab, content.transform);
            collectorUI.runeList.Add(go);
            CollectorSlot slotData = go.GetComponent<CollectorSlot>();
            slots.Add(slotData);

            if (itemDataDic.ContainsKey(data.displayName))
            {
                slotData.icon.sprite = data.icon;
            }
            slotData.itemData = data;
        }

        foreach (var data in collectDataListSo.itemList.itemData)
        {
            GameObject go = Instantiate(slotPrefab, content.transform);
            collectorUI.itemList.Add(go);
            CollectorSlot slotData = go.GetComponent<CollectorSlot>();
            slots.Add(slotData);

            if (itemDataDic.ContainsKey(data.displayName))
            {
                slotData.icon.sprite = data.icon;
            }
            slotData.itemData = data;
        }

        foreach (var data in collectDataListSo.achievementList.achievementData)
        {
            GameObject go = Instantiate(slotPrefab, content.transform);
            collectorUI.achievementList.Add(go);
            CollectorSlot slotData = go.GetComponent<CollectorSlot>();
            slots.Add(slotData);

            if (achievDataDic.ContainsKey(data.displayName))
            {
                slotData.icon.sprite = data.icon;
            }
            slotData.achievementData = data;
        }

        collectorUI.slots = slots;
    }

}
