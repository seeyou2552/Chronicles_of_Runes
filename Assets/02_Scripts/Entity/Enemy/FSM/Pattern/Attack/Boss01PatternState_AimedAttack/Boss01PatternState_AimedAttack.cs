using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss01PatternState_AimedAttack : EnemyState
{
    private Boss01PatternState_AimedAttack_So data;

    private int cnt;
    private float firstDelay;
    private float lastDelay;
    private float shotSpeed;
    private bool isDelayStay;
    private float warningLength;
    private float warningWidth;
    private float Ratio;
    private float coolDown;
    private AnimationType endAnimation;
    private string castSFX;
    private List<string> SFXKeyList;
    private PatternDataSo callBackPattern;
    private List<Rune> runeList = new();


    private Action OnStarted;
    private Action OnUpdate;
    private Action OnFinished;

    private enum Phase
    {
        Warning,
        Delay,
        Fire,
        Transition,
        End
    }

    private Phase currentPhase;
    private float timer;
    private int attackIndex;
    private Vector3 dir;
    private Quaternion rotation;

    public Boss01PatternState_AimedAttack(EnemyController enemyController, StateMachine stateMachine, Boss01PatternState_AimedAttack_So data)
        : base(enemyController, stateMachine)
    {
        this.data = data;
        cnt = data.cnt;
        firstDelay = data.firstDelay;
        lastDelay = data.lastDelay;
        shotSpeed = data.shotSpeed;
        isDelayStay = data.isDelayStay;
        warningLength = data.warningLength;
        warningWidth = data.warningWidth;
        Ratio = data.Ratio;
        coolDown = data.coolDown;
        castSFX = data.castSFX;
        SFXKeyList = data.SFX;
        //projectilePrefab = data.projectileSo.obj;
        callBackPattern = data.callBackPattern;
        endAnimation = data.endAnimation;


        OnStarted = UnityEventUtil.GetEventAction(data.OnStarted, enemyController);
        OnFinished = UnityEventUtil.GetEventAction(data.OnFinished, enemyController);

        foreach (var rune in data.runeList)
        {
            Rune temp = ScriptableObject.Instantiate(rune);
            runeList.Add(temp);
        }
    }

    public override void Enter()
    {
        if (enemyController.isDead || afterCoolTime > Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        OnStarted?.Invoke();
        SoundManager.PlaySFX(castSFX);

        attackIndex = 0;
        timer = 0f;
        currentPhase = Phase.Warning;


        //선딜 안기다릴경우 최초 1회만 애니메이션 실행
        if (!isDelayStay)
        {
            enemyController.AnimationPlay(endAnimation.ToString(), firstDelay, null);
        }
    }

    public override void Update()
    {
        if (enemyController.isDead)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        switch (currentPhase)
        {
            case Phase.Warning:
                WarningPhase();
                break;
            case Phase.Delay:
                DelayPhase();
                break;
            case Phase.Fire:
                FirePhase();
                break;
            case Phase.Transition:
                TransitionPhase();
                break;
            case Phase.End:
                EndPhase();
                break;
        }
    }

    public override void Exit()
    {
        OnFinished?.Invoke();
    }

    private void WarningPhase()
    {
        dir = (enemyController.target.transform.position - enemyController.transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);
        rotation = rot;
        enemyController.animationController.LookDir(dir);

        if (isDelayStay)
        {
            enemyController.AnimationPlay(endAnimation.ToString(), firstDelay, null);

            var warningReturn = WarningEffect.Instance.CreateLines(
                new Vector3[] { enemyController.transform.position },
                dir, 1, warningLength, warningWidth, firstDelay,
                enemyController.targetLayer,
                () => 
                {
                    SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]);
                    }
            );

            enemyController.tweens.Add(warningReturn.tween);
            timer = firstDelay;
            currentPhase = Phase.Delay;
        }
        else
        {

            var warningReturn = WarningEffect.Instance.CreateLines(
                new Vector3[] { enemyController.transform.position },
                dir, 1, warningLength, warningWidth, firstDelay,
                enemyController.targetLayer,
                () =>
                {
                    SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]);

                    foreach (var rune in runeList)
                    {
                        rune.enemy = true;
                        rune.Apply(data.projectileSo);
                    }

                    GameObject projectile = data.projectileSo.EnemyUse(enemyController.enemyStat.attackPower, enemyController.gameObject, shotSpeed);
                    projectile.transform.rotation = rot;
                    Logger.Log(attackIndex.ToString() + "발사");
                }
            );

            enemyController.tweens.Add(warningReturn.tween);
            timer = lastDelay;

            if (attackIndex == cnt - 1)
            {
                attackIndex++;
                currentPhase = Phase.Delay;
            }
            else
            {
                attackIndex++;
                currentPhase = Phase.Transition;
            }
        }
    }

    private void DelayPhase()
    {
        if (!isDelayStay)
        {
            timer = firstDelay;

            if (attackIndex >= cnt)
            {
                currentPhase = Phase.End;
            }
            else
            {
                currentPhase = Phase.Transition;
            }
        }

        else if (isDelayStay)
        {
            FirePhase();
        }
    }

    private void FirePhase()
    {
        foreach (var rune in runeList)
        {
            rune.enemy = true;
            rune.Apply(data.projectileSo);
        }

        GameObject projectile = data.projectileSo.EnemyUse(enemyController.enemyStat.attackPower, enemyController.gameObject, shotSpeed);
        projectile.transform.rotation = rotation;

        timer = lastDelay;
        currentPhase = Phase.Transition;
    }

    private void TransitionPhase()
    {
        if (isDelayStay)
        {
            attackIndex++;
            if (attackIndex >= cnt)
            {
                currentPhase = Phase.End;
                timer = 0f;
                return;
            }
            timer = 0f;
            currentPhase = Phase.Warning;
            

        }
        else
        {
            if (attackIndex >= cnt)
            {
                currentPhase = Phase.End;
                timer = 0f;
            }
            else
            {
                currentPhase = Phase.Warning;
                timer = 0f;
            }
        }
    }

    private void EndPhase()
    {
        afterCoolTime = Time.time + coolDown;

        //이걸 해줘야 애니메이션이 움직임
        enemyController.animationController.isAttack = false;

        if (callBackPattern != null)
        {
            stateMachine.ChangeState(enemyController.FSM[callBackPattern]);
        }
        else
        {
            stateMachine.ChangeState(enemyController.idleState);
        }
    }

}