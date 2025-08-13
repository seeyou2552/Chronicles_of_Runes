using UnityEngine;
using TMPro;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI;
using System;

public static class DoTweenExtensions
{
    // ?먮옒 ?띿뒪?몃? ??뼱?뚯슦硫댁꽌 DoText
    public static Tweener DoText(this TMP_Text text, string targetText, float duration)
    {
        // ?꾩옱 ?띿뒪?몃? 珥덇린媛믪쑝濡??ㅼ젙
        string currentText = text.text;

        // DOTween??To 硫붿꽌?쒕? ?ъ슜?섏뿬 ?띿뒪?몃? ?먯쭊?곸쑝濡?蹂寃?
        return DOTween.To(
            () => currentText, // ?꾩옱 ?띿뒪??媛?
            x => text.text = x, // ?띿뒪???낅뜲?댄듃
            targetText, // 紐⑺몴 ?띿뒪??
            duration
            ).SetEase(Ease.Linear); // ?뷀뤃?몃뒗 Linear Ease濡?
    }

    //텍스트 타이핑하듯이 나오는 기능
    public static Tweener DoTextClean(this TMP_Text text, string targetText, float duration)
    {
        int currentLength = 0;
        return DOTween.To(
            () => currentLength,
            x =>
            {
                currentLength = x;
                text.text = targetText.Substring(0, currentLength);
            },
            targetText.Length,
            duration
            ).SetEase(Ease.Linear);
    }

    //float 원하는 시간동안 증가 및 감소 시켜주는 기능
    /*
    public static float TweenFloat(float start, float end, float duration, System.Action<float> onUpdate = null, System.Action onComplete = null, Ease easeType = Ease.Linear)
    {
        float value;
        value = start;
        DOTween.To(() => value, x =>
        {
            value = x;
            onUpdate?.Invoke(x);
        }, end, duration).SetEase(easeType)
            .OnComplete(() =>
        {
            onComplete?.Invoke();
        });

        return value;
    }
    */

    public static Tween TweenFloat(float start, float end, float duration, Action<float> onUpdate = null, Action onComplete = null, Ease easeType = Ease.Linear)
    {
        float value = start;
        Tween tween = DOTween.To(() => value, x =>
        {
            value = x;
            onUpdate?.Invoke(x);
        }, end, duration)
        .SetEase(easeType)
        .OnComplete(() => onComplete?.Invoke());

        return tween;
    }
   
}