using _02_Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class LevelSelectUI : MonoBehaviour, IUIInterface
{
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private Image easyBtn;
    [SerializeField] private Image normalBtn;
    [SerializeField] private Image hardBtn;
    [SerializeField] private Sprite selectSprite;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private TextMeshProUGUI levelText;

    void BtnSelectEffect()
    {
        switch (GameManager.Instance.dungeonLevel)
        {
            case DungeonLevel.Easy:
                easyBtn.sprite = selectSprite;
                break;
            case DungeonLevel.Normal:
                normalBtn.sprite = selectSprite;
                break;
            case DungeonLevel.Hard:
                hardBtn.sprite = selectSprite;
                break;
        }
    }

    public void OnOffPanel()
    {
        if (levelPanel.activeSelf)
        {
            StaticNoticeManager.Instance.ShowMainNotice();
            CloseUI();
        }
        else
        {
            StaticNoticeManager.Instance.HideMainNotice();
            OpenUI();
        }
    }

    public void CloseUI()
    {
        if (!UIManager.Instance.UnRegisterUI(this))
        {
            return;
        }
        levelPanel.SetActive(false);

    }

    public void OpenUI()
    {
        if (!UIManager.Instance.RegisterUI(this))
        {
            return;
        }
        levelPanel.SetActive(true);
        BtnSelectEffect();
    }

    public void ChangeToEasy()
    {
        GameManager.Instance.ChangeDungeonLevel(DungeonLevel.Easy);
        easyBtn.sprite = selectSprite;
        normalBtn.sprite = normalSprite;
        hardBtn.sprite = normalSprite;
        levelText.text = "적들의 힘이 약화되어 한결 여유롭습니다.\r\n공격의 위협이 낮아 초보자에게 추천합니다.\r\n";
    }

    public void ChangeToNormal()
    {
        GameManager.Instance.ChangeDungeonLevel(DungeonLevel.Normal);
        easyBtn.sprite = normalSprite;
        normalBtn.sprite = selectSprite;
        hardBtn.sprite = normalSprite;
        levelText.text = "적들의 전투력이 적절히 강화되어 긴장함 있는 전투를 즐길 수 있습니다.";
    }
    public void ChangeToHard()
    {
        GameManager.Instance.ChangeDungeonLevel(DungeonLevel.Hard);
        easyBtn.sprite = normalSprite;
        normalBtn.sprite = normalSprite;
        hardBtn.sprite = selectSprite;
        levelText.text = "적들이 매우 강해집니다. 공격 한 방 한 방이 치명적입니다. 완벽한 컨트롤과 신속한 회피가 요구됩니다. \n진정한 계승자의 모습을 보여주세요.";

    }

}
