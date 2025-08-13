using _02_Scripts.UI;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Image = UnityEngine.UI.Image;

namespace _02_Scripts.NPC
{
    public enum DialogueType
    {
        End,
        Text
    }
    
    public struct DialogueResult
    {
        public DialogueType type;
        public int seq;

        public DialogueResult(DialogueType type, int seq)
        {
            this.type = type;
            this.seq = seq;
        }
    }
    
    public class DialogueManager : Singleton<DialogueManager>
    {
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private GameObject LeftProfile;
        [SerializeField] private Image LeftProfileImage;
        [SerializeField] private TMP_Text LeftProfileName;
        [SerializeField] private GameObject RightProfile;
        [SerializeField] private Image RightProfileImage;
        [SerializeField] private TMP_Text RightProfileName;
        [SerializeField] private TMP_Text TextBox;
        // [SerializeField] private Image SelectBtnImage;
        // [SerializeField] private Image UnSelectBtnImage;
        [SerializeField] private GameObject BtnPrefab;
        [SerializeField] private GameObject BtnBox;
        PlayerInputActions inputAction;
        DialogueData dialogueData;
        private int currentSeq;
        private int currentBtnIndex;
        private Sprite SelectSprite;
        private Sprite UnSelectSprite;
        private bool isTyping = false;
        private bool isWaitingForInput = false;
        private bool isChoosing = false;
        private DialogueUIAdapter uiAdapter;

        private Coroutine typingCoroutine;
        
        private bool active;
        private TaskCompletionSource<int> dialogueCompletionSource;
        
        private void OnEnable()
        {
            
            if (inputAction == null)
                inputAction = new PlayerInputActions();

            inputAction.Dialogue.Enable();
            inputAction.Dialogue.Move.performed += OnMove;
            inputAction.Dialogue.Submit.performed += OnSubmit;
        }

        private void OnDisable()
        {
            inputAction.Dialogue.Move.performed -= OnMove;
            inputAction.Dialogue.Submit.performed -= OnSubmit;
            inputAction.Dialogue.Disable();
        }

        private void Awake()
        {
            active = false;
            uiAdapter = new DialogueUIAdapter(this);
            SelectSprite = Resources.Load<Sprite>("Dialogue/Btn/SelectedButton");
            UnSelectSprite = Resources.Load<Sprite>("Dialogue/Btn/UnSelectedButton");
        }
        
        
        public async Task<int> StartDialogue(string dialogue)
        {
            Logger.Log(dialogue);
            active = true;
            PlayerController.Instance.canControl = false;
            if (!UIManager.Instance.RegisterUI(uiAdapter))
            {
                return -1;
            }
            dialogueData = LoadDialogue(dialogue);
            currentBtnIndex = 0;
            currentSeq = 0;
            SetupProfiles();

            dialogueCompletionSource = new TaskCompletionSource<int>();

            SetDialogue(currentSeq);

            // 대화 종료까지 기다림
            return await dialogueCompletionSource.Task;
        }

        public DialogueResult SetDialogue(int seq)
        {
            // 버튼 제거 및 비활성화
            ClearButtons();
            BtnBox.SetActive(false);
            TextBox.gameObject.SetActive(true); // 텍스트 다시 표시

            // 프로필 설정
            DialogueLine line = dialogueData.Dialogues.FirstOrDefault(d => d.seq == seq);
            if (line == null)
            {
                EndDialogue(seq);
                return new DialogueResult(DialogueType.End, seq);
            }

            // 프로필 강조
            if (line.speaker == "Left")
            {
                LeftProfileImage.color = new Color(1, 1, 1, 1);
                RightProfileImage.color = new Color(1, 1, 1, 0.05f);
            }
            else
            {
                LeftProfileImage.color = new Color(1, 1, 1, 0.05f);
                RightProfileImage.color = new Color(1, 1, 1, 1);
            }

            // 텍스트 타이핑 시작
            typingCoroutine = StartCoroutine(TypeTextCoroutine(line.text));

            return new DialogueResult(DialogueType.Text, seq);
        }

        
        private IEnumerator TypeTextCoroutine(string fullText)
        {
            isTyping = true;
            float delay = 0.03f;

            for (int i = 0; i < fullText.Length; i++)
            {
                TextBox.text += fullText[i];
                yield return YieldCache.WaitForSeconds(delay);
            }

            isTyping = false;
            isWaitingForInput = true;

            yield return null;
        }


        
        private void SetupProfiles()
        {
            LeftProfileImage.sprite = Resources.Load<Sprite>("Dialogue/Profile/" + dialogueData.Left.profile);
            RightProfileImage.sprite = Resources.Load<Sprite>("Dialogue/Profile/" + dialogueData.Right.profile);
            LeftProfileName.text = dialogueData.Left.name;
            RightProfileName.text = dialogueData.Right.name;
            dialogueBox.SetActive(true);
        }
        
