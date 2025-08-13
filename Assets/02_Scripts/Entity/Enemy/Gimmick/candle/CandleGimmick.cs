using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class CandleGimmick : MonoBehaviour
{
    private EnemyController enemyController;
    public GameObject candle;

    public PatternDataSo pattern;


    Collider2D roomCollider;

    public bool flag = false;

    Vector2[] spawnPos = new Vector2[4];


    public int candleCnt;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        candleCnt = 4;
    }

    private void OnEnable()
    {
        if (flag == false)
        {
            flag = true;
        }
        else
        {
            //타이밍 이슈 임시
            //룸 콜라이더가 OnEnable 이후 할당됨
            Invoke(nameof(Init), 0.5f);
        }
    }



    public void Init()
    {
        roomCollider = enemyController.roomCollider;
        GetSpawnPos();
        for (int i = 0; i < spawnPos.Length; i++)
        {
            GameObject go = Instantiate(candle, spawnPos[i], Quaternion.identity);
            go.GetComponent<CandleController>().candleGimmick = this;
        }
    }
    public void GetSpawnPos()
    {
       Bounds bounds = roomCollider.bounds;
        spawnPos[0] = new Vector2(bounds.center.x, bounds.max.y);
        spawnPos[1] = new Vector2(bounds.center.x, bounds.min.y);
        spawnPos[2] = new Vector2(bounds.min.x, bounds.center.y);
        spawnPos[3] = new Vector2(bounds.max.x, bounds.center.y);
    }


    public void CandleOff()
    {
        candleCnt--;
        if (candleCnt == 0)
        {
            if (enemyController.isDead == false)
            {
                enemyController.stateMachine.ChangeState(enemyController.FSM[pattern]);
                Invoke(nameof(InvokeCandleOn), 3f);
            }
            //화면이 어두워지고 어쩌구
            //어쩌구 저쩌구
        }
    }

    public void CandleOn()
    {
        candleCnt++;
    }

    public void InvokeCandleOn()
    {
        
        EventBus.Publish(GimmickEvent.CandleOn, null);
    }

}
