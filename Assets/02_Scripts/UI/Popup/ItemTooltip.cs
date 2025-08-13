using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipShowEvent
{
    public ItemData data;
    public AchievementData achievData;
    public PassiveBase passive;
    public Vector2 screenPosition;
    public List<RuneData> attachedRunes;
    public bool isCollectorSlot;
}
public class TooltipHideEvent { }
public class ItemTooltip : MonoBehaviour
{
    //아이템 정보를 보여줄 툴 팁 스크립트(액티브 스킬 슬롯X)

    public GameObject tooltipWindow; 
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI coolTimeText;
    public TextMeshProUGUI runesText;
    [Header("툴팁 최대 너비 (px)")]
    public float maxWidth = 800f;

    private RectTransform rt;
    private LayoutElement layout;

    void Awake()
    {
        rt = tooltipWindow.GetComponent<RectTransform>();
        layout = tooltipWindow.GetComponent<LayoutElement>();
        tooltipWindow.SetActive(false);
    }

    void OnEnable()
    {
        EventBus.Subscribe<TooltipShowEvent>(OnShow);
        EventBus.Subscribe<TooltipHideEvent>(OnHide);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<TooltipShowEvent>(OnShow);
        EventBus.Unsubscribe<TooltipHideEvent>(OnHide);
    }

    private void OnShow(TooltipShowEvent evt)
    {
        if (evt.data != null)  // 데이터 채우기
        {
            nameText.text = evt.data.displayName;
            descText.text = evt.data.description;
        }
        else if (evt.achievData != null)
        {
            nameText.text = evt.achievData.displayName;
            descText.text = evt.achievData.description;
        }
        else if (evt.passive != null)
        {
            nameText.text = evt.passive.Name;
            descText.text = evt.passive.Description;
        }

        if (evt.data is SkillBookData skillData) // 스킬 데미지, 마나 출력
        {
            //SkillBookData skillData = evt.data as SkillBookData;
            damageText.text = "Damage   : " + skillData.skillSO.damage.ToString();
            manaText.text = "Mana     : " + skillData.skillSO.coast.ToString();
            coolTimeText.text = "CoolTime : " + skillData.skillSO.coolTime.ToString();
            damageText.gameObject.SetActive(true);
            manaText.gameObject.SetActive(true);
            coolTimeText.gameObject.SetActive(true);
        }
        else
        {
            damageText.gameObject.SetActive(false);
            manaText.gameObject.SetActive(false);
            coolTimeText.gameObject.SetActive(false);
        }

        //스킬에 룬 목록이 추가되었을 경우
        if (evt.attachedRunes != null && evt.attachedRunes.Count > 0)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("--------------");
            sb.AppendLine("장착한 룬");
            foreach (var rune in evt.attachedRunes)
            {
                sb.AppendLine($"\n{rune.displayName} : {rune.description}");
            }
            runesText.text = sb.ToString();
            runesText.gameObject.SetActive(true);
        }
        else
        {
            runesText.gameObject.SetActive(false);
        }

        //툴팁 활성화, 레이아웃 강제 갱신
        tooltipWindow.SetActive(true);
        Canvas.ForceUpdateCanvases();

        // 현재 컨텐트 기준 너비 측정
        float contentWidth = rt.rect.width;

        //preferredWidth에 clamp 적용
        layout.preferredWidth = Mathf.Min(contentWidth, maxWidth);
        Canvas.ForceUpdateCanvases();

        if (evt.isCollectorSlot)
        {
            rt.pivot = new Vector2(0.5f, 1f);
            rt.position = evt.screenPosition;
            return;  // 도감 전용 처리 끝
        }

        //pivot.x, pos 조정
        float halfW = Screen.width * 0.5f;
        float pivotX = evt.screenPosition.x > halfW ? 1f : 0f;
        rt.pivot = new Vector2(pivotX, 0.5f);

        //화면 상단 overflow 방지 로직
        Vector2 pos = evt.screenPosition;

        //툴팁 높이
        float tooltipH = rt.rect.height;
        // pivot.y = 0.5 이므로 상단/하단 엣지 계산
        float topEdge = pos.y + tooltipH * 0.5f;
        float bottomEdge = pos.y - tooltipH * 0.5f;

        //위쪽으로 넘친 만큼 내리기
        float overflowTop = topEdge - Screen.height;
        if (overflowTop > 0f)
            pos.y -= overflowTop;

        //아래쪽으로 넘친 만큼 올리기
        float overflowBot = 0f - bottomEdge;  // bottomEdge<0 이면 음수
        if (overflowBot > 0f)
            pos.y += overflowBot;

        rt.position = pos;
    }

    private void OnHide(TooltipHideEvent e)
    {
        tooltipWindow.SetActive(false);
        layout.preferredWidth = -1;
    }
}
