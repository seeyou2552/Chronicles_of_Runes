using _02_Scripts.Inventory;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CrateController : NPCController
{
    public StageRewardTable stageRewardTable;
    
    public Canvas canvas;

    [Header("아이템 프리팹")]
    public GameObject potionItemPrefab;
    public GameObject runeItemPrefab;
    public GameObject skillItemPrefab;

    private ItemData[] items;
    private List<StageRewardEntry> rewardEntries;
    private bool isOpened = false;

    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        
    }
    void Start()
    {
        stageRewardTable = ItemManager.Instance.stageRewardTable;
        
        if (stageRewardTable == null)
        {
            return;
        }

        int stageNumber = GameManager.Instance.sceneFlowManager.GetCurrentStage();
        rewardEntries = stageRewardTable.GetRewards(stageNumber);
        if (rewardEntries == null || rewardEntries.Count < 3)
        {
            return;
        }

        SetItemData();
    }

    public bool IsOpened
    {
        get { return isOpened; }
        private set { isOpened = value; }
    }

    public ItemData[] SetItemData()
    {
        items = new ItemData[3];
        List<StageRewardEntry> pool = new(rewardEntries);

        for (int i = 0; i < 3; i++)
        {
            if (pool.Count == 0) break;

            var selected = GetWeightedRandom(pool);
            items[i] = ItemManager.Instance.GetItem(selected.itemId); // ItemDatabase는 ID 기반 아이템 반환하는 클래스
            pool.Remove(selected);
        }

        return items;
    }

    private StageRewardEntry GetWeightedRandom(List<StageRewardEntry> entries)
    {
        float totalWeight = 0f;
        foreach (var entry in entries)
            totalWeight += entry.weight;

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in entries)
        {
            cumulative += entry.weight;
            if (rand <= cumulative)
                return entry;
        }

        return entries[0]; // fallback
    }

    public void OnItemSelected(int index)
    {
        IsOpened = true;
        ItemData selectedItem = items[index];

        InventoryManager.Instance.AddItem(selectedItem.id);
        canvas.gameObject.SetActive(false); // UI 닫기
        Time.timeScale = 1f;
        Destroy(this.gameObject);
    }
}
