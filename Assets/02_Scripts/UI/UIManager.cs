using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _02_Scripts.UI
{
    
    public class UIOpenedEvent
    {
        public IUIInterface openedUI;
        public UIOpenedEvent(IUIInterface ui) => openedUI = ui;
    }

    public class UIClosedEvent
    {
        public IUIInterface closedUI;
        public UIClosedEvent(IUIInterface ui) => closedUI = ui;
    }
    public class UIManager : Singleton<UIManager>
    {
        public GameObject baseUI;
        [SerializeField] private StoreManager storeManager;
        

        [SerializeField]
        private PauseUI pauseUI;
        [SerializeField]
        private LevelSelectUI levelSelectUI;
        // [SerializeField]
        // private GameOverUI gameOverUI;
        [SerializeField] private MiniMapController miniMapController;
        
        private Stack<IUIInterface> uiStack = new Stack<IUIInterface>();
        public PlayerInputActions inputAction;
        
        private void OnEnable()
        {
            if (inputAction == null)
            {
                inputAction = new PlayerInputActions();
            }
            // inputAction.Player.Enable();
            
        }
        
        private void OnDisable()
        {
            // if (inputAction != null)
            // {
            //     inputAction.Player.Paused.performed -= OnPerformedEsc;
            //     inputAction.Player.Disable();
            // }
        }
        
        public void OnPerformedEsc(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            // 팝업이 떠 있으면 → No 액션 취소
            if (StaticPopupManager.Instance.IsShowing)
            {
                StaticPopupManager.Instance.SelectNo();
                return;
            }

            // 일시정지창이 떠 있을 땐
            if (uiStack.Count == 0 )
            {
                pauseUI.OpenUI();
                return;
            }

            // 일반 UI 닫기
            IUIInterface currentUI = uiStack.Pop();
            currentUI.CloseUI();
            //상점 판매대에 아이템을 올려놓고 esc를 눌러 상점을 닫았을 경우 판매대에 올린 아이템이 다시 인벤토리로 돌아오게
            if (currentUI is StoreManager)
            {
                StoreManager.Instance.CloseUI();

            }
            //if (context.performed)
            //{
            //    int count = uiStack.Count;
            //    if (count == 0)
            //    {
            //        pauseUI.OpenUI();
            //    }
            //    else
            //    {
            //        IUIInterface currentUI = uiStack.Pop();
            //        currentUI.CloseUI();    
            //    }   
            //}
        }
        
        public bool RegisterUI(IUIInterface ui)
        {

            if (!PlayerController.Instance.canUIInteraction) return false; 
            PlayerController.Instance.canControl = false;
            uiStack.Push(ui);
            
            EventBus.Publish(new UIOpenedEvent(ui));
            return true;
        }
        
        public bool UnRegisterUI(IUIInterface ui)
        {
            if (!PlayerController.Instance.canUIInteraction) return false;
            Stack<IUIInterface> tempStack = new Stack<IUIInterface>();

            while (uiStack.Count > 0)
            {
                IUIInterface topUI = uiStack.Pop();
                if (topUI != ui)
                {
                    tempStack.Push(topUI);
                }
            }
            while (tempStack.Count > 0)
            {
                uiStack.Push(tempStack.Pop());
            }
            
            EventBus.Publish(new UIClosedEvent(ui));
            if (uiStack.Count == 0)
            {
                PlayerController.Instance.canControl = true;
            }
            return true;
        }

        public void ClearAllUI()
        {
            while (uiStack.Count > 0)
            {
                var ui = uiStack.Pop();
                EventBus.Publish(new UIClosedEvent(ui));
            }

            // StaticNoticeManager가 있을 때만 Main/Side notice 모두 숨김
            if (StaticNoticeManager.Instance != null)
            {
                StaticNoticeManager.Instance.HideMainNotice();
                StaticNoticeManager.Instance.HideSideNotice();
            }

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.canControl = true;
            }
        }
        public bool IsUIOpen<T>() where T : IUIInterface
        {
            // 스택에 T 타입의 UI 가 하나라도 쌓여 있으면 true
            return uiStack.Any(x => x is T);
        }
    }
}