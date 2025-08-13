using System.Collections;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine fadeCoroutine;
    
    public void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }
    
    public void Play(SoundData soundData, float fadeTime = 0f, float customVolume = 1f, float customPitch = 1f)
    {
        float targetVol = soundData.volume    // CSV 에 설정된 기본 볼륨
                          * SoundManager.bgmVolume  // 슬라이더로 조절된 BGM 볼륨
                          * SoundManager.Instance.masterVolume;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (fadeTime > 0f)
        {
            fadeCoroutine = StartCoroutine(FadePlay(soundData, fadeTime));
        }
        else
        {
            audioSource.clip = soundData.clip;
            audioSource.volume = soundData.volume * targetVol;
            audioSource.pitch = soundData.pitch * customPitch;
            audioSource.loop = soundData.loop;
            audioSource.Play();
        }
    }
    
    public void Stop(float fadeTime = 0f)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (fadeTime > 0f)
        {
            fadeCoroutine = StartCoroutine(FadeStop(fadeTime));
        }
        else
        {
            audioSource.Stop();
        }
    }
    
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
    
    public bool IsPlaying => audioSource.isPlaying;
    
    private IEnumerator FadePlay(SoundData soundData, float fadeTime)
    {
        float startVolume = audioSource.volume;
        audioSource.volume = 0f;
        
        audioSource.clip = soundData.clip;
        audioSource.pitch = soundData.pitch;
        audioSource.loop = soundData.loop;
        audioSource.Play();
        
        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, soundData.volume, timer / fadeTime);
            yield return null;
        }
        
        audioSource.volume = soundData.volume;
        fadeCoroutine = null;
    }
    
    private IEnumerator FadeStop(float fadeTime)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }
        
        audioSource.volume = 0f;
        audioSource.Stop();
        fadeCoroutine = null;
    }
}