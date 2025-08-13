using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class CursorMover : MonoBehaviour
{
    [SerializeField] private ItemType type;
    [SerializeField] private RectTransform goA;  // 시작점 마커
    [SerializeField] private RectTransform goB;  // 끝점 마커
    [SerializeField] private RectTransform cursor;
    [SerializeField] private RectTransform pointAUI; // 디버그 표시
    [SerializeField] private RectTransform pointBUI;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float delay = 0.2f;

    RectTransform CursorParent => cursor.parent as RectTransform;

    void OnEnable()
    {
        Canvas.ForceUpdateCanvases();
        StartCoroutine(InitAndPlay());
    }

    IEnumerator InitAndPlay()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        //슬롯 RectTransform 가져오기
        RectTransform slotA = InventoryUIManager.Instance.inventorySlots[0].GetComponent<RectTransform>();

        RectTransform slotB;
        if (type == ItemType.SkillBook)
            slotB = InventoryUIManager.Instance.skillSlots[0].GetComponent<RectTransform>();
        else
            slotB = InventoryUIManager.Instance.skillSlots[0].runeSlots[0].GetComponent<RectTransform>();

        //슬롯 좌표 goA, goB 위치로 이동
        goA.anchoredPosition = ToLocalCenter(slotA, goA.parent as RectTransform);
        goB.anchoredPosition = ToLocalCenter(slotB, goB.parent as RectTransform);

        //goA / goB 좌표 커서 부모 좌표계로 변환
        Vector2 aLocal = ToLocalCenter(goA, CursorParent);
        Vector2 bLocal = ToLocalCenter(goB, CursorParent);

        //커서 초기 위치 세팅
        cursor.anchoredPosition = aLocal;

        if (pointAUI) pointAUI.anchoredPosition = aLocal;
        if (pointBUI) pointBUI.anchoredPosition = bLocal;

        //트윈 시작
        StartPingPong(aLocal, bLocal);
    }

    void StartPingPong(Vector2 a, Vector2 b)
    {
        var seq = DOTween.Sequence();
        seq.Append(cursor.DOAnchorPos(b, duration).SetEase(Ease.InOutSine)); // A → B
        seq.AppendInterval(delay);
        seq.SetLoops(-1);
    }

    Vector2 ToLocalCenter(RectTransform from, RectTransform toParent)
    {
        // 월드 중심점
        Vector3 worldCenter = from.TransformPoint(from.rect.center);

        // 타깃(커서 부모)의 캔버스
        var canvas = toParent.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        // 월드 → 스크린
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

        // 스크린 → 커서 부모 로컬
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            toParent, screen, cam, out var local);

        return local;
    }
}
