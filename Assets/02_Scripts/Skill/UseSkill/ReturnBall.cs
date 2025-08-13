using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnBall : MonoBehaviour
{
    SkillAction magicBall;
    public float delay = 3f;
    public float speedRatio = 3f;
    public float tempSpeed;

    public float lineLength = 50f;
    public float lineWidth = 0.3f;
    public float lineDelay = 0.25f;
    Vector3[] pos;

    public string SFX;

    bool startFlag = false;

    private void Awake()
    {
        magicBall = GetComponent<SkillAction>();
    }
    void Start()
    {
        
        
        
    }

    private void OnEnable()
    {
        magicBall.speed = tempSpeed;
        if (startFlag == false)
        {
            startFlag = true;
        }
        else
        {
            Invoke(nameof(ReturnToTarget), delay);
        }
    }

    public void ReturnToTarget()
    {

        if(true)
        {
            tempSpeed = magicBall.speed;
            magicBall.speed = 0;
            Vector3 dir = PlayerController.Instance.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;  // 방향 벡터에서 각도 계산 (라디안 -> 도)
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            pos = new Vector3[] { transform.position };
            WarningEffect.Instance.CreateLines(pos, dir, 1, lineLength, lineWidth, lineDelay, 0,  () =>
            {
                SoundManager.PlaySFX(SFX);
                transform.rotation = rot;
                magicBall.speed = tempSpeed * speedRatio;
            }
            );

            
        }
    }

}
