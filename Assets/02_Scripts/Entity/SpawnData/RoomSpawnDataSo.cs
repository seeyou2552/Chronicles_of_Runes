using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SpawnData/RoomData")]
public class RoomSpawnDataSo : ScriptableObject
{
    public List<EnemySpawnDataSo> enemyDataSo;
}