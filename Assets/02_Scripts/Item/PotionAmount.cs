using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PotionAmount : MonoBehaviour, IBeginDragHandler
{
    //포션 아이템 프리팹에 붙이는 포션 보유량 스크립트


    //활성화/비활성화
    public GameObject potionAmount;
    //포션 보유량 텍스트
    public TextMeshProUGUI amountText;
    //보유 횟수 저장
    public int curPotionAmount = 0;

    void Start()
    {
        potionAmount.SetActive(true);
    }


    void Update()
    {
        amountText.text = curPotionAmount.ToString();
    }

    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        potionAmount.SetActive(false);//보유량 텍스트 숨기기
    }

    // 드래그 끝나고 난 뒤
    void OnTransformParentChanged()
    {
        var parent = transform.parent;
        //부모가 인벤토리 슬롯인지 아닌지
        bool inInventory = parent.GetComponent<InventorySlotController>() != null;
        //인벤토리 슬롯이면 보유량 보여주고 / 포션 슬롯이면 숨기기
        // potionAmount.SetActive(inInventory);
        potionAmount.SetActive(true);
    }
}
