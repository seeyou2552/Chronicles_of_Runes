using _02_Scripts.Inventory;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : Singleton<ItemManager> 
{
    [SerializeField]
    private ItemTable itemTable;
    [SerializeField]
    private InventoryManager inventoryManager;
    
    public MonsterDropTable monsterDropTable;
    public StageRewardTable stageRewardTable;
    
    public ItemData GetItem(int id)
    {
        ItemData item = itemTable.GetItem(id);
        return item;
    }

    private void Start()
    {
        InventoryManager.Instance.addGold(50);
    }

    [ContextMenu("Add Item")]
    private void AddPotion()
    {
        InventoryManager.Instance.AddItem(2);
        InventoryManager.Instance.AddItem(3);
        InventoryManager.Instance.AddItem(4);
        InventoryManager.Instance.AddItem(5);
        InventoryManager.Instance.AddItem(6);
        InventoryManager.Instance.AddItem(7);
        InventoryManager.Instance.AddItem(8);
        InventoryManager.Instance.AddItem(9);
        InventoryManager.Instance.AddItem(10);
        InventoryManager.Instance.AddItem(14);
        InventoryManager.Instance.AddItem(15);
        InventoryManager.Instance.AddItem(16);
        InventoryManager.Instance.AddItem(17);
        InventoryManager.Instance.AddItem(18);
        InventoryManager.Instance.AddItem(19);
        InventoryManager.Instance.AddItem(20);
        InventoryManager.Instance.AddItem(27);
        InventoryManager.Instance.AddItem(28);
        InventoryManager.Instance.AddItem(29);
        InventoryManager.Instance.AddItem(30);
        InventoryManager.Instance.AddItem(31);
        InventoryManager.Instance.AddItem(32);
        InventoryManager.Instance.AddItem(33);
        InventoryManager.Instance.AddItem(34);
        InventoryManager.Instance.AddItem(35);
        InventoryManager.Instance.AddItem(36);
        InventoryManager.Instance.AddItem(37);

        InventoryManager.Instance.AddItem(38);


    }

    public class DropSystem
    {
        private MonsterDropTable dropTable;

        public DropSystem(MonsterDropTable table)
        {
            this.dropTable = table;
        }
        
        public List<int> GetDropItems(string monsterName, out int dropGold)
        {
            dropGold = 0;

            // 몬스터 정보 찾기
            MonsterDrop monster = dropTable.MonsterDropList.Find(m => m.MonsterName == monsterName);

            if (monster == null)
            {
                return new List<int>();
            }

            dropGold = monster.DropGold;
            List<int> droppedItems = new();

            foreach (var drop in monster.ItemDropList)
            {
                float roll = Random.value;
                if (roll <= drop.chance)
                {
                    droppedItems.Add(drop.itemId);
                }
            }

            return droppedItems;
        }
    }
}