using System.Collections.Generic;
using UnityEngine;

public class SFXPool : MonoBehaviour
{
    private List<AudioSource> audioSources = new List<AudioSource>();
    private int poolSize = 10;
    private int currentIndex = 0;
    private float defaultVolume = 1f;
    
    public void Initialize(int size = 10)
    {
        poolSize = size;
        
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSources.Add(audioSource);
        }
    }
    
    public void Play(SoundData soundData, float customVolume = 1f, float customPitch = 1f)
    {
        float targetVol = soundData.volume    // CSV 에 설정된 기본 볼륨
                          * SoundManager.sfxVolume  // 슬라이더로 조절된 BGM 볼륨
                          * SoundManager.Instance.masterVolume
                          * customVolume;
        AudioSource audioSource = GetAvailableAudioSource();
        
        audioSource.clip = soundData.clip;
        audioSource.volume = targetVol;
        audioSource.pitch = soundData.pitch * customPitch;
        audioSource.loop = soundData.loop;
        audioSource.Play();
    }
    
    private AudioSource GetAvailableAudioSource()
    {
        // 사용 가능한 AudioSource 찾기
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                return audioSources[i];
            }
        }
        
        // 모든 AudioSource가 사용 중이면 가장 오래된 것 사용
        AudioSource oldestSource = audioSources[currentIndex];
        currentIndex = (currentIndex + 1) % audioSources.Count;
        return oldestSource;
    }
    
    public void StopAll()
    {
        foreach (var audioSource in audioSources)
        {
            audioSource.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        defaultVolume = volume;
    }
}