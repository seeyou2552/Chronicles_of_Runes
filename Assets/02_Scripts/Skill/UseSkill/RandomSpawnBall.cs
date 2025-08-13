using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomSpawnBall : MonoBehaviour
{
    MagicBall magicBall;
    public float radius = 0f;
    float tempSpeed;

    public float lineLength = 50f;
    public float lineWidth = 0.3f;
    public float lineDelay = 0.75f;
    Vector3[] pos;

    Transform target;

    Action action;

    Vector3 vectorPos;
    void Awake()
    {
        //magicBall = GetComponent<MagicBall>();
        //tempSpeed = magicBall.speed;
    }

    private void OnEnable()
    {
        target = PlayerController.Instance.transform;

        MoveToTarget();
        //action?.Invoke();
    }

    private void Start()
    {
        action = MoveToTarget;
    }

    void Update()
    {
        Logger.Log(transform.position.ToString());
    }
    public void MoveToTarget()
    {
        Logger.Log("호출됨");
        //magicBall.speed = 0;
        vectorPos = GetRandomPos(target, radius);
        Logger.Log(transform.position.ToString());
        transform.position = vectorPos;
        Logger.Log(transform.position.ToString());


        /*Vector3 dir = PlayerController.Instance.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;  // 방향 벡터에서 각도 계산 (라디안 -> 도)
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        pos = new Vector3[] { transform.position };
        WarningEffect.Instance.CreateLines(pos, dir, 1, lineLength, lineWidth, lineDelay, 0, () =>
        {
            transform.rotation = rot;
            magicBall.speed = tempSpeed;
        }
        );
        */
    }

    public Vector3 GetRandomPos(Transform target, float radius)
    {
        Logger.Log("위치 바꿨음");
        Logger.Log(transform.position.x.ToString() + " : " + transform.position.y.ToString() + " : " + transform.position.z.ToString());

        float angle = Random.Range(0f, 360f); // 0~360도 중 랜덤
        float rad = angle * Mathf.Deg2Rad;    // 라디안 변환

        float x = Mathf.Cos(rad) * radius;
        float y = Mathf.Sin(rad) * radius;


        Logger.Log((target.position.x + x).ToString() + " : " + (target.position.y + y).ToString());
        return new Vector3(
            target.position.x + x,
            target.position.y + y,
            0f
        );
    }

}
