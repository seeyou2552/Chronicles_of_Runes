using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Boss01PatternState_CornerCast : EnemyState
{
    Boss01PatternState_CornerCast_So data;

    float moveDuration;

    PatternDataSo callBackPattern;
    public Boss01PatternState_CornerCast(EnemyController enemyController, StateMachine stateMachine, Boss01PatternState_CornerCast_So data) : base(enemyController, stateMachine)
    {
        this.data = data;
        moveDuration = data.moveDuration;
        callBackPattern = data.callBackPattern;
    }


    //4개 받을듯?
    public Vector3[] cornerPositions;
    public Vector3 centerPos;
    

    public override void Enter()
    {
        if (enemyController.isDead) return;
        //코너로 어떻게 보내지? 기준을 머로할까
        //enemyController.targetPos.position = ;

        centerPos = GetCenter();
        cornerPositions = GetCorners();
        //enemyController.isMove = false;
        //목표로 빠른이동
        enemyController.transform.DOMove(cornerPositions[Random.Range(0, cornerPositions.Length - 1)], moveDuration).SetEase(Ease.InOutSine)
            .OnComplete(()=>
            {
                //우선 이동 완료시 보낼까
                //근데 이동완료후 인보크 호출 후 시간 지나면 호출하는게 더 자연스러울듯?
                //이거는 해당 So 에서 사용할 So를 넣어주면 될듯 ? 변수로?
                //stateMachine.ChangeState(enemyController.FSM[Boss01PatternState_LineSweep_So]);
                if (callBackPattern != null)
                {
                    var a = callBackPattern.GetType();
                    stateMachine.ChangeState(enemyController.FSM[callBackPattern]);
                }
                else
                {
                    stateMachine.ChangeState(enemyController.idleState);
                }
            }); 
        
    }

    public override void Update()
    {
    }

    public override void Exit()
    {

    }

    public Vector3 GetCenter()
    {
        Bounds b = enemyController.roomCollider.bounds;
        return new Vector3(b.center.x, b.center.y, 0);
    }

    public Vector3[] GetCorners()
    {
        Bounds b = enemyController.roomCollider.bounds;

        return new Vector3[] {
        new Vector3(b.min.x, b.min.y, 0),
        new Vector3(b.max.x, b.min.y, 0), 
        new Vector3(b.min.x, b.max.y, 0), 
        new Vector3(b.max.x, b.max.y, 0), 
    };
    }

    /*
     복사용

    public Boss01PatternState_CornerCast(EnemyController enemyController, StateMachine stateMachine) : base(enemyController, stateMachine)
    {
    }

    public override void Enter()
    {

    }

    public override void Update()
    {
    }

    public override void Exit()
    {
    }
    */
}
