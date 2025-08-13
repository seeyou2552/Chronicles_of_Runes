using System;
using UnityEngine;
using UnityEngine.UI;

namespace _02_Scripts.Sound
{
    public class SoundSliderHandler : MonoBehaviour
    {
        public Slider BGMSlider;
        public Slider SFXSlider;

        public void Awake()
        {
            BGMSlider.value = SoundManager.bgmVolume;
            SFXSlider.value = SoundManager.sfxVolume;
            BGMSlider.onValueChanged.AddListener(ChangeBGMVolume);
            SFXSlider.onValueChanged.AddListener(ChangeSFXVolume);
        }

        public void ChangeBGMVolume(float volume)
        {
            SoundManager.SetBGMVolume(volume);
        }

        public void ChangeSFXVolume(float volume)
        {
            SoundManager.SetSFXVolume(volume);
        }
    }
}