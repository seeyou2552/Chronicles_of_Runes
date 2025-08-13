using UnityEngine;
using UnityEngine.UI;

public class InventoryFilter : MonoBehaviour
{
    //인벤토리 버튼(아이템 필터링 기능)을 담당하는 스크립트

    public Button skillBookButton;
    public Button runeButton;
    public Button potionButton;//일단은 포션만을 표시
    public Transform invenContainer; // 인벤토리 ItemSlot이 있는 Grid 넣어주기

    //여기서 None은 필터 해제로 사용한다
    //스킬북, 룬, 포션 외 다른 아이템이 추가 된다면 None 외의 다른 enum타입을 줘야 한다.
    //그렇게 하지 않는다면 필터 전용 enum을 만들기
    //현재 필터를 외부에서 조회할 수 있는 정적 프로퍼티
    public static ItemType CurrentFilter { get; private set; } = ItemType.None;

    private void Start()
    {
        skillBookButton.onClick.AddListener(() => ToggleFilter(ItemType.SkillBook));
        runeButton.onClick.AddListener(() => ToggleFilter(ItemType.Rune));
        potionButton.onClick.AddListener(() => ToggleFilter(ItemType.Potion));

    }
    //버튼을 눌렀을 때 호출
    private void ToggleFilter(ItemType itemType)
    {
        CurrentFilter = (CurrentFilter == itemType) ? ItemType.None : itemType;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        // 아이템의 icon이 투명해지게
        foreach (var draggable in invenContainer.GetComponentsInChildren<DraggableItem>(true))
        {
            var cg = draggable.GetComponent<CanvasGroup>()
                     ?? draggable.gameObject.AddComponent<CanvasGroup>();

            if (CurrentFilter == ItemType.None ||
                draggable.itemData.itemType == CurrentFilter)
            {
                cg.alpha = 1f;
            }
            else
            {
                cg.alpha = 0.3f;
            }
        }
    }
}