        public DialogueData LoadDialogue(string dialogue)
        {
            
            TextAsset jsonFile = Resources.Load<TextAsset>("Dialogue/"+dialogue);
            if (jsonFile == null)
            {
                Logger.LogError($"파일을 찾을 수 없습니다: {dialogue}");
                return null;
            }

            return JsonUtility.FromJson<DialogueData>(jsonFile.text);
        }
        
        private void OnMove(InputAction.CallbackContext context)
        {
            if (!active || !isChoosing) return;

            Vector2 input = context.ReadValue<Vector2>();

            int maxIndex = dialogueData.Dialogues
                .First(d => d.seq == currentSeq)
                .btn.Length - 1;

            if (input.y > 0.1f) // 위
            {
                currentBtnIndex = Mathf.Max(0, currentBtnIndex - 1);
                UpdateButtonSelection();
            }
            else if (input.y < -0.1f) // 아래
            {
                currentBtnIndex = Mathf.Min(maxIndex, currentBtnIndex + 1);
                UpdateButtonSelection();
            }
        }
        
        private void UpdateButtonSelection()
        {
            for (int i = 0; i < BtnBox.transform.childCount; i++)
            {
                var img = BtnBox.transform.GetChild(i).GetComponent<Image>();
                img.sprite = (i == currentBtnIndex) ? SelectSprite : UnSelectSprite;
            }
        }


        
        private void OnSubmit(InputAction.CallbackContext context)
        {
            if (!active) return;

            if (isTyping)
            {
                // 타이핑 중 Enter 누르면 전체 출력
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);
                TextBox.text = dialogueData.Dialogues.First(d => d.seq == currentSeq).text;
                isTyping = false;
                isWaitingForInput = true;
                return;
            }

            if (isWaitingForInput)
            {
                isWaitingForInput = false;
                ShowButtonsForCurrentLine(); // 버튼 보여주기만
                return;
            }

            if (isChoosing)
            {
                var currentLine = dialogueData.Dialogues.FirstOrDefault(d => d.seq == currentSeq);
                if (currentLine == null) return;
                if (currentBtnIndex >= currentLine.btn.Length) return;
                int nextSeq = currentLine.btn[currentBtnIndex].nextSeq;

                // 상태 초기화
                isChoosing = false;
                BtnBox.SetActive(false);
                TextBox.text = "";

                currentSeq = nextSeq;
                SetDialogue(currentSeq); // 이제 다음 대사 출력
            }
        }


        private void ShowButtonsForCurrentLine()
        {
            isChoosing = true;
            TextBox.gameObject.SetActive(false); // 텍스트 숨기기
            BtnBox.SetActive(true);
            ClearButtons(); // 기존 버튼 제거

            DialogueLine line = dialogueData.Dialogues.First(d => d.seq == currentSeq);
            for (int j = 0; j < line.btn.Length; j++)
            {
                GameObject btn = Instantiate(BtnPrefab, BtnBox.transform);
                RectTransform rt = btn.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                btn.GetComponent<Image>().sprite = (j == currentBtnIndex) ? SelectSprite : UnSelectSprite;
                btn.GetComponentInChildren<TMP_Text>().text = line.btn[j].text;
            }
        }
        
        private void ClearButtons()
        {
            foreach (Transform child in BtnBox.transform)
            {
                Destroy(child.gameObject);
            }
        }


        
        private void EndDialogue(int seq)
        {
            if (!UIManager.Instance.UnRegisterUI(uiAdapter))
            {
                return;
            }
            active = false;
            dialogueBox.SetActive(false);
            PlayerController.Instance.canControl = true;

            dialogueCompletionSource?.TrySetResult(seq);
        }
        
        public class DialogueUIAdapter : IUIInterface
        {
            private DialogueManager manager;

            public DialogueUIAdapter(DialogueManager mgr)
            {
                this.manager = mgr;
            }

            public void CloseUI()
            {
                manager.ForceCloseDialogue(); // 강제로 종료
            }
        }
        
        public void ForceCloseDialogue()
        {
            if (!UIManager.Instance.UnRegisterUI(uiAdapter))
            {
                return;
            }
            if (!active) return;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            active = false;
            isTyping = false;
            isChoosing = false;
            isWaitingForInput = false;

            dialogueBox.SetActive(false);
            ClearButtons();
            TextBox.text = "";

            PlayerController.Instance.canControl = true;

            dialogueCompletionSource?.TrySetResult(currentSeq);
        }
    }
}