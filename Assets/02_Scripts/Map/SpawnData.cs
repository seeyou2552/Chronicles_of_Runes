using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SpawnData/SpawnData")]
public class SpawnData : ScriptableObject
{
    public List<StageDataSo> stageDataList;
}