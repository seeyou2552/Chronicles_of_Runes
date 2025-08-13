using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemDropData
{
    public float chance;
    public int itemId;
}

[System.Serializable]
public class MonsterDrop
{
    public string MonsterName;
    public List<ItemDropData> ItemDropList = new();
    public int DropGold;
}

[CreateAssetMenu(fileName = "MonsterDropTable", menuName = "Tables/MonsterDropTable")]
public class MonsterDropTable : ScriptableObject
{
    public List<MonsterDrop> MonsterDropList = new();
}
