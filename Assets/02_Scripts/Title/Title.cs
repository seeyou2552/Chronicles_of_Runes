using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using _02_Scripts.UI;
using _02_Scripts;

public class Title : MonoBehaviour
{
    [Space(5)]
    [Header("오브젝트 할당")]
    public GameObject saveList;
    public Camera titleCamera;

    void Update()
    {
        UpdateCanvasScale();
    }

    void Start()
    {
        UIManager.Instance.baseUI.SetActive(false);
        PlayerController.Instance.gameObject.SetActive(false);
        CameraManager.Instance.gameObject.SetActive(false);
    }

    void UpdateCanvasScale()
    {
        float targetAspect = 1920f / 1080f; // 기준 비율
        float windowAspect = (float)Screen.width / Screen.height;

        if (windowAspect >= targetAspect)
        {
            titleCamera.orthographicSize = 3.5f; // 기준값
        }
        else
        {
            titleCamera.orthographicSize = 3.5f * (targetAspect / windowAspect);
        }
    }


    public void OnExitGame()
    {
#if UNITY_EDITOR
        // 에디터 모드에서 플레이 중지
        EditorApplication.isPlaying = false;
#else
        // 빌드된 게임 종료
        Application.Quit();
#endif
    }
}
