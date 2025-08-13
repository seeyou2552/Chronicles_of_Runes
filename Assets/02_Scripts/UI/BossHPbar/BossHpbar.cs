using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHpbar : Singleton<BossHpbar>
{
    public GameObject HPbar;
    public Slider BgSlider;
    public Slider HpSlider;

    public TextMeshProUGUI nameText;

    private void OnEnable()
    {
        EventBus.Subscribe(LoadState.LoadStart, OffBossHPbar);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(LoadState.LoadStart, OffBossHPbar);
    }

    public void OffBossHPbar(object obj)
    {
        HPbar.SetActive(false);
    }
}
