using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class Boss01PatternState_CircleAttack : EnemyState
{
    Boss01PatternState_CircleAttack_So data;

    int cnt;
    int shotCnt;

    float radius;

    int installIntervalDelay;
    int fireRate;

    PatternDataSo callBackPattern;
    public Boss01PatternState_CircleAttack(EnemyController enemyController, StateMachine stateMachine, Boss01PatternState_CircleAttack_So data) : base(enemyController, stateMachine)
    {
        this.data = data;
        cnt = data.cnt;
        shotCnt = data.shotCnt;
        radius = data.radius;
        installIntervalDelay = data.installIntervalDelay;
        fireRate = data.fireRate;
        callBackPattern = data.callBackPattern;
    }

    Vector3[] pos;


    public override async void Enter()
    {
        try
        {
            CancellationToken token = enemyController.GetCancellationTokenOnDestroy();


            if (enemyController.isDead) return;
            enemyController.aiPath.canMove = true;


            for (int j = 0; j < cnt; j++)
            {
                if (this == null || enemyController.gameObject == null) return;


                //발사 위치
                Vector3 direction = enemyController.target.position - enemyController.transform.position;  // 방향 벡터
                float angleRad = Mathf.Atan2(direction.y, direction.x); // 라디안 각도 (X축 기준)
                float angleDeg = angleRad * Mathf.Rad2Deg;

                pos = GetCirclePositions(enemyController.transform.position, radius, shotCnt, angleDeg + UnityEngine.Random.Range(-10, 10));

                for (int i = 0; i < shotCnt; i++)
                {
                    //발사 각도
                    Vector3 dir = pos[i] - enemyController.transform.position;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;  // 방향 벡터에서 각도 계산 (라디안 -> 도)
                    Quaternion rot = Quaternion.Euler(0, 0, angle);
                    //GameObject go = UnityEngine.Object.Instantiate(enemyController.enemyStat.skillList[1].SkillPrefab, pos[i], rot);

                    await UniTask.Delay(installIntervalDelay);
                }
                await UniTask.Delay(fireRate);
            }

            if (callBackPattern != null)
            {
                var a = callBackPattern.GetType();
                stateMachine.ChangeState(enemyController.FSM[callBackPattern]);
            }
            else
            {
                stateMachine.ChangeState(enemyController.idleState);
            }
        }
        catch (OperationCanceledException) { }
        catch (MissingReferenceException) { }
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        enemyController.aiPath.canMove = false;
    }


    public static Vector3[] GetCirclePositions(Vector3 center, float radius, int count, float startAngle = 0f)
    {
        Vector3[] positions = new Vector3[count];
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;  // 시작 각도부터 시작해서 각 점 계산
            float rad = angle * Mathf.Deg2Rad;

            float x = center.x + Mathf.Cos(rad) * radius;
            float y = center.y + Mathf.Sin(rad) * radius;

            positions[i] = new Vector3(x, y, center.z);
        }

        return positions;
    }
}
