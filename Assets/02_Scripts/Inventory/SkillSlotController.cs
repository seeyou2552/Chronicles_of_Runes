using _02_Scripts.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillChangedEvent
{
    public InputIndex slotIndex;           // 0부터 시작하는 슬롯 번호
    public SkillBookData skillData; // 새로 장착된 스킬 (해제 시 null)
    //public int level;                  // 스킬 레벨
    //public List<RuneData> attachedRunes; // 장착된 룬 목록
    public SkillItemController skillItem;
    public float currentCoolTime; // 현재 쿨타임
}
public class SkillSlotController : MonoBehaviour, IDropHandler
{
    //인벤토리 안에 있는 스킬 슬롯에 붙어있는 스크립트

    [Header("이 슬롯의 인덱스 (0부터)")]
    public InputIndex slotIndex;
    [Header("이 슬롯의 룬 슬롯")]
    public RuneSlotController[] runeSlots;   // 룬 슬롯 5개 연결
    [Header("프레임 이미지")]
    public Image frameImage;

    [Space(10)]
    public GameObject runeItemPrefab;//아이템 데이터가 비어있는 빈 프리팹 할당
    public SkillBookData CurrentSkill { get; private set; } //현재 스킬
    public DraggableItem skillDragItem;
    public SkillItemController skillItemCtrl;

    void Awake()
    {
        EventBus.Subscribe(GameState.GameOver, ResetSkillSlot);
        EventBus.Subscribe(GameState.MainMenu, ResetSkillSlot);
        EventBus.Subscribe(GameState.Ending, ResetSkillSlot);
        // LoadEvent 구독을 Awake에서도 추가
        // EventBus.Subscribe<LoadEvent>(OnLoadEvent);
    }
    
    void OnDestroy()
    {
        EventBus.Unsubscribe(GameState.GameOver, ResetSkillSlot);
        EventBus.Unsubscribe(GameState.Ending, ResetSkillSlot);
    }

    public void Init()
    {
        Logger.Log("LoadState");
        LockAllRunes();
        // Start에서 저장된 데이터가 있는지 확인해서 직접 로드
        CheckAndLoadFromSave();
    }

