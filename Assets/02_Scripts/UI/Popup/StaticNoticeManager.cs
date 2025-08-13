using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaticNoticeManager : Singleton<StaticNoticeManager>
{
    [SerializeField] private GameObject MainNotice;
    [SerializeField] private GameObject SideNotice;
    [SerializeField] private TMP_Text MainNoticeText;
    [SerializeField] private TMP_Text SideNoticeText;
    private CanvasGroup sideNoticeCanvasGroup;
    private CanvasGroup mainNoticeCanvasGroup;

    private Coroutine mainNoticeCoroutine;
    private Coroutine sideNoticeCoroutine;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (SideNotice != null)
            sideNoticeCanvasGroup = SideNotice.GetComponent<CanvasGroup>();

        if (MainNotice != null)
            mainNoticeCanvasGroup = MainNotice.GetComponent<CanvasGroup>();
    }

    public void ShowMainNotice()
    {
        mainNoticeCoroutine = null;
        MainNotice.SetActive(true);
    }

    public void ShowMainNotice(string message, float noticeTime = -1f)
    {
        MainNoticeText.text = message;
        MainNotice.SetActive(true);

        if (mainNoticeCanvasGroup != null)
            mainNoticeCanvasGroup.alpha = 1f;

        if (mainNoticeCoroutine != null)
            StopCoroutine(mainNoticeCoroutine);

        if (noticeTime > 0)
            mainNoticeCoroutine = StartCoroutine(FadeOutMainNoticeAfterDelay(noticeTime));
    }

    public void ShowSideNotice(string message, float noticeTime = -1f)
    {
        SideNoticeText.text = message;
        SideNotice.SetActive(true);

        if (sideNoticeCanvasGroup != null)
            sideNoticeCanvasGroup.alpha = 1f;

        if (sideNoticeCoroutine != null)
        {
            StopCoroutine(sideNoticeCoroutine);
        }

        if (noticeTime > 0)
        {
            sideNoticeCoroutine = StartCoroutine(FadeOutSideNoticeAfterDelay(noticeTime));
        }
    }
    
    public void HideMainNotice()
    {
        if (mainNoticeCoroutine != null)
        {
            StopCoroutine(mainNoticeCoroutine);
            mainNoticeCoroutine = null;
        }

        if (mainNoticeCanvasGroup != null)
            mainNoticeCanvasGroup.alpha = 0f;

        MainNotice.SetActive(false);
    }

    public void HideSideNotice()
    {
        if (sideNoticeCoroutine != null)
        {
            StopCoroutine(sideNoticeCoroutine);
            sideNoticeCoroutine = null;
        }

        if (sideNoticeCanvasGroup != null)
            sideNoticeCanvasGroup.alpha = 0f;

        SideNotice.SetActive(false);
    }
    
    private IEnumerator FadeOutSideNoticeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        float fadeDuration = 1f; // 페이드 아웃 시간
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sideNoticeCanvasGroup.alpha = alpha;
            yield return null;
        }

        SideNotice.SetActive(false);
        sideNoticeCoroutine = null;
    }
    
    private IEnumerator FadeOutMainNoticeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            mainNoticeCanvasGroup.alpha = alpha;
            yield return null;
        }

        MainNotice.SetActive(false);
        mainNoticeCoroutine = null;
    }
}
