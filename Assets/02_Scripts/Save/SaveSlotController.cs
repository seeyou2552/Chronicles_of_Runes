using _02_Scripts.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotController : MonoBehaviour, IUIInterface
{
    [Header("저장 슬롯 인덱스 (0부터)")]
    public int slotIndex;

    [Header("첫 생성용 + 버튼")]
    public Button createButton;
    [Header("로드용 슬롯 버튼")]
    public Button slotButton;
    [Header("삭제용 X 버튼")]
    public Button deleteButton;

    [Header("UI 텍스트")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI saveTimeText;
    public TextMeshProUGUI elapsedTimeText;
    public TextMeshProUGUI highScoreText;
    
    [SerializeField] private TitleUIManager saveUIManager;

    void Awake()
    {
        createButton.onClick.AddListener(OnClickCreate);
        slotButton.onClick.AddListener(OnClickLoad);
        deleteButton.onClick.AddListener(OnClickDelete);
    }
    public void Refresh()
    {
        var data = SaveManager.Instance.slots[slotIndex];

        // data가 null이거나, playerName이 비어 있으면 빈 슬롯으로 보기
        bool isEmpty = data == null || string.IsNullOrEmpty(data.playerName);

        createButton.gameObject.SetActive(isEmpty);
        slotButton.gameObject.SetActive(!isEmpty);
        deleteButton.gameObject.SetActive(!isEmpty);

        nameText.gameObject.SetActive(!isEmpty);
        saveTimeText.gameObject.SetActive(!isEmpty);
        elapsedTimeText.gameObject.SetActive(!isEmpty);
        highScoreText.gameObject.SetActive(!isEmpty);

        if (!isEmpty)
        {
            nameText.text = data.playerName;
            saveTimeText.text = data.saveTime;
            elapsedTimeText.text = data.elapsedTime;
            highScoreText.text = "최고 점수 : " + data.highScore.ToString();
        }
    }

    // 생성 버튼 눌렀을 때
    private void OnClickCreate()
    {
        saveUIManager.OpenNameInput(slotIndex);
    }

    // 로드 버튼 눌렀을 때
    private void OnClickLoad()
    {
        Logger.Log(slotIndex.ToString());
        SaveManager.Instance.SelectSlot(slotIndex);
        SaveManager.Instance.LoadSelectedSlotData();
    }

    // 삭제 버튼 눌렀을 때
    private void OnClickDelete()
    {
        SaveManager.Instance.DeleteSlot(slotIndex);
        Refresh();
    }

    public void CloseUI()
    {
        OnClickDelete();
    }
}