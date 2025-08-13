using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterboxCinematic : MonoBehaviour
{

    public float startPos = 750;
    public float endPos = 500;
    public RectTransform top;
    public RectTransform down;
    private void OnEnable()
    {
        EventBus.Subscribe(EventType.BossSpawnCinematic, LetterboxEffect);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.BossSpawnCinematic, LetterboxEffect);
    }


    

    public void LetterboxEffect(object obj)
    {
        DoTweenExtensions.TweenFloat(startPos, endPos, 3f, y => {
            Vector2 pos = top.anchoredPosition;
            pos.y = y;
            top.anchoredPosition = pos;
        },
        () => { // CallBack
            DoTweenExtensions.TweenFloat(endPos, startPos, 1.5f, y =>
            {
                Vector2 pos = top.anchoredPosition;
                pos.y = y;
                top.anchoredPosition = pos;
            });
        });

        DoTweenExtensions.TweenFloat(-startPos, -endPos, 3f, y => {
            Vector2 pos = down.anchoredPosition;
            pos.y = y;
            down.anchoredPosition = pos;
        },
        () => { // CallBack
            DoTweenExtensions.TweenFloat(-endPos, -startPos, 1.5f, y =>
            {
                Vector2 pos = down.anchoredPosition;
                pos.y = y;
                down.anchoredPosition = pos;
            });
        });


    }
}
