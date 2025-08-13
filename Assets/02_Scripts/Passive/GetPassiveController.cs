using System.Collections.Generic;
using _02_Scripts.Inventory;
using System.Collections;
using UnityEngine;

public class GetPassiveController : MonoBehaviour
{
    public List<PassiveBase> passiveObject;
    
    public PassiveBase[] SetItemData()  //랜덤하게 3개의 패시브 뽑아 리턴
    {
        PassiveBase[] items = new PassiveBase[3];
        List<PassiveBase> selected = new List<PassiveBase>(passiveObject);

        if (selected.Count < 3)
        {
            return null;
        }

        for (int i = 0; i < 3; i++)
        {
            int randIndex = Random.Range(0, selected.Count);
            items[i] = selected[randIndex];
            selected.RemoveAt(randIndex);
        }

        return items;
    }
    
    public void OnItemSelected(PassiveBase index)  //인벤토리에 패시브를 넣고 적용
    {
        PassiveBase selectedItem = index;

        GameObject temp = InventoryManager.Instance.inventoryUIManager.FindFirstEmptySlotPassive();
        if (temp != null)
        {
            PassiveSlot tempSlot = temp.GetComponent<PassiveSlot>();
            if (tempSlot.Passive == null)
            {
                tempSlot.Passive = selectedItem;
                tempSlot.ChangeImage();
                selectedItem.PassiveEffect();
            }
        }
        else
        {

        }
    }
}
