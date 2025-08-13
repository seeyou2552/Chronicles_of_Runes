using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageRewardEntry
{
    public int itemId;
    public float weight;
}

[Serializable]
public class StageRewardGroup
{
    public int stageNumber; // 숫자형 스테이지
    public List<StageRewardEntry> rewards = new();
}

[CreateAssetMenu(fileName = "StageRewardTable", menuName = "Table/StageRewardTable")]
public class StageRewardTable : ScriptableObject
{
    [SerializeField]
    private List<StageRewardGroup> stageRewardList = new();

    private Dictionary<int, List<StageRewardEntry>> rewardMap = new();

    public List<StageRewardEntry> GetRewards(int stage)
    {
        if (rewardMap.Count == 0)
            BuildMap();

        return rewardMap.TryGetValue(stage, out var list) ? list : new List<StageRewardEntry>();
    }

    public void LoadFromRaw(Dictionary<int, List<StageRewardEntry>> rawMap)
    {
        stageRewardList.Clear();
        foreach (var kvp in rawMap)
        {
            stageRewardList.Add(new StageRewardGroup
            {
                stageNumber = kvp.Key,
                rewards = kvp.Value
            });
        }

        BuildMap(); // 런타임 캐시 갱신
    }

    private void BuildMap()
    {
        rewardMap = new();
        foreach (var group in stageRewardList)
        {
            rewardMap[group.stageNumber] = group.rewards;
        }
    }
}