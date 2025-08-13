using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapSelectionManager : MonoBehaviour
{
    //스킬 슬롯 선택을 담당하는 매니저 스크립트. 싱글톤
    public static SwapSelectionManager Instance { get; private set; }

    private SkillSlotController firstSelected;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public void OnSlotClicked(SkillSlotController clicked)
    {
        //첫 클릭 -> 슬롯 선택
        if (firstSelected == null)
        {
            //스킬이 없는 슬롯을 클릭하면 무시(빈 슬롯)
            if (clicked.skillDragItem == null)
                return;
            //스킬이 선택되면
            firstSelected = clicked;
            clicked.Highlight(true);//색 변화
            return;
        }

        // 두 번째 클릭
        if (clicked == firstSelected //자기 자신을 클릭했거나
            || firstSelected.skillDragItem == null //첫 번째 슬롯이 빈 상태가 되었거나
            || clicked.skillDragItem == null)//두 번째 클릭한 슬롯이 빈 상태라면
        {
            //선택 취소
            firstSelected.Highlight(false);
            firstSelected = null;
            return;
        }

        // 서로 다른 슬롯 클릭 → 스왑
        PerformSwap(firstSelected, clicked);
        firstSelected.Highlight(false);
        firstSelected = null;
    }
    public void ClearSelection()
    {
        if (firstSelected != null)
        {
            firstSelected.Highlight(false);
            firstSelected = null;
        }
    }
    void PerformSwap(SkillSlotController A, SkillSlotController B)
    {
        //상태 백업
        //첫 선택 
        var a_item  = A.skillDragItem;
        var a_data  = A.CurrentSkill;
        var a_ctrl  = a_item.GetComponent<SkillItemController>();
        var a_runes = new List<RuneData>(a_ctrl.attachedRunes);
        var a_lvl   = a_ctrl.level;
        var a_coolTime = PlayerSkillManager.Instance.slotSkills[A.slotIndex].currentCoolTime;
        //두번째로 클릭한 슬롯
        var b_item  = B.skillDragItem;
        var b_data  = B.CurrentSkill;
        var b_ctrl  = b_item.GetComponent<SkillItemController>();
        var b_runes = new List<RuneData>(b_ctrl.attachedRunes);
        var b_lvl   = b_ctrl.level;
        var b_coolTime = PlayerSkillManager.Instance.slotSkills[B.slotIndex].currentCoolTime;

        //슬롯 내부 초기화
        A.ClearSlot();
        B.ClearSlot();

        //부모(슬롯) 교체
        a_item.transform.SetParent(B.transform, false);
        b_item.transform.SetParent(A.transform, false);

        //데이터·룬·레벨 복원 & UI 갱신
        A.ReceiveSkill(b_item, b_data, b_runes, b_lvl, b_coolTime);
        B.ReceiveSkill(a_item, a_data, a_runes, a_lvl, a_coolTime);
    }
}