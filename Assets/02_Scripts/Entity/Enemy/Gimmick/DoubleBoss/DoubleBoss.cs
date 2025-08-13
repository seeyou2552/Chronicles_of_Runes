using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class DoubleBoss : MonoBehaviour, IPatternCallBack
{
    public EnemyDataSo enemyDataSo;

    public float spawnOffSetY = 5f;

    [Header("디버깅용")]
    public EnemyController enemyController;
    public EnemyController otherEnemyController;

    public GameObject secondBoss;

    bool flag = false;
    public bool isSecond = false;

    public PatternDataSo callBackPatternDataSo;
    public PatternDataSo stayPatternDataSo;
    public PatternDataSo rushPatternDataSo;


    public int cntIndex = 0;
    private int Maxcnt = 3;
    public bool patternFlag = false;
    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();

    }

    //이쪽보스는 isboss를 안해야할듯?

    private void OnEnable()
    {
        if (!isSecond)
        {
            if (flag == false)
            {
                flag = true;
            }
            else
            {
                if (!isSecond)
                    ObjectPoolManager.Instance.CreatePool(enemyDataSo.enemyPrefab.name, enemyDataSo.enemyPrefab, 1);

                secondBoss = ObjectPoolManager.Instance.Get(enemyDataSo.enemyPrefab.name);
                ObjectPoolManager.Instance.Return(secondBoss, enemyDataSo.enemyPrefab.name);
                secondBoss = ObjectPoolManager.Instance.Get(enemyDataSo.enemyPrefab.name);


                secondBoss.GetComponent<DoubleBoss>().otherEnemyController = enemyController;
                secondBoss.GetComponent<DoubleBoss>().secondBoss = gameObject;
                otherEnemyController = secondBoss.GetComponent<EnemyController>();

                //룸콜라이더때문에 시간여유두고 주입
                //SpawnEnemyInit();
                Invoke(nameof(SpawnEnemyInit), 0.125f);
                //소환한 두번째 보스도 뭔가 스크립트를 들고있어야할듯?
            }
        }
    }

    public void SpawnEnemyInit()
    {
        Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y + spawnOffSetY);
        secondBoss.transform.position = spawnPos;
        otherEnemyController.doubleBoss.isSecond = true;
        otherEnemyController.ConvertSoToStat(enemyDataSo);
    }

    public void CallBack()
    {
        if (otherEnemyController.isDead == false)
        {
            if (cntIndex < Maxcnt)
            {
                cntIndex++;

                //상대방의 상태를 변경함
                CallBackChangePattern();
            }
        }


    }

    public void CallBackChangePattern()
    {
        //기존 패턴 리턴 시켜야하는데 어떻게?
        //콜백패턴으로 넣어주기로함
        //enemyController.stateMachine.ChangeState(enemyController.FSM[stayPatternDataSo]);
        if (otherEnemyController.doubleBoss.cntIndex < Maxcnt)
        {
            otherEnemyController.stateMachine.ChangeState(otherEnemyController.FSM[otherEnemyController.doubleBoss.callBackPatternDataSo]);
        }

        else
        {
            cntIndex = 0;
            otherEnemyController.doubleBoss.cntIndex = 0;
        }
    }

    public void ResetCnt()
    {
        cntIndex = 0;
    }

    public void JointChargeAttack()
    {
        if (otherEnemyController.isDead == false)
        {
            otherEnemyController = secondBoss.GetComponent<EnemyController>();
            otherEnemyController.stateMachine.ChangeState(otherEnemyController.FSM[otherEnemyController.doubleBoss.rushPatternDataSo]);
        }
    }

    public void EndJointChargeAttack()
    {
        if (otherEnemyController.isDead == false)
        {
            otherEnemyController.stateMachine.ChangeState(otherEnemyController.idleState);
        }
    }
}
