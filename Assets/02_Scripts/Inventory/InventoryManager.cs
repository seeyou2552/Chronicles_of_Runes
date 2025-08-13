using Unity.VisualScripting;
using UnityEngine;

namespace _02_Scripts.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        [SerializeField] public GameObject PotionItemPrefab;
        [SerializeField] public GameObject SkillItemPrefab;
        [SerializeField] public GameObject RuneItemPrefab;
        [SerializeField] public InventoryUIManager inventoryUIManager;
        [SerializeField] public ActiveSkillSlotController[] activeSkillSlots;
        public int Gold;

        public void AddItem(int id)
        {
            ItemData itemdata = ItemManager.Instance.GetItem(id);
            var emptySlot = inventoryUIManager.FindFirstEmptySlot();
            GameObject go = null;
            
            //도감에 추가
            if (!CollectorManager.Instance.itemDataDic.ContainsKey(itemdata.displayName))
                CollectorManager.Instance.itemDataDic.Add(itemdata.displayName, itemdata);
            switch (itemdata.itemType)
            {
                case ItemType.Potion:
                    //인벤토리에 이미 있는지
                    var diInInventory = inventoryUIManager.FindPotionInSlot((PotionData)itemdata);
                    if (diInInventory != null)
                    {
                        if (diInInventory.transform.parent.TryGetComponent<PotionSlotController>(out var controller))
                        {
                            diInInventory.GetComponent<PotionAmount>().curPotionAmount++;
                            EventBus.Publish(new PotionChangedEvent
                            {
                                slotIndex = controller.slotIndex, 
                                potionData = (PotionData)itemdata,
                                currentAmount = diInInventory.GetComponent<PotionAmount>().curPotionAmount
                            });
                        }
                        else
                        {
                            diInInventory.GetComponent<PotionAmount>().curPotionAmount++;
                        }
                        return;
                    }
                    //없으면 새로 인벤토리에 생성
                    if (emptySlot == null)
                    {
                        StaticNoticeManager.Instance.ShowSideNotice("인벤토리가 가득 차 \n 아이템을 잃어버렸습니다.",5f);
                        return;
                    }
                    go = GameObject.Instantiate(PotionItemPrefab,
                        inventoryUIManager.FindFirstEmptySlot().transform);
                    go.GetComponent<PotionAmount>().curPotionAmount = 1;
                    break;
                case ItemType.SkillBook:
                    if (emptySlot == null)
                    {
                        StaticNoticeManager.Instance.ShowSideNotice("인벤토리가 가득 차 \n 아이템을 잃어버렸습니다.",5f);
                        return;
                    }
                    GameManager.Instance.magicGetCount++; // 마법서 획득 횟수 저장
                    go = GameObject.Instantiate(SkillItemPrefab,
                        inventoryUIManager.FindFirstEmptySlot().transform);
                    // go.AddComponent<SkillItemController>();
                    break;
                case ItemType.Rune:
                    if (emptySlot == null)
                    {
                        StaticNoticeManager.Instance.ShowSideNotice("인벤토리가 가득 차 \n 아이템을 잃어버렸습니다.",5f);
                        return;
                    }
                    GameManager.Instance.runeGetCount++; // 룬 획득 횟수 저장
                    go = GameObject.Instantiate(RuneItemPrefab,
                        inventoryUIManager.FindFirstEmptySlot().transform);
                    break;
            }
            
            if (go != null)
            {
                go.GetComponent<DraggableItem>().Setup(itemdata);
                EventBus.Publish("GetItem", this);
            }
        }
        
        public void addGold(int amount)
        {
            Gold += amount;
            PlayerController.Instance.GetStatModifier().SetMoneyShotAtk(Gold);
        }

        public void consumGold(int amount)
        {
            Gold -= amount;
            PlayerController.Instance.GetStatModifier().SetMoneyShotAtk(Gold);
        }
    }
}