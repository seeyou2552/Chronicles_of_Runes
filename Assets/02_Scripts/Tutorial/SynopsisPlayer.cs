using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using _02_Scripts.UI;

public class SynopsisPlayer : MonoBehaviour
{
    public TextMeshProUGUI dialogueText; // UI 텍스트
    public string[] currentDialogues; // 대사 배열
    public Image skipProgressBar; // 스킵 Image
    public GameObject skipText;
    public float skipSpeed = 0.5f;
    public float skipAmount = 0f;
    public float typingSpeed = 0.05f;
    public TimelineController timelineController;
    public SynopsisConst synopsisConst;
    public Action<float> skipDelayAction;


    /* Dialogue 진행에 사용되는 변수 */
    private int index = 0;
    private int skipIndex = 0; // 스킵 시 넘어갈 TimeLine Index
    private bool isTyping = false; // 대사 배열이 타이핑 중일 때
    private bool canSkip = false; // 대사 배열이 타이핑 중인 상태에서 한 번더 입력 시 타이핑 효과 스킵
    private bool isHolding = false; // 스킵 홀딩 여부
    private bool synopsisEnd = false;

    private void Awake() // 초기값 세팅
    {
        Cursor.Instance.gameObject.SetActive(false);
        currentDialogues = synopsisConst.synopsisDialogues;
        skipDelayAction += UpdateSkipUI;
        UpdateSkipUI(skipAmount);
    }

    void Start() // 초기값 세팅 후 currentDialogues에 있는 string을 출력
    {
        StartCoroutine(TypeLine());
    }


    void Update()
    {
        if (Input.anyKeyDown && isTyping) // 대사가 타이핑 중인 경우 스킵
        {
            canSkip = true;
            return;
        }
        if (Input.anyKeyDown && !isTyping) // 대사가 타이핑 중이 아닐 때(출력이 끝났을 때)
        {
            if (synopsisEnd) return;
            
            if (index < currentDialogues.Length)
            {
                StartCoroutine(TypeLine()); // 만약 다음 다이얼로그가 남아있으면 출력
            }
            else
            {
                dialogueText.text = "";
                synopsisEnd = true;
                skipText.SetActive(false);
                timelineController.PlayTimeline();
            }
        }
        if (isHolding)
        {
            skipAmount += Time.deltaTime * skipSpeed;
            skipDelayAction?.Invoke(skipAmount);
            if (skipAmount > 1f)
            {
                

                dialogueText.gameObject.SetActive(false);
                index = currentDialogues.Length;
                synopsisEnd = true;
                skipProgressBar.gameObject.SetActive(false);
                skipText.SetActive(false);

                timelineController.SkipToPoint(timelineController.timelineSkipPoint.Count - 1);
                timelineController.PlayTimeline();
            }
        }
    }

    IEnumerator TypeLine() // 대사 진행
    {
        timelineController.PlayTimeline();
        isTyping = true;
        dialogueText.text = string.Empty;
        canSkip = false;
        foreach (char letter in currentDialogues[index].ToCharArray())
        {
            if (canSkip)
            {
                dialogueText.text = currentDialogues[index];
                timelineController.SkipToPoint(skipIndex);
                break;
            }
            dialogueText.text += letter;
            yield return YieldCache.WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        canSkip = false;
        index++;
        skipIndex++;
    }

    public void SynopsisSkip(InputAction.CallbackContext context)
    {
        // if (synopsisEnd) return; // 시놉시스 끝나면 리턴

        if (context.performed)
        {
            if (synopsisEnd) return;
            isHolding = true;
        }

        if (context.canceled)
        {
            skipAmount = 0f;
            isHolding = false;
        }
        skipDelayAction?.Invoke(skipAmount);
    }

     public void UpdateSkipUI(float amount)
    {
        skipProgressBar.fillAmount = amount;

        if (amount > 1f)
        {
            amount = 1f;
        }

    }

    

    public void EndSynopsis()
    {
        timelineController.StopTimeline();
        Cursor.Instance.gameObject.SetActive(true);
        GameManager.Instance.ChangeState(GameState.Tutorial);
    }

}
