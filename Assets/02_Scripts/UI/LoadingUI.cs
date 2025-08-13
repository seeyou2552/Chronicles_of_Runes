using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;
using _02_Scripts.UI;
using UnityEngine.EventSystems;

public class LoadingUI : MonoBehaviour
{

    [SerializeField] private Image loadingPanel;
    [SerializeField] public Image loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private LoadingConst loadingConst;
    private float fadeDuration = 0.7f;

    void OnEnable()
    {
        EventBus.Subscribe(LoadState.LoadStart, param => OnLoadStart(param).Forget());
        EventBus.Subscribe(LoadState.LoadComplete, param => OnLoadComplete(param).Forget());
    }

    async UniTask OnLoadStart(object param)
    {
        if (DraggableItem.IsDragging) ExecuteEvents.Execute<IEndDragHandler>(DraggableItem.dragItem.pointerDrag, DraggableItem.dragItem, ExecuteEvents.endDragHandler);
        // PlayerController.Instance.canControl = false;
        PlayerController.Instance.gameObject.SetActive(false);
        UIManager.Instance.baseUI.SetActive(false);
        if (loadingPanel != null) loadingPanel.gameObject.SetActive(true);
        loadingPanel.DOFade(1f, fadeDuration).OnComplete(
            () =>
            {
                PlayerController.Instance.transform.position = new Vector2(900, 900);
            });
        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(true);
            loadingBar.fillAmount = 0f;
            RandomConst();
        }
    }

    async UniTask OnLoadComplete(object param)
    {
        // PlayerController.Instance.canControl = true;
        if (GameManager.Instance.state != GameState.MainMenu
            && GameManager.Instance.state != GameState.Ending
            && GameManager.Instance.state != GameState.Synopsis) // 일부 state의 경우 ui 출력 안함
        {
            PlayerController.Instance.gameObject.SetActive(true);
            UIManager.Instance.baseUI.SetActive(true);
        }
        
        
        if (loadingBar != null) loadingBar.gameObject.SetActive(false);
        
        if (loadingText != null) loadingText.gameObject.SetActive(false);
        EventBus.Publish("DungeonStart", null);
        await loadingPanel.DOFade(0f, fadeDuration).AsyncWaitForCompletion();
        
        if (loadingPanel != null) loadingPanel.gameObject.SetActive(false);
        
    }

    private void RandomConst()
    {
        loadingText.gameObject.SetActive(true);
        int index = Random.Range(0, loadingConst.loadingDialogues.Length - 1);
        loadingText.text = loadingConst.loadingDialogues[index];
    }
}
