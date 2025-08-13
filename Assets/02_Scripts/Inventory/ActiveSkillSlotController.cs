using _02_Scripts.Inventory;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ActiveSkillSlotController : MonoBehaviour
{
    //액티브 스킬 슬롯 프리팹에 붙는 스크립트(실제로 사용할 스킬 슬롯)

    [Header("이 액티브 슬롯의 인덱스 (0부터)")]
    public InputIndex slotIndex;

    public Image skillImage;
    public Image skillCoolTimeImage;
    private Color coolTimeColor;
    private Color color;
    public TextMeshProUGUI coolTimeText;
    private float coolTime;
    public TMP_Text bindingText;


    //실질 데이터 보관용 프로퍼티
    public SkillBookData CurrentSkill { get; private set; }
    public int Level { get; private set; }
    public List<RuneData> AttachedRunes { get; private set; } = new List<RuneData>();
    private CancellationTokenSource cts;
    void OnEnable()
    {
        EventBus.Subscribe<KeyBindingChangedEvent>(OnKeyBindingChanged);
        EventBus.Subscribe<SkillChangedEvent>(OnSkillChanged);
        EventBus.Subscribe<SkillSlotData>(param => CoolTimeFillAmount(param).Forget());
        EventBus.Subscribe(GameState.GameOver, Clear);
        EventBus.Subscribe(GameState.MainMenu, Clear);
        EventBus.Subscribe(GameState.Ending, Clear);
        StartCoroutine(RefreshBindingTextNextFrame());//이 자리에 있어야 바로 적용됨
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<KeyBindingChangedEvent>(OnKeyBindingChanged);
        EventBus.Unsubscribe<SkillChangedEvent>(OnSkillChanged);
        EventBus.Unsubscribe<SkillSlotData>(param => CoolTimeFillAmount(param).Forget());
        EventBus.Unsubscribe(GameState.GameOver, Clear);
        EventBus.Unsubscribe(GameState.MainMenu, Clear);
        EventBus.Unsubscribe(GameState.Ending, Clear);
    }

    private void Awake()
    {
        color = skillImage.color;
        coolTimeColor = skillCoolTimeImage.color;
    }

    private void Start()
    {
        RefreshBindingText();
    }

    private void OnSkillChanged(SkillChangedEvent evt)
    {
        if (evt.slotIndex != slotIndex)
        {
            return;
        }

        //UI 갱신
        if (evt.skillData == null)
        {
            // 빈 슬롯 표시
            skillImage.sprite = null;
            skillCoolTimeImage.sprite = null;
            coolTimeColor.a = 0f;
            color.a = 0f;
            cts?.Cancel();
        }
        else
        {
            // 새 스킬 아이콘 표시
            skillImage.sprite = evt.skillData.skillImage;
            skillCoolTimeImage.sprite = evt.skillData.skillImage;

            skillCoolTimeImage.type = Image.Type.Filled;
            skillCoolTimeImage.fillOrigin = (int)Image.Origin360.Top; ;
            skillCoolTimeImage.fillClockwise = false;

            coolTimeColor.a = 0.9f;
            color.a = 1f;
            // coolTime = evt.currentCoolTime;
        }

        skillImage.color = color;
        skillCoolTimeImage.color = coolTimeColor;

    }

    public void RenewSkill()
    {
        foreach (var skillSlot in InventoryManager.Instance.inventoryUIManager.skillSlots)
        {
            if (slotIndex == skillSlot.slotIndex)
            {
                SkillBookData skillData = skillSlot.CurrentSkill;
                skillImage.sprite = skillData != null
                    ? skillData.skillImage
                    : null;

                Color baseColor = skillImage.color;

                baseColor.a = (skillData != null) ? 1f : 0f;
                skillImage.color = baseColor;

                // Color baseCT = skillCoolTimeImage.color;
                // baseCT.a = (skillData != null) ? 0.9f : 0f;
                skillCoolTimeImage.sprite = skillImage.sprite;
                skillCoolTimeImage.type = Image.Type.Filled;
                skillCoolTimeImage.fillOrigin = (int)Image.Origin360.Top; ;
                skillCoolTimeImage.fillClockwise = false;
                coolTimeColor = skillCoolTimeImage.color;
                coolTimeColor.a = 0.9f;
                skillCoolTimeImage.color = coolTimeColor;

            }
        }
    }

    private void OnKeyBindingChanged(KeyBindingChangedEvent evt)
    {
        if (evt.slotIndex != slotIndex) return;
        RefreshBindingText();
    }

    private async UniTask CoolTimeFillAmount(SkillSlotData data)
    {
        if (data.index != slotIndex)
            return;

        cts?.Cancel();
        cts = new CancellationTokenSource();
        var token = cts.Token;
        coolTimeText.text = "";
        Debug.Log("도는중?");
        try
        {
            while (data.currentCoolTime > 0f)
            {
                if (skillCoolTimeImage == null) return;
                skillCoolTimeImage.fillAmount = data.currentCoolTime / data.skill.coolTime;
                if (data.currentCoolTime < 1) coolTimeText.text = data.currentCoolTime.ToString("F1");
                else coolTimeText.text = ((int)data.currentCoolTime).ToString();
                color.a = 0.3f;
                await UniTask.Yield(token);
            }

            coolTimeText.text = "";
            color.a = 1f;

            if (skillCoolTimeImage != null)
                skillCoolTimeImage.fillAmount = 0f;
        }
        catch (OperationCanceledException) // 중단되면 0으로
        {
            skillCoolTimeImage.fillAmount = 0f;
            coolTimeText.text = "";
        }

    }

    //키 리바인딩 하는거 공통 로직
    private void RefreshBindingText()
    {
        string path = SaveManager.Instance.keyBindings[slotIndex];
        bindingText.text = InputBindingUtility.GetInitialForPath(path);
    }
    //이 코루틴 없으면 씬 이동할 때 오류뜸
    private IEnumerator RefreshBindingTextNextFrame()
    {
        yield return null;
        RefreshBindingText();
    }
    public void Clear(object param)
    {
        CurrentSkill = null;
        Level = 0;
        AttachedRunes.Clear();

        if (skillImage != null)
        {
            skillImage.sprite = null;
            var color = skillImage.color;
            color.a = 0f;
            skillImage.color = color;
        }

        if (skillCoolTimeImage != null)
        {
            skillCoolTimeImage.sprite = null;
            skillCoolTimeImage.fillAmount = 0f;
            var ctColor = skillCoolTimeImage.color;
            ctColor.a = 0f;
            skillCoolTimeImage.color = ctColor;
        }

        cts?.Cancel();
        cts = null;
    }
}
