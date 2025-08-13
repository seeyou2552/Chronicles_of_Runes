using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;  // Pointer 이벤트를 사용하기 위한 네임스페이스

public class ButtonTransparency : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buttonImage; // 이 버튼의 Image 컴포넌트
    private Color originalColor; // 원래 이미지 색상 저장

    public float highlightedAlpha = 1f; // 마우스가 올라갔을 때 불투명도
    public float normalAlpha = 0.5f; // 마우스가 벗어났을 때 불투명도

    private void Start()
    {
        if (buttonImage != null)
        {
            originalColor = buttonImage.color; // 원래 색상을 저장
        }
    }

    // 마우스가 버튼 위로 올라갔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            Color newColor = buttonImage.color;
            newColor.a = highlightedAlpha;  // 알파값을 설정하여 불투명도를 높임
            buttonImage.color = newColor;
        }
    }

    // 마우스가 버튼에서 벗어났을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            Color newColor = originalColor; // 원래 색상으로 되돌림
            newColor.a = normalAlpha; // 알파값을 설정하여 불투명도를 낮춤
            buttonImage.color = newColor;
        }
    }
}