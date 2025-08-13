using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public List<float> timelineSkipPoint; // 스킵 지점

    public void PlayTimeline()
    {
        playableDirector.Play(); // 재생
    }

    public void PauseTimeline()
    {
        playableDirector.Pause(); // 일시 정지
    }

    public void StopTimeline()
    {
        playableDirector.Stop(); // 정지
    }

    public void SkipToPoint(int index)
    {
        if (index >= timelineSkipPoint.Count) return;

        playableDirector.time = timelineSkipPoint[index];
        playableDirector.Evaluate(); // 강제로 반영
    }

    public void SetTime(float t)
    {
        playableDirector.time = Mathf.Clamp(t, 0, (float)playableDirector.duration);
        playableDirector.Evaluate();
    }
}
