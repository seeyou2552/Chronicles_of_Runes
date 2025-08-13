using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternState_CreateShelter : EnemyState
{

    private GameObject shelter;
    private float coolDown = 15f;
    private AnimationType endAnimation;
    private List<string> SFXKeyList;

    public PatternDataSo pattern;
    public PatternState_CreateShelter(EnemyController enemyController, StateMachine stateMachine, PatternState_CreateShelter_So data) : base(enemyController, stateMachine)
    {
        shelter = data.shelter;
        coolDown = data.coolDown;
        endAnimation = data.endAnimation;
        SFXKeyList = new List<string>();
        pattern = data.pattern;
    }

    public override void Enter()
    {
        if (enemyController.isDead || afterCoolTime > Time.time)
        {
            stateMachine.ChangeState(enemyController.idleState);
            return;
        }
        afterCoolTime = Time.time + coolDown;

        Object.Instantiate(shelter, GetSpawnPos(), Quaternion.identity);
        enemyController.stateMachine.ChangeState(enemyController.FSM[pattern]);
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
    }


    public Vector3 GetSpawnPos(
    float yOffset = 0f, float extraHeight = 0f, float xMargin = 0f, float centerBias = 0.5f)
    {
        var bounds = enemyController.roomCollider.bounds;

        float xMin = bounds.min.x - xMargin;
        float xMax = bounds.max.x + xMargin;

        float yMin = bounds.min.y;
        float yMax = bounds.max.y;

        // 가운데 중심으로 보정
        float xMid = (xMin + xMax) * 0.5f;
        float yMid = (yMin + yMax) * 0.5f;

        // 중심에서 퍼지는 정도 (0이면 완전 중앙, 1이면 전체 영역)
        float xSpread = (xMax - xMin) * centerBias * 0.5f;
        float ySpread = (yMax - yMin) * centerBias * 0.5f;

        float x = Random.Range(xMid - xSpread, xMid + xSpread);
        float y = Random.Range(yMid - ySpread, yMid + ySpread + extraHeight) + yOffset;

        return new Vector3(x, y, 0f);
    }

}