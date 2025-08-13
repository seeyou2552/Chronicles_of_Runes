using _02_Scripts.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//없다면 만들어줘
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //드래그가 가능한 아이템 프리팹에 모두 붙일 수 있음


    //드래그 중인 상태를 전역으로 알려 주는 static 프로퍼티(툴팁에서 참조중)
    public static bool IsDragging { get; private set; }
    public static PointerEventData dragItem { get; private set; } // 드래그 중인 데이터

    // 드래그 자체를 허용할지 말지
    public bool dragEnabled = true;

    //스왑중인지 플래그
    [HideInInspector] public bool isSwapping = false;

    [SerializeField] private Image iconImage;
    public ItemData itemData;
    private Sprite defaultSprite; //기존 스프라이트
    public Vector2 defaultSize; //기존 사이즈
    public Transform originalParent { get; private set; } // 드래그 전 부모 위치
    public Canvas parentCanvas; // 최상위 캔버스
    private CanvasGroup canvasGroup; // Raycast 제어용
    private PotionSlotController fromPotionSlot; // 드래그 시작 시 포션 슬롯 여부 저장

    public float currentCoolTime; // 일단 쿨타임 저장용

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>()
                      ?? gameObject.AddComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();

        bool isSkill = false;
        if (this.TryGetComponentInParent<SkillSlotController>(out var slotController))
        {
            isSkill = true;
            defaultSprite = itemData.icon;
            // defaultSize = new Vector2(60, 60);
            // return;
        }

        if (itemData != null && itemData.icon != null)
        {
            if (!isSkill)
            {
                iconImage.sprite = itemData.icon;
            }
                iconImage.enabled = true;
        }


        if (iconImage != null)
        {
            defaultSprite = iconImage.sprite;
            var rt = GetComponent<RectTransform>();
            if (!isSkill)
            {
                defaultSize = rt.sizeDelta;    
            }
            else
            {
                defaultSize = new Vector2(60,60);    
            }
        }
    }

    //세팅하기(이미지를 data의 이미지 적용시키기)
    public void Setup(ItemData data)
    {
        itemData = data;
        iconImage.sprite = data.icon;
        iconImage.enabled = true;

        if (data.itemType == ItemType.SkillBook)
        {
            SkillBookData skill = data as SkillBookData;
            currentCoolTime = skill.skillSO.coolTime;
        }
    }

    #region 드래그 시작

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!dragEnabled)
        {
            eventData.pointerDrag = null;
            return;
        }

        IsDragging = true;
        originalParent = transform.parent;
        dragItem = eventData;

        // 포션 슬롯에서 드래그 시작했는지 체크
        fromPotionSlot = originalParent.GetComponent<PotionSlotController>();

        //스킬슬롯에서 드래그 시작했으면, 룬 UI 떼어내기
        var oldSkillSlot = originalParent.GetComponent<SkillSlotController>();
        if (oldSkillSlot != null)
        {
            if (PlayerSkillManager.Instance.slotSkills.TryGetValue(oldSkillSlot.slotIndex, out var slotData))
                currentCoolTime = slotData.currentCoolTime;
            //currentCoolTime = PlayerSkillManager.Instance.slotSkills[oldSkillSlot.slotIndex].currentCoolTime;
            oldSkillSlot.DetachRunes();
        }

        // 최상위 캔버스로 이동
        transform.SetParent(parentCanvas.transform, true);
        // Raycast 통과시키도록 설정 (Drop 가능하게)
        canvasGroup.blocksRaycasts = false;
        //드래그 시작하면 툴팁 숨기기
        EventBus.Publish(new TooltipHideEvent());
        //만약 슬롯 스왑을 위해 슬롯을 선택했는데 해당 슬롯에 있는 아이템을 드래그 했을 경우 스왑 상태 풀리게
        if (SwapSelectionManager.Instance != null)
            SwapSelectionManager.Instance.ClearSelection();
    }

    #endregion

    #region 드래그 중

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragEnabled && !IsDragging) return;
        // 마우스 위치로 직접 이동
        transform.position = eventData.position;
    }

    #endregion

    #region 드래그 끝

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
        dragItem = null;
        // Raycast 다시 걸리도록 복구
        canvasGroup.blocksRaycasts = true;

        if (!dragEnabled) return;
        //잘못된 드롭일 때 -> canvas에 붙어 있으면 원위치 + 룬 복원
        if (transform.parent == parentCanvas.transform)
        {
            var oldSkillSlot = originalParent.GetComponent<SkillSlotController>();
            if (oldSkillSlot != null)
            {
                oldSkillSlot.ApplyRunes();
            }

            transform.SetParent(originalParent, false);
            var rt = GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            Debug.Log("잘못된 드롭임");
            return;
        }

        // 정상적 드롭 -> 이전 슬롯 초기화
        var prevSlot = originalParent.GetComponent<SkillSlotController>();
        if (prevSlot != null && !isSwapping)
            prevSlot.ClearSlot();
        fromPotionSlot = null; // 마무리 후 초기화

        if (fromPotionSlot != null && transform.parent.GetComponent<PotionSlotController>() != fromPotionSlot)
        {
            EventBus.Publish(new PotionChangedEvent
            {
                slotIndex = fromPotionSlot.slotIndex, potionData = null, currentAmount = 0
            });
        }
    }

    #endregion

    // 외부에서 드래그 허용/금지를 설정할 메서드
    public void SetDragEnabled(bool enabled)
    {
        dragEnabled = enabled;
    }

    //부모가 스킬 슬롯이라면 -> 스킬 이미지로 변환
    public void ToSkillSlot(Sprite skillSprite)
    {
        iconImage.sprite = skillSprite;
        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80f, 80f);
    }

    //다시 인벤토리로 돌아갈 때 원래 아이콘, 크기로 복원
    public void ToInventorySlot()
    {
        if (itemData != null && itemData.icon != null)
            iconImage.sprite = itemData.icon;
        else
            iconImage.sprite = defaultSprite;
        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = defaultSize;
    }

    //부모가 바뀔 때 자동 실행 (스킬북 <-> 스킬 이미지)
    void OnTransformParentChanged()
    {
        // 스킬북 데이터인지 확인
        if (itemData is SkillBookData skillData)
        {
            // 최상위 SkillSlotController를 찾는다
            var inSkillSlot = GetComponentInParent<SkillSlotController>() != null;
            if (inSkillSlot)
                ToSkillSlot(skillData.skillImage);

            else
                ToInventorySlot();
        }
    }
}