using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Boss01PatternState_LineSweep : EnemyState
{
    Boss01PatternState_LineSweep_So data;

    //반복횟수
    int cnt;
    //발사갯수
    int shotCnt;

    float Delay;

    float warningDuration;
    float warningLength;
    float warningWidth;
    float shotSpeed;

    float coolDown = 5f;

    string castSFX;
    List<string> SFXKeyList;

    AnimationType endAnimation;

    GameObject projectile;
    List<Rune> runeList = new List<Rune>();
    PatternDataSo callBackPattern;
    PatternDataSo specialPattern;

    private Action OnStarted;
    private Action OnUpdate;
    private Action OnFinished;
    public Boss01PatternState_LineSweep(EnemyController enemyController, StateMachine stateMachine, Boss01PatternState_LineSweep_So data) : base(enemyController, stateMachine)
    {
        this.data = data;

        cnt = data.cnt;
        shotCnt = data.shotCnt;
        Delay = data.Delay;
        shotSpeed = data.shotSpeed;
        warningDuration = data.warningDuration;
        warningLength = data.warningLength;
        warningWidth = data.warningWidth;
        endAnimation = data.endAnimation;
        projectile = data.projectileSo.obj;
        coolDown = data.coolDown;

        castSFX = data.castSFX;
        SFXKeyList = data.SFX;
        foreach (Rune rune in data.runeList)
        {
            Rune tempRune = ScriptableObject.Instantiate(rune);
            runeList.Add(tempRune);
        }
        callBackPattern = data.callBackPattern;
        specialPattern = data.specialPattern;

        OnStarted = UnityEventUtil.GetEventAction(data.OnStarted, enemyController);
        OnFinished = UnityEventUtil.GetEventAction(data.OnFinished, enemyController);

    }

    Vector3[] shotPosA;
    Vector3[] shotPosB;

    float shotTime;
    int rand;

    float afterCoolDown;
    public override void Enter()
    {
        if (enemyController.isDead || afterCoolTime > Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        if (this == null || enemyController.gameObject == null) return;

        OnStarted?.Invoke();
        cnt = data.cnt;
        shotPosA = GetShotPos(0);
        shotPosB = GetShotPos(1);
        rand = Random.Range(0, cnt);
    }

    public override void Update()
    {
        if (this == null || enemyController.gameObject == null) return;
        Vector3 dir = enemyController.target.position - enemyController.transform.position;

        
        if (shotTime < Time.time)
        {
            if (cnt <= 0) 
            {
                afterCoolDown = Time.time + coolDown;
                if (callBackPattern != null)
                {
                    var a = callBackPattern.GetType();
                    stateMachine.ChangeState(enemyController.FSM[callBackPattern]);
                }
                else
                {
                    stateMachine.ChangeState(enemyController.idleState);
                }
                return;
            }
            SoundManager.PlaySFX(castSFX);
            shotTime = Time.time + Delay;

            enemyController.AnimationPlay(endAnimation.ToString(), Delay);
            if (specialPattern != null)
            {
                /*
                if (cnt == rand)
                {
                    //엔터만 동작함
                    Logger.Log("동작함");
                    enemyController.FSM[specialPattern].Enter();
                }
                */
                if (cnt == 0)
                {//이건가??
                    //enemyController.FSM[specialPattern].Enter();
                }
            }
            if (cnt % 2 == 0)
            {
                var warningReturn = WarningEffect.Instance.CreateLines(shotPosA, enemyController.transform.right , shotCnt, warningLength, warningWidth, warningDuration, enemyController.targetLayer, () => {
                    for (int i = 0; i < shotCnt; i++)
                    {
                        //Object.Instantiate(enemyController.enemyStat.skillList[0].SkillPrefab, shotPosA[i], Quaternion.identity);
                       
                        foreach (var rune in runeList)
                        {
                            rune.enemy = true;
                            rune.Apply(data.projectileSo);
                        }
                        
                        SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]);
                        GameObject tempProjectile = data.projectileSo.EnemyUse(enemyController.enemyStat.attackPower, enemyController.gameObject, shotSpeed);

                        float angle = Mathf.Atan2(enemyController.transform.right.y, enemyController.transform.right.x) * Mathf.Rad2Deg;  // 방향 벡터에서 각도 계산 (라디안 -> 도)
                        Quaternion rot = Quaternion.Euler(0, 0, angle);
                        tempProjectile.transform.position = shotPosA[i];
                        tempProjectile.transform.rotation = rot;
                        
                    }
                });
                //이건 구조를 좀 수정할 필요가 있을듯? 정상동작 안할거임
                enemyController.tweens.Add(warningReturn.tween);
            }
            else
            {
                var warningReturn = WarningEffect.Instance.CreateLines(shotPosB, enemyController.transform.right, shotCnt, warningLength, warningWidth, warningDuration, enemyController.targetLayer, () => {
                    for (int i = 0; i < shotCnt; i++)
                    {
                        //Object.Instantiate(enemyController.enemyStat.skillList[0].SkillPrefab, shotPosB[i], Quaternion.identity);
                        
                        foreach (var rune in runeList)
                        {
                            rune.Apply(data.projectileSo);
                        }
                        GameObject tempProjectile = data.projectileSo.EnemyUse(enemyController.enemyStat.attackPower, enemyController.gameObject, shotSpeed);
                        float angle = Mathf.Atan2(enemyController.transform.right.y, enemyController.transform.right.x) * Mathf.Rad2Deg;  // 방향 벡터에서 각도 계산 (라디안 -> 도)
                        Quaternion rot = Quaternion.Euler(0, 0, angle);
                        tempProjectile.transform.position = shotPosB[i];
                        tempProjectile.transform.rotation = rot;
                        
                    }
                });
                //이건 구조를 좀 수정할 필요가 있을듯? 정상동작 안할거임
                enemyController.tweens.Add(warningReturn.tween);
            }
            cnt--;
        }
    }

    public override void Exit()
    {
        OnFinished?.Invoke();
    }

    public Vector3[] GetShotPos(float offsetY)
    {
        if (enemyController == null) return null;
        Bounds b = enemyController.roomCollider.bounds;
        Vector3[] shotPos = new Vector3[shotCnt];

        float xPos = b.min.x; // 왼쪽 면 X 좌표 고정
        float yStart = b.min.y;
        float yEnd = b.max.y;

        // Y축 길이
        float height = yEnd - yStart;

        // shotCnt 개수에 맞게 균등 간격 계산 (총 길이 / (갯수 - 1))
        // shotCnt가 1이면 중간 위치 하나 반환
        if (shotCnt == 1)
        {
            shotPos[0] = new Vector3(xPos, (yStart + yEnd) / 2f, 0);
            return shotPos;
        }

        float interval = height / (shotCnt - 1);

        for (int i = 0; i < shotCnt; i++)
        {
            float y = yStart + interval * i + (offsetY * interval / 2);
            shotPos[i] = new Vector3(xPos, y , 0);
        }

        return shotPos;
    }
}
