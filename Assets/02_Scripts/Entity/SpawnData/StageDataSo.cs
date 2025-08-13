using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SpawnData/StageData")]
public class StageDataSo : ScriptableObject
{
    public EnemyDataSo bossEnemyData;
    public List<RoomSpawnDataSo> roomSpawnDataList;
}