    #region OnDrop() : 드롭 이벤트(스킬 슬롯에 드롭되는 이벤트를 관리)
    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null || dragged.itemData.itemType != ItemType.SkillBook)
            return;
        //인벤토리 필터가 걸린 상태면 스킬 슬롯으로 이동 불가
        if (InventoryFilter.CurrentFilter != ItemType.None
            && dragged.itemData.itemType != InventoryFilter.CurrentFilter)
            return;
        // 이미 슬롯에 있는 자기 자신을 다시 놓을 경우 합성 불가
        if (skillDragItem == dragged)
            return;

        var newSkill = dragged.itemData as SkillBookData;

        var origin = dragged.originalParent;
        var iconRT = dragged.GetComponent<RectTransform>();
        var dragLv = dragged.GetComponent<SkillItemController>().level;
        //자기 자신 제외 같은 스킬이면 레벨업 -> 룬 슬롯 한 칸 확장
        if (skillDragItem != null && CurrentSkill == newSkill)
        {
            if (skillItemCtrl.level >= 4)
            {
                //이미 만렙이면 스킬북을 드롭해도 레벨이 늘어나지 않음
                var draggedItem = dragged.GetComponent<DraggableItem>();
                dragged.transform.SetParent(draggedItem.parentCanvas.transform, false);
                return;
            }
            else
            {
                //합성을 시도하려는 스킬북이 레벨이 더 높을 때
                if(dragLv > skillItemCtrl.level)
                {
                    //알림창 띄워주기
                    OverwriteAlert.SkillShow(
                        onYes: () =>
                        {
                            if (skillItemCtrl.level < 4)
                            {
                                skillDragItem.GetComponent<SkillItemController>().level++;
                                UpdateRuneSlots();
                                dragged.originalParent
                                    ?.GetComponent<SkillSlotController>()
                                    ?.ClearSlot();
                                Destroy(dragged.gameObject);
                            }
                            else
                            {
                                // 최대 레벨 도달 시엔 그냥 원위치
                                dragged.OnEndDrag(eventData);
                            }
                        },
                        onNo: () => {
                            dragged.transform.SetParent(origin, false);
                            iconRT.anchoredPosition = Vector2.zero;
                        });
                    return;
                }
                skillItemCtrl.level++;
            }

            UpdateRuneSlots();

            //드래그 한 스킬 완전 초기화 -> 액티브 스킬에도 이미지 남지 않음
            var originSlot = dragged.originalParent?.GetComponent<SkillSlotController>();
            if (originSlot != null)
                originSlot.ClearSlot();

            //스킬 북 합성 후 다른 툴팁이 보이지 않는 문제 방지(드래그 상태를 끝냄)
            dragged.OnEndDrag(eventData);

            Destroy(dragged.gameObject);
            return;
        }
        Debug.Log("걸리는지 1");
        //이미 다른 스킬이 있으면 교체 금지
        if (skillDragItem != null)
            return;
        Debug.Log("걸리는지 2");
        //빈 슬롯이면 장착
        skillDragItem = dragged;
        skillItemCtrl = skillDragItem.GetComponent<SkillItemController>();
        CurrentSkill = newSkill;
        // PlayerSkillManager.Instance.slotSkills[slotIndex].currentCoolTime = dragged.currentCoolTime;

        dragged.ToSkillSlot(newSkill.skillImage);
        dragged.transform.SetParent(transform, false);
        var rt = dragged.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        //룬 슬롯 활성화 + 기존 룬 복원
        UpdateRuneSlots();
        ApplyRunes();

        //툴팁 띄우기
        if (RectTransformUtility.RectangleContainsScreenPoint(
        GetComponent<RectTransform>(),
        eventData.position,
        eventData.enterEventCamera))
        {
            // 슬롯 퍼블리셔를 직접 찾아서 OnPointerEnter 흉내
            var publisher = GetComponent<SlotTooltipEventPublisher>();
            publisher.OnPointerEnter(eventData);
        }

        EventBus.Publish(new SkillChangedEvent
        {
            slotIndex = slotIndex,
            skillData = CurrentSkill,
            skillItem = skillItemCtrl,
            currentCoolTime = dragged.currentCoolTime
            
        });
    }
    #endregion

    #region LockAllRunes() : 장착된 룬 잠그기(움직이지 않게)
    private void LockAllRunes()
    {
        foreach (var slot in runeSlots)
            slot.SetInteractable(false);
    }
    #endregion

    #region DatachRunes() : 드래그 시작 시 슬롯에서 룬만 분리
    //룬 UI만 떼어내고, 실제 데이터는 SkillItemController의 List에 남겨두기
    public void DetachRunes()
    {
        if (skillDragItem == null) return;
        skillItemCtrl = skillDragItem.GetComponent<SkillItemController>();
        skillItemCtrl.attachedRunes.Clear();

        for (int i = 0; i < runeSlots.Length; i++)
        {
            var slot = runeSlots[i];
            //슬롯 안에 룬이 한 개 이상이면
            if (slot.transform.childCount > 0)
            {
                //해당 룬 가져오기
                var runeGO = slot.transform.GetChild(0).gameObject;
                //실제 룬 데이터 (RuneData)꺼내기
                var data = runeGO.GetComponent<DraggableItem>().itemData as RuneData;
                //SkillitemController의 리스트에 추가
                skillItemCtrl.attachedRunes.Add(data);
                //룬 슬롯 자식에 있는 해당 룬 아이템은 파괴
                Destroy(runeGO);
            }
            //현재 스킬 아이템 드래그 중이므로 룬 슬롯은 다시 잠금 상태로
            slot.SetInteractable(false);
        }
    }
    #endregion

    #region ClearSlot() : 슬롯 완전 초기화(스킬, 룬 UI 제거)
    public void ClearSlot()
    {
        CurrentSkill = null;
        skillDragItem = null;
        LockAllRunes();

        // 슬롯 내 남은 룬 UI 파괴
        foreach (var slot in runeSlots)
        {
            Debug.Log(slot);
            if (slot.transform.childCount > 0)
                Destroy(slot.transform.GetChild(0).gameObject);
        }
            
        EventBus.Publish(new SkillChangedEvent
        {
            slotIndex = slotIndex,
            skillData = null,
            skillItem = skillItemCtrl,
            currentCoolTime = 0,
        });
    }
    #endregion

    #region ReceiveSkill() : 스왑될 때 데이터, 룬, 레벨 복원, UI갱신 등
    public void ReceiveSkill(DraggableItem item, SkillBookData data, List<RuneData> runes, int level, float coolTime)
    {
        //내부 상태 교체
        skillDragItem = item;
        CurrentSkill = data;
        skillItemCtrl = item.GetComponent<SkillItemController>();
        skillItemCtrl.attachedRunes = runes;

        //Transform 위치 재설정
        item.transform.SetParent(transform, false);
        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        item.ToSkillSlot(data.skillImage);

        skillItemCtrl.level = level;

        //룬 슬롯 UI 갱신
        UpdateRuneSlots();
        ApplyRunes();

        // 액티브 스킬 슬롯으로 이벤트 발행(슬롯이 바뀜을 알려주기)
        EventBus.Publish(new SkillChangedEvent
        {
            slotIndex = this.slotIndex,
            skillData = this.CurrentSkill,
            skillItem = skillItemCtrl,
            currentCoolTime = coolTime
        });
    }
    #endregion

    #region UpdateRuneSlots() : 스킬 레벨에 맞춰 룬 슬롯 활성화
    public void UpdateRuneSlots()
    {
        int level = 0;
        if (skillDragItem != null)
        {
            if (skillItemCtrl.level != null)
                level = skillItemCtrl.level;
        }
        // 레벨 0 → 1칸, 레벨 1 → 2칸, ... 최대 5칸
        int openCount = Mathf.Clamp(level + 1, 0, runeSlots.Length);
        for (int i = 0; i < runeSlots.Length; i++)
            runeSlots[i].SetInteractable(i < openCount);
    }
    #endregion

    #region ApplyRunes() : attachedRunes 리스트에 따라 룬 채우기
    //저장된 룬 데이터를 다시 슬롯 UI에 복원
    public void ApplyRunes()
    {
        if (skillDragItem == null) return;
        skillItemCtrl = skillDragItem.GetComponent<SkillItemController>();

        // 스킬 레벨에 맞춰 슬롯 활성화
        UpdateRuneSlots();

        for (int i = 0; i < skillItemCtrl.attachedRunes.Count && i < runeSlots.Length; i++)
        {
            var slot = runeSlots[i];
            slot.SetInteractable(true);

            var go = Instantiate(runeItemPrefab, slot.transform, false);
            var di = go.GetComponent<DraggableItem>();
            di.Setup(skillItemCtrl.attachedRunes[i]);  // 룬 데이터를 세팅
            di.enabled = false;                        // 장착된 룬은 드래그 금지

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }
    }
    #endregion

    #region Highlight() : 스킬 슬롯 선택 표시(프레임 색 변경)
    public void Highlight(bool select)
    {
        if (select)
        {
            //선택시 흰 프레임 활성화
            frameImage.gameObject.SetActive(true);
        }
        else
        {
            //흰 프레임 비활성화 
            frameImage.gameObject.SetActive(false);
        }
    }
    #endregion

    private void ResetSkillSlot(object param)
    {
        Debug.Log("실행도");
        ClearSlot();
        if(skillItemCtrl != null ) Destroy(skillItemCtrl.gameObject);
    }
    
    // private void OnLoadEvent(LoadEvent loadEvent)
    // {
    //     Debug.Log($"[SkillSlotController] ★★★ OnLoadEvent 호출됨! - 슬롯 인덱스: {slotIndex} ★★★");
    //     Debug.Log($"[SkillSlotController] 받은 스킬 데이터 개수: {loadEvent.saveData.skillSlots.Count}");
    //     // 1) 먼저 슬롯 초기화
    //     ClearSlot();
    //
    //     // 2) 이 슬롯의 저장 정보 찾기
    //     var save = loadEvent.saveData.skillSlots
    //         .FirstOrDefault(s => s.slotIndex == (int)slotIndex);
    //     if (save == null || !save.hasSkill)
    //         return;
    //
    //     // 3) 프리팹 인스턴스화
    //     var go = Instantiate(InventoryManager.Instance.SkillItemPrefab, transform, false);
    //     var draggable = go.GetComponent<DraggableItem>();
    //
    //     // 4) SkillBookData 할당
    //     var skillData = ItemManager.Instance.GetItem(save.skillID);
    //     draggable.itemData = skillData;
    //
    //     // 5) SkillItemController 복원
    //     var ctrl = draggable.GetComponent<SkillItemController>();
    //     ctrl.level = save.skillLevel;
    //
    //     // 6) 룬 데이터 복원 리스트 생성
    //     var runeList = new List<RuneData>();
    //     foreach (var runeId in save.runeIDs)
    //     {
    //         var rd = ItemManager.Instance.GetItem(runeId);
    //         if (rd != null) runeList.Add(rd as RuneData);
    //     }
    //     draggable.Setup(skillData);
    //     // 7) ReceiveSkill으로 내부 상태·UI 일괄 복원
    //     ReceiveSkill(draggable, skillData as SkillBookData, runeList, save.skillLevel);
    // }
    
    private void CheckAndLoadFromSave()
    {
        // SaveManager에서 현재 선택된 슬롯 데이터 가져오기
        if (SaveManager.Instance.SelectedIndex < 0) 
        {
            return;
        }
    
        var slotData = SaveManager.Instance.GetSlotData(SaveManager.Instance.SelectedIndex);
        if (slotData == null)
        {
            return;
        }
    
    
        // 이 슬롯에 해당하는 스킬 찾기
        var saveSkill = slotData.skillSlots.FirstOrDefault(s => s.slotIndex == (int)slotIndex);
        if (saveSkill == null || !saveSkill.hasSkill)
        {
            return;
        }
    
    
        // 스킬 로드 실행
        LoadSkillFromSave(saveSkill);
    }
    
    private void LoadSkillFromSave(SkillSlotSave saveSkill)
    {
        try
        {
            // 1) 프리팹 인스턴스화
            if (InventoryManager.Instance?.SkillItemPrefab == null)
            {
                return;
            }
        
            var go = Instantiate(InventoryManager.Instance.SkillItemPrefab, transform, false);
            var draggable = go.GetComponent<DraggableItem>();

            // 2) SkillBookData 할당
            var skillData = ItemManager.Instance.GetItem(saveSkill.skillID) as SkillBookData;
            if (skillData == null)
            {
                Destroy(go);
                return;
            }
        
            draggable.itemData = skillData;

            // 3) SkillItemController 복원
            var ctrl = draggable.GetComponent<SkillItemController>();
            if (ctrl == null)
            {
                Destroy(go);
                return;
            }
            ctrl.level = saveSkill.skillLevel;

            // 4) 룬 데이터 복원 리스트 생성
            var runeList = new List<RuneData>();
            foreach (var runeId in saveSkill.runeIDs)
            {
                var rd = ItemManager.Instance.GetItem(runeId) as RuneData;
                if (rd != null) runeList.Add(rd);
            }
        
            // 5) DraggableItem 셋업
            draggable.Setup(skillData);
        
        
            // 6) ReceiveSkill으로 내부 상태·UI 일괄 복원
            ReceiveSkill(draggable, skillData, runeList, saveSkill.skillLevel, 0f);
            
            foreach (var skillSlot in InventoryManager.Instance.activeSkillSlots)
            {
                skillSlot.RenewSkill();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillSlotController] 스킬 로드 중 오류: {e.Message}");
        }
    }

}