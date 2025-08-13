using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "SpawnData/EnenmySpawnData")]
public class EnemySpawnDataSo : ScriptableObject
{
    public int spawnCnt = 1;
    public EnemyDataSo enemyDataSo;
}