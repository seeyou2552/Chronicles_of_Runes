using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class PatternState_ChargeExplosion : EnemyState
{
    PatternState_ChargeExplosion_So data;
    private float radius;

    private float firstDelay;
    private float lastDelay;

    private float coolDown;

    private AnimationType endAnimation;

    bool isSelfDestruct;

    PatternDataSo callBackPatternSo;

    private Action OnStarted;
    private Action OnUpdate;
    private Action OnFinished;

    private List<string> SFXKeyList;

    public PatternState_ChargeExplosion(EnemyController enemyController, StateMachine stateMachine, PatternState_ChargeExplosion_So data) : base(enemyController, stateMachine)
    {
        radius = data.radius;
        firstDelay = data.firstDelay;
        lastDelay = data.lastDelay;
        coolDown = data.coolDown;
        endAnimation = data.endAnimation;
        SFXKeyList = data.SFX;
        isSelfDestruct = data.isSelfDestruct;
        callBackPatternSo = data.callBackPattern;
        OnStarted = UnityEventUtil.GetEventAction(data.OnStarted, enemyController);
        OnFinished = UnityEventUtil.GetEventAction(data.OnFinished, enemyController);

    }


    private enum Phase
    {
        Warning,
        Rush,
        Delay,
        End
    }

    private Phase currentPhase;
    private float timer;

    public override void Enter()
    {
        if (enemyController.isDead || afterCoolTime > Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }

        afterCoolTime = Time.time + coolDown;
        
        timer = 0f;
        currentPhase = Phase.Warning;

        OnStarted?.Invoke();
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


        //현재 패턴 구조에서는 timer를 사용하지않음
        switch (currentPhase)
        {
            
            case Phase.Warning:
                currentPhase = Phase.Delay;
                WarningPhase();
                break;
            case Phase.Rush:
                //RushPhase();
                break;
                
            case Phase.End:
                EndPhase();
                break;
        }
        

    }

    public override void Exit()
    {
        OnFinished?.Invoke();
        //캔슬될경우 데미지 주면 안되니 제거해주기
        //warningreturn.go.GetComponent<CircleWarningEffectController>().OnDamage -= HandleDamage;
    }

    (GameObject go, Tween tween) warningreturn;
    public void WarningPhase()
    {
        //enemyController.animationController.LookDir(dir);
        //애니메이션 넣어
        //줘야함
        warningreturn = WarningEffect.Instance.CreateCircle(
            enemyController.transform.position, radius, firstDelay, enemyController.targetLayer,
        () =>
        { SoundManager.PlaySFX(SFXKeyList[Random.Range(0, SFXKeyList.Count)]);

            EndPhase();

            /*
            if (enemyController != null)
            {
                enemyController.StartCoroutine(enemyController.DelayOneFrame(() =>

                {
                    EndPhase();
                }));
            }
            */
        }

        );
        
            enemyController.tweens.Add(warningreturn.tween);

            warningreturn.go.GetComponent<CircleWarningEffectController>().OnDamage += HandleDamage;
            //아마 콜백으로 줄거같긴한데 선딜만큼 기다리기
            //timer = firstDelay + 1;
        
    }

    private void EndPhase()
    {
        if (isSelfDestruct)
        {
            enemyController.OnDamage(999);
        }
        if (callBackPatternSo != null)
        {
            stateMachine.ChangeState(enemyController.FSM[callBackPatternSo]);
        }
        else
        {
            stateMachine.ChangeState(enemyController.idleState);
        }
    }

    //타격판정 워닝사인용
    //이벤트 제거는 워닝사인 내부에서 Get할때 알아서해주는중
    //하 이거 핸들러에 놓고 꺼내쓰는게 좋아보이긴하는데... 복잡해지네....
    private void HandleDamage(List<IDamageable> damageables)
    {
        foreach (var damageable in damageables)
        {
            damageable.OnDamage(enemyController.enemyStat.attackPower);
        }
    }
    //다른 스크립트에서 불러오는식으로 해줄듯?
    //레거시코드임
    /*
    public void ExecuteCallback(UnityEvent unityEvent)
    {
        
        if (unityEvent == null || unityEvent.GetPersistentEventCount() == 0)
        {
            return;
        }

        var prefabTarget = unityEvent.GetPersistentTarget(0);
        var methodName = unityEvent.GetPersistentMethodName(0);

        if (prefabTarget == null || string.IsNullOrEmpty(methodName))
        {
            return;
        }

        // 런타임에 enemyController가 가진 prefabTarget 타입 컴포넌트 찾기
        var runtimeTarget = enemyController.GetComponent(prefabTarget.GetType());
        if (runtimeTarget == null)
        {
            return;
        }

        MethodInfo methodInfo = runtimeTarget.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (methodInfo == null)
        {
            return;
        }

        methodInfo.Invoke(runtimeTarget, null); // 매개변수 있으면 수정 필요
        
    }
    */
}

