using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTable", menuName = "GameData/ItemTable")]
public class ItemTable : ScriptableObject
{
    public List<ItemData> items;

    private Dictionary<int, ItemData> _cache;

    public void Initialize()
    {
        _cache = new Dictionary<int, ItemData>();
        foreach (var item in items)
        {
            if (item != null && !_cache.ContainsKey(item.id))
                _cache.Add(item.id, item);
        }
    }

    public ItemData GetItem(int id)
    {
        if (_cache == null)
            Initialize();

        return _cache.TryGetValue(id, out var item) ? item : null;
    }
}