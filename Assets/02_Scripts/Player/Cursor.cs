using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Cursor : Singleton<Cursor>
{
    //모든 씬에서 사용하기 위해 싱글톤으로 작성
    public Image cursor;
    private RectTransform rt;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        rt = cursor.rectTransform;
        UnityEngine.Cursor.visible = false;
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드될 때마다 명시적으로 커서 숨김
        UnityEngine.Cursor.visible = false;
    }

    void Update()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        rt.position = screenPos;
    }
}