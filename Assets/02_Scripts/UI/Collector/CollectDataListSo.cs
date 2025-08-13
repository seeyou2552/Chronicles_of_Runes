using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataCollector", menuName = "DataSo/DataCollector")]
public class CollectDataListSo : ScriptableObject
{
    public SkillDataList skillList;
    public RuneDataList runeList;
    public ItemDataList itemList;
    public AchievementDataList achievementList;

}


[Serializable]
public class SkillDataList
{
    public List<SkillBookData> skillData;
}

[Serializable]
public class RuneDataList
{
    public List<RuneData> runeData;
}

[Serializable]
public class ItemDataList
{
    public List<PotionData> itemData;
}

[Serializable]
public class AchievementDataList
{
    public List<AchievementData> achievementData;
}

public class EnemyDataList
{

}
