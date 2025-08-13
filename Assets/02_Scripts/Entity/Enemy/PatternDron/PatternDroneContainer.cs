using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternDroneContainer : MonoBehaviour
{
    public GameObject[] droneList;
    [HideInInspector] public EnemyController enemyController;

    private List<GameObject> spawnDroneList = new List<GameObject>();
    bool flag;
    bool disableFlag;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }
    private void OnEnable()
    {
        if(flag == false) flag = true;
        else
        {
            Invoke(nameof(SpawnDrone), 6f);
        }
    }

    private void OnDisable()
    {
        /*
        if (disableFlag == false) disableFlag = true;
        else
        {
            for (int i = 0; i < droneList.Length; i++)
            {
                ObjectPoolManager.Instance.Return(spawnDroneList[i], droneList[i].name);
            }
        }
        */
    }

    public void SpawnDrone()
    {
        foreach (GameObject drone in droneList)
        {
            GameObject go = ObjectPoolManager.Instance.Get(drone.name);
            EnemyController droneEnemyController = go.GetComponent<EnemyController>();
            droneEnemyController.roomCollider = enemyController.roomCollider;
            droneEnemyController.enemyStat.attackPower = enemyController.enemyStat.attackPower;
            droneEnemyController.enabled = true;
            
            //spawnDroneList.Add(go);
        }
    }
}
