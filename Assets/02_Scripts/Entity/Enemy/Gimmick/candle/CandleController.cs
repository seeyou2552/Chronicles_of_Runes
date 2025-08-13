using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleController : MonoBehaviour, IDamageable
{
    public float timer;
    public float timerOffSet = 10f;


    public bool isActive = true;

    public CandleGimmick candleGimmick;

    private void OnEnable()
    {
        EventBus.Subscribe(GimmickEvent.CandleOn, ResetCandle);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(GimmickEvent.CandleOn, ResetCandle);
    }
    private void Awake()
    {
        timer = 10f;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                isActive = false;
                candleGimmick.CandleOff();

                transform.rotation = Quaternion.Euler(0, 0, 90f);
                //모양 바꾸기
            }
        }
    }

    public void OnDamage(float damage)
    {
        timer = timerOffSet;
        if (isActive == false)
        {
            candleGimmick.CandleOn();
            transform.rotation = Quaternion.Euler(0, 0, 0f);
            //모양 바꾸기
            isActive = true;
        }
    }

    public void ResetCandle(object obj)
    {
        timer = timerOffSet;
        if (isActive == false)
        {
            candleGimmick.CandleOn();
            transform.rotation = Quaternion.Euler(0, 0, 0f);
            //모양 바꾸기
            isActive = true;
        }
    }
}
