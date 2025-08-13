using _02_Scripts.UI;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDeadTimeSlow : MonoBehaviour
{
    public float timeScale = 0.2f;
    public float delayTime = 2f;

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.BossDead, BossDead);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.BossDead, BossDead);
    }

    public async void BossDead(object obj)
    {
        Time.timeScale = timeScale;

        await UniTask.Delay((int)(delayTime * 1000), DelayType.Realtime);

        Time.timeScale = 1f;
    }
}
