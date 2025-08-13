using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RoomData
{
    public GameObject roomPrefab;
    public RoomDirection[] roomType;
}



public class MapController: MonoBehaviour
{
    [Header("Dungeon")]
    public List<RoomData> dungeonRooms;
    public List<RoomData> dungeonLastRoom;
    
    [Header("Castle")]
    public List<RoomData> castleRooms;
    public List<RoomData> castleLastRoom;
    
    [Header("Catacomb")] 
    public List<RoomData> catacombRooms;
    public List<RoomData> catacombLastRooms;
    
    [Header("Volcano")] 
    public List<RoomData> volcanoRooms;
    public List<RoomData> volcanoLastRooms;
    
    [Header("Boss")]
    public List<RoomData> castleBossRooms;
    public List<RoomData> dungeonBossRooms;
    public List<RoomData> catacombBossRooms;
    public List<RoomData> volcanoBossRooms;
    
    public List<RoomData> GetRoomDataList(StageTheme theme, bool a)
    {
        if (!a)
        {
            switch (theme)
            {
                case StageTheme.Castle:
                    return castleRooms;
                case StageTheme.Dungeon:
                    return dungeonRooms;
                case StageTheme.Catacomb:
                    return catacombRooms;
                case StageTheme.Volcano:
                    return volcanoRooms;
            }
        }
        else
        {
            switch (theme)
            {
                case StageTheme.Castle:
                    return castleLastRoom;
                case StageTheme.Dungeon:
                    return dungeonLastRoom;
                case StageTheme.Catacomb:
                    return catacombLastRooms;
                case StageTheme.Volcano:
                    return volcanoLastRooms;
            }
        }
        
        return null;
    }
    
    public RoomData GetRoom(List<RoomDirection> roomType, StageTheme theme, bool i)  // 방향에 맞는 방 리턴
    {
        roomType = roomType.Distinct().ToList();
        
        List<RoomData> list = new List<RoomData>();
        list = GetRoomDataList(theme, false);
        
        if (roomType[0] == RoomDirection.None || roomType.Count == 0)
        {
            return list[Random.Range(0, list.Count)];
        }
        
        List<RoomData> data = new List<RoomData>();
        if (roomType.Count == 1)
        {
            if (i)
            {
                foreach (RoomData room in list)
                {
                    if (room.roomType.Contains(roomType[roomType.Count - 1]))
                    {
                        return room;
                    }
                }
            }
            foreach (RoomData room in list)
            {
                if (room.roomType.Contains(roomType[roomType.Count - 1]))
                {
                    data.Add(room);
                }
            }
        }
        else
        {
            foreach (RoomData room in list)
            {
                if (new HashSet<RoomDirection>(roomType).SetEquals(room.roomType))
                {
                    data.Add(room);
                }
            }
        }
        
        if (data.Count == 0)
        {
            return list[Random.Range(0, list.Count)];
        }
        
        return data[Random.Range(0, data.Count)];
    }

    public List<RoomData> GetBossRoom(StageTheme theme)
    {
        switch (theme)
        {
            case StageTheme.Castle:
                return castleBossRooms;
            case StageTheme.Dungeon:
                return dungeonBossRooms;
            case StageTheme.Catacomb:
                return catacombBossRooms;
            case  StageTheme.Volcano:
                return volcanoBossRooms;
        }
        return null;
    }
}