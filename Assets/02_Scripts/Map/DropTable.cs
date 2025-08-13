using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropData
{
    public ItemGrade grade;
    public float dropRate;
    public List<ItemData> items;
}

[CreateAssetMenu(menuName = "DropTable/DropTable")]
public class DropTable : ScriptableObject
{
    public List<DropData> dropData;
}
