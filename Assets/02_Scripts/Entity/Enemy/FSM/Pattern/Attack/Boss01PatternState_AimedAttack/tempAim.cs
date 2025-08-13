using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class tttt : EnemyState
{
    Boss01PatternState_AimedAttack_So data;

    int cnt; //횟수

    GameObject prefab;

    float firstDelay; //선딜
    float lastDelay; //후딜
    bool isDelayStay;

    float warningLength;
    float warningWidth;

    float Ratio;

    float coolDown;
    AnimationType endAnimation;

    string castSFX;
    List<string> SFXKeyList;

    GameObject projectile;
    List<Rune> runeList = new List<Rune>();
    PatternDataSo callBackPattern;
    public tttt(EnemyController enemyController, StateMachine stateMachine, Boss01PatternState_AimedAttack_So data) : base(enemyController, stateMachine)
    {
        this.data = data;
        cnt = data.cnt;
        prefab = data.prefab;
        this.firstDelay = data.firstDelay;
        this.lastDelay = data.lastDelay;
        isDelayStay = data.isDelayStay;
        this.warningLength = data.warningLength;
        this.warningWidth = data.warningWidth;
        this.Ratio = data.Ratio;
        coolDown = data.coolDown;
        castSFX = data.castSFX;
        SFXKeyList = data.SFX;

        endAnimation = data.endAnimation;
        callBackPattern = data.callBackPattern;

        projectile = data.projectileSo.obj;
        foreach (Rune rune in data.runeList)
        {
            Rune tempRune = ScriptableObject.Instantiate(rune);
            runeList.Add(tempRune);
        }
    }



    Vector3 dir;

    //공격나감
    public override async void Enter()
    {
        try
        {

            SoundManager.PlaySFX(castSFX);
            CancellationToken token = enemyController.GetCancellationTokenOnDestroy();
            if (enemyController.isDead) return;

            if (afterCoolTime > Time.time)
            {
                stateMachine.ChangeState(enemyController.idleState);
                return;
            }

            if (!isDelayStay)
            {
                dir = (enemyController.target.transform.position - enemyController.transform.position).normalized;

                enemyController.animationController.LookDir(dir);
                enemyController.AnimationPlay(endAnimation.ToString(), firstDelay, null);
            }
            for (int i = 0; i < cnt; i++)
            {
                if (enemyController.isDead) return;


                dir = (enemyController.target.transform.position - enemyController.transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;  // 방향 벡터에서 각도 계산 (라디안 -> 도)
                Quaternion rot = Quaternion.Euler(0, 0, angle);

                Vector3[] pos = new Vector3[] { enemyController.transform.position };

                var tcs = new UniTaskCompletionSource();

                if (isDelayStay)
                {
                    enemyController.animationController.LookDir(dir);
                    enemyController.AnimationPlay(endAnimation.ToString(), firstDelay, null);
                }


                WarningEffect.Instance.CreateLines
                    (pos, dir, 1, warningLength, warningWidth, firstDelay, enemyController.targetLayer, async () =>
                    {
                        try
                        {
                            SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]);
                            if (enemyController.isDead) return;
                            await UniTask.Delay(0);
                            foreach (var rune in runeList)
                            {
                                rune.enemy = true;
                                rune.Apply(data.projectileSo);
                            }

                            if (enemyController == null) // 파괴되었을 때 리턴
                                return;
                            GameObject tempProjectile = data.projectileSo.EnemyUse(enemyController.enemyStat.attackPower, enemyController.gameObject);
                            tempProjectile.transform.rotation = rot;



                            //후딜
                            //float delayTime = 0.3f;
                            /*
                            float elapsed = 0f;
                            while (elapsed < lastDelay)
                            {
                                elapsed += Time.deltaTime;
                                await UniTask.Yield(); // 프레임 단위 대기
                            }

                            tcs.TrySetResult(); // 현재 반복 종료
                            */

                        }
                        catch (OperationCanceledException) { }

                    });

                //await tcs.Task;

                if (isDelayStay)
                {
                    await UniTask.Delay((int)((firstDelay + lastDelay) * 1000), cancellationToken: token);
                }
                else
                {
                    //마지막 발사는 모두 발사하고 스테이트 변경
                    if (i >= cnt - 1) await UniTask.Delay((int)((firstDelay + lastDelay) * 1000), cancellationToken: token);
                    else await UniTask.Delay((int)(lastDelay * 1000));
                }
            }

            //쿨타임 적용
            afterCoolTime = Time.time + coolDown;
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
        //enemyController.transform.position += dir * speed * Time.deltaTime;
    }

    public override void Exit()
    {
    }



}
