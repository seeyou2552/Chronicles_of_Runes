using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertController : MonoBehaviour
{
    public Image image;
    public float duration = 0.7f;
    public float startAlpha = 0.14f;

    private void OnEnable()
    {
        FadeOut(duration);
    }

    private void FadeOut(float duration)
    {
        float warningStartAlpha = startAlpha;

        //warningCircle 페이드아웃
        DOTween.To(() => warningStartAlpha, alpha =>
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }, 0f, duration)
        .SetEase(Ease.OutCubic).OnComplete( () => gameObject.SetActive(false));

    }
}
