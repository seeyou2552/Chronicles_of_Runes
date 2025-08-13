using _02_Scripts.Enetity.Enemy;
using Pathfinding;
using SmallScaleInc.TopDownPixelCharactersPack1;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;


//드론용 컨트롤러
public class PatternDroneController : MonoBehaviour
{
    EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        
    }

    private void Start()
    {
        enemyController.isDead = false;
    }


    private void OnEnable()
    {
        EventBus.Subscribe(EventType.BossDead, ReturnPool);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.BossDead, ReturnPool);
    }


    public void ReturnPool(object obj)
    {
        enemyController.ReturnPool(null);
    }
}
