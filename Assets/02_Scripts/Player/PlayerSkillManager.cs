using _02_Scripts.Inventory;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSkillManager : Singleton<PlayerSkillManager>
{
    public Skill[] skills;
    public Dictionary<InputIndex, SkillSlotData> slotSkills = new();
    private PlayerInputActions inputActions => SaveManager.Instance.inputActions;
    [SerializeField] PlayerStatHandler statHandler;
    [SerializeField]
    PlayerAnimationController playerAnimationController;

    private void Awake()
    {
        EventBus.Subscribe(GameState.GameOver, ResetSkillSlot);
        EventBus.Subscribe(GameState.Ending, ResetSkillSlot);
        //inputActions = new PlayerInputActions();
        //DontDestroyOnLoad(gameObject);
        
    }

    private void OnEnable()
    {
        if (inputActions == null) return;
        inputActions.Enable(); // 활성화 필수

        // 키 별 InputAction 연결
        inputActions.Player.Use_Skill_1.performed += ctx => UseSkill(InputIndex.Skill1);
        inputActions.Player.Use_Skill_2.performed += ctx => UseSkill(InputIndex.Skill2);
        inputActions.Player.Use_Skill_3.performed += ctx => UseSkill(InputIndex.Skill3);
        inputActions.Player.Use_Skill_4.performed += ctx => UseSkill(InputIndex.Skill4);
        //inputActions.Player.Use_Skill_RightClick.performed += ctx => UseSkill(InputIndex.RightClick);

        EventBus.Subscribe<SkillChangedEvent>(OnSkillChanged);
    }

    private void OnDisable()
    {
        // 바인딩 해제
        inputActions.Player.Use_Skill_1.performed -= ctx => UseSkill(InputIndex.Skill1);
        inputActions.Player.Use_Skill_2.performed -= ctx => UseSkill(InputIndex.Skill2);
        inputActions.Player.Use_Skill_3.performed -= ctx => UseSkill(InputIndex.Skill3);
        inputActions.Player.Use_Skill_4.performed -= ctx => UseSkill(InputIndex.Skill4);
        //inputActions.Player.Use_Skill_RightClick.performed -= ctx => UseSkill(InputIndex.RightClick);

        inputActions.Disable(); // 비활성화

        EventBus.Unsubscribe<SkillChangedEvent>(OnSkillChanged);
    }

    private void Update()
    {
        foreach (var entry in slotSkills)
        {
            var data = entry.Value;
            if (data.currentCoolTime > 0)
            {
                data.currentCoolTime -= Time.deltaTime;
                if (data.currentCoolTime < 0)
                    data.currentCoolTime = 0;
            }
        }
    }

    public void SetSkillQuickSlot(InputIndex index, Skill skill, List<Rune> runes, float coolTime)
    {
        Skill inputSkill = ScriptableObject.Instantiate(skill);

        var data = new SkillSlotData(inputSkill, runes);
        slotSkills[index] = data;

        inputSkill.ApplyRunes(runes);
        data.currentCoolTime = coolTime;
        data.index = index;
        EventBus.Publish<SkillSlotData>(data);
        foreach (var kvp in slotSkills)
        {
            var slot = kvp.Key;
            var skill1 = kvp.Value.skill;
            var runeList = kvp.Value.runes;

            string runeNames = runeList != null && runeList.Count > 0
                ? string.Join(", ", runeList.ConvertAll(r => r.name))
                : "없음";
        }
        PlayerController.Instance.GetStatModifier().SkillSlotChanged();
    }

    public void OutSkillQuickSlot(InputIndex index)
    {
        slotSkills.Remove(index);
        PlayerController.Instance.GetStatModifier().SkillSlotChanged();
    }

    public void UseSkill(InputIndex index)
    {
        if (!PlayerController.Instance.canControl) return;
        if (slotSkills.TryGetValue(index, out SkillSlotData data))
        {
            if (data.currentCoolTime > 0f)
                return; // 쿨타임 중
            if (!statHandler.UseMana(data.skill.coast))
            {
                StaticNoticeManager.Instance.ShowSideNotice("마나가 부족합니다.",2f);
                return;
            }

            playerAnimationController.HandleAttack(data.skill.SkillAnim);
            data.skill.Use();

            data.currentCoolTime = data.skill.coolTime * (PlayerController.Instance.GetStatModifier().CheckHarmony());
            EventBus.Publish<SkillSlotData>(data);
        }
    }

    private void OnSkillChanged(SkillChangedEvent evt)
    {
        if (evt.skillData == null)
        {
            // 슬롯에서 스킬 제거된 경우
            OutSkillQuickSlot((InputIndex)evt.slotIndex);
            return;
        }
        Skill skill = evt.skillData.skillSO; // SkillBookData 내부 Skill 참조
        List<Rune> runes = new List<Rune>();

        foreach (var runeData in evt.skillItem.attachedRunes)
        {
            if (runeData == null) continue;

            // RuneData → Rune 인스턴스로 변환
            // 
            // Rune rune = ScriptableObject.CreateInstance(runeData.runeSO.GetType()) as Rune;
            Rune rune = Instantiate(runeData.runeSO);
            // runeData에서 파라미터 복사해도 되고 안 해도 됨
            runes.Add(rune);
        }

        SetSkillQuickSlot((InputIndex)evt.slotIndex, skill, runes, evt.currentCoolTime);
    }

    private void ResetSkillSlot(object param)
    {
        slotSkills.Clear();
    }
}