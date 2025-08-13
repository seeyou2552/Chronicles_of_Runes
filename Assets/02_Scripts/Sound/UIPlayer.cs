// UI 플레이어

using UnityEngine;

public class UIPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    
    public void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }
    
    public void Play(SoundData soundData, float customVolume = 1f, float customPitch = 1f)
    {
        audioSource.clip = soundData.clip;
        audioSource.volume = soundData.volume * customVolume;
        audioSource.pitch = soundData.pitch * customPitch;
        audioSource.loop = soundData.loop;
        audioSource.Play();
    }
    
    public void Stop()
    {
        audioSource.Stop();
    }
}