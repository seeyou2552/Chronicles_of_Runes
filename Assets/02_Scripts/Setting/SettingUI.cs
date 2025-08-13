using _02_Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour, IUIInterface
{

    public Button closeBtn;
    //세팅 (오디오, 키 커스텀)
    public Button audioSetBtn;
    public Button keySetBtn;
    [Space(5)]
    public GameObject pausePopUp;
    public GameObject setting;
    public GameObject audioSetting;
    public GameObject keySetting;

    private void Awake()
    {
        setting.SetActive(false);
    }
    private void Start()
    {
        audioSetBtn.onClick.AddListener(OnAudioSetting);
        keySetBtn.onClick.AddListener(OnKeySetting);
        closeBtn.onClick.AddListener(CloseUI);
    }

    public void OnSettings()//설정 버튼 클릭 
    {
        setting.SetActive(true);
        audioSetting.SetActive(true);
        keySetting.SetActive(false);
        if(pausePopUp != null)
        {
            pausePopUp.SetActive(false);
        }
        else
        {
            return;
        }
    }
    void OnAudioSetting()
    {
        audioSetting.SetActive(true);
        keySetting.SetActive(false);
    }
    void OnKeySetting()
    {
        keySetting.SetActive(true);
        audioSetting.SetActive(false);
    }
    public void ClosePanel()
    {
        setting.SetActive(false);
    }
    public void OpenUI()
    {
        if (!UIManager.Instance.RegisterUI(this))
            return;
        setting.SetActive(true);
    }
    public void CloseUI()
    {
        setting.SetActive(false);

        UIManager.Instance.UnRegisterUI(this);
    }
}
