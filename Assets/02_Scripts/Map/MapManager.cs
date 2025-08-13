using _02_Scripts;
using _02_Scripts.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public struct leftRoom  //앞으로 만들어야 하는 방의 위치와 타입
{
    public Vector3 position;
    public RoomDirection roomType;

    public leftRoom(Vector3 position, RoomDirection roomType)
    {
        this.position = position;
        this.roomType = roomType;
    }
}

public class MapManager : MonoBehaviour
{
    public MapController mapController;
    public GameObject MapList;  //프리팹이 생성될 위치
    public MapDecorationController mapDecorationController;  //맵 장식물 관리자
    public EnemySpawnManager EnemySpawnManager;
    
    private List<Room> roomScripts = new List<Room>(); //방 프리팹에 붙어있는 스크립트
    private List<GameObject> rooms = new List<GameObject>(); //생성된 방 오브젝트
    private List<RoomData> roomData = new List<RoomData>(); // 생성된 방 프리팹 리스트
    private List<leftRoom> leftRoomData = new List<leftRoom>(); //생성된 방들의 남은 길 리스트
    private List<int> lastRoomNum = new List<int>(); //끄트머리 방 번호 리스트
    
    private int roomNumber;  // 생성할 방의 수(완벽히 이 숫자에 맞춰지지 않음)
    private int roomPrecess; //방 생성 진행도
    private int bossRoomNumber; //보스방 번호
    private int shopRoomNumber;
    private float x_interval = 58.0f;  //방들간 간격
    private float y_interval = 40.0f;  //방들간 간격
    private static StageTheme  stageTheme;
    private static int stage;

    void OnEnable()
    {
        EventBus.Subscribe(GameState.GameOver, ResetStage);
        EventBus.Subscribe(GameState.Ending, ResetStage);
        switch (GameManager.Instance.dungeonLevel)
        {
            case DungeonLevel.Easy:
                roomNumber = 12;
                break;
            case DungeonLevel.Normal:
                roomNumber = 18;
                break;
            case  DungeonLevel.Hard:
                roomNumber = 21;
                break;
        }
    }

    void OnDisable()
    {
        EventBus.Unsubscribe(GameState.GameOver, ResetStage);
        EventBus.Unsubscribe(GameState.Ending, ResetStage);
    }
    
    void Start()
    {
        if (GameManager.Instance.sceneFlowManager.currentStageIndex != 0)
        {
            stage = GameManager.Instance.sceneFlowManager.currentStageIndex - 1;
        }
        
        rooms = new List<GameObject>();
        MapReset();
        
        if (roomScripts != null && roomScripts.Count > 0)
            DoorControl(false);
        EventBus.Publish("MapDrawComplete", roomScripts);
        PlayerController.Instance.statHandler.PlusRerollCount(2);
        // InventoryManager.Instance.AddItem(3);
    }
    
    public void MapReset()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "BossScene")
        {
            ClearMap();
            stage++; StageToEnemySpawnManager();
            stageTheme = (StageTheme)Random.Range(0, Enum.GetValues(typeof(StageTheme)).Length);
            MapGenerate(stageTheme); //랜덤 맵 테마 선택 후 맵 생성
            SetBossRoom(); //보스룸 선택
            SetShopRoom(sceneName); //상점방 선택
            //CreateRoomDecoration(); //방 장식 생성
            CameraManager.Instance.ChangeBackground(stageTheme);
            StoreManager.Instance.purchaseManager.GeneratePurchaseList();
            //EventBus.Publish("MapDrawComplete", roomScripts);
        }
        else
        {
            ClearMap();
            StageToEnemySpawnManager();
            CreateBossRoom();
            mapDecorationController.RepositionShopNPC(new Vector3(0,0,0), sceneName);
        }
        
        AstarPath.active.Scan();
        StartCoroutine(StartScanAsync());
        if (roomScripts != null && roomScripts.Count > 0)
            DoorControl(false);
    }

    public IEnumerator StartScanAsync()
    {
        yield return AstarPath.active.ScanAsync(); // ScanAsync() 자체가 IEnumerator니까 yield return으로 바로 기다림
    }




    public void MapGenerate(StageTheme theme)  // 맵 생성
    {
        leftRoomData = new List<leftRoom>();
        rooms = new List<GameObject>();
        roomData = new List<RoomData>();
        roomScripts = new List<Room>();
        lastRoomNum = new List<int>();
        stageTheme = theme;
        List<RoomDirection> tempType = new List<RoomDirection>();
        
        tempType.Add(RoomDirection.None);
        GenerateRoom(mapController.GetRoomDataList(theme, false)[Random.Range(0, 11)], new Vector3(-5, 8, 0));
        tempType.Clear();
        roomPrecess++;

        foreach (RoomDirection type in roomData[0].roomType)
        {
            leftRoomData.Add(new leftRoom(new Vector3(-5, 8, 0), type));
        }

        while (true)
        {
            if (leftRoomData.Count > 0)
            {
                CreateRoom(leftRoomData[0].roomType, leftRoomData[0].position, theme);
                leftRoomData.RemoveAt(0);
            }

            // 남은 길이 남은 방의 갯수와 같으면 마무리
            if (roomNumber - roomPrecess <= leftRoomData.Count)
            {
                foreach (leftRoom room in leftRoomData)
                {
                    CreateLastRoom(room.roomType, room.position, theme);
                }

                //겹치는 방 처리
                List<RoomDirection> result = new List<RoomDirection>();
                Vector3 tempVector;

                for (int i = 0; i < rooms.Count; i++)
                {
                    for (int j = i + 1; j < rooms.Count; j++)
                    {
                        // 위치 비교
                        if (Vector3.Distance(rooms[i].transform.position, rooms[j].transform.position) < 0.05f)
                        {
                            tempVector = rooms[i].transform.position;
                            int b = 0;
                            float x, y;

                            foreach (GameObject obj in rooms)  //상하좌우에 있는 방들의 길 방향 조사
                            {
                                x = rooms[i].transform.position.x - obj.transform.position.x;
                                y = rooms[i].transform.position.y - obj.transform.position.y;
                                
                                
                                if (x < (x_interval + 0.05f) && x > (x_interval - 0.05f) &&
                                    y < 0.05f && y > -0.05f)
                                {
                                    if (roomData[b].roomType.Contains(RoomDirection.East))
                                    {
                                        result.Add(RoomDirection.West);
                                    }
                                }
                                else if (x > -(x_interval + 0.05f) && x < -(x_interval - 0.05f) &&
                                         y < 0.05f && y > -0.05f)
                                {
                                    if (roomData[b].roomType.Contains(RoomDirection.West))
                                    {
                                        result.Add(RoomDirection.East);
                                    }
                                }
                                else if (x < 0.05f && x > -0.05f &&
                                         y < (y_interval + 0.05f) && y > (y_interval - 0.05f))
                                {
                                    if (roomData[b].roomType.Contains(RoomDirection.North))
                                    {
                                        result.Add(RoomDirection.South);
                                    }
                                }
                                else if (x < 0.05f && x > -0.05f && 
                                         y < -(y_interval - 0.05f) && y > -(y_interval + 0.05f))
                                {
                                    if (roomData[b].roomType.Contains(RoomDirection.South))
                                    {
                                        result.Add(RoomDirection.North);
                                    }
                                }

                                b++;
                            }
                            
                            tempVector = rooms[i].transform.position;
                            
                            //생성
                            if (result.Count == 1)
                            {
                                CreateLastRoom(result[0], tempVector, theme);
                            }
                            else
                            {
                                GenerateRoom(mapController.GetRoom(result, theme, true),  tempVector);
                            }
                            roomPrecess++;
                                    
                            int first = Mathf.Min(i, j);
                            int second = Mathf.Max(i, j);

                            // 기존 겹쳐있던 방 제거
                            Destroy(rooms[second]);
                            rooms.RemoveAt(second);
                            roomData.RemoveAt(second);
                            roomScripts.RemoveAt(second);

                            Destroy(rooms[first]);
                            rooms.RemoveAt(first);
                            roomData.RemoveAt(first);
                            roomScripts.RemoveAt(first);
                                    
                            i = -1;
                            
                            result = new List<RoomDirection>();
                            tempVector = new Vector3();

                            break; // j 루프 종료
                        }
                    }
                }

                for (int k = 0; k < roomData.Count; k++)  //끄트머리 방 번호 기록
                {
                    if (roomData[k].roomType.Length == 1)
                    {
                        lastRoomNum.Add(k);
                    }
                }

                foreach (Room room in roomScripts)
                {
                    if (Vector3.Distance(new Vector3(-5, 8, 0), room.gameObject.transform.position) <= 0.05f)
                    {
                        room.roomType = RoomType.Start;
                    }
                }
                
                break;
            }
        }
    }

    private void GenerateRoom(RoomData data, Vector3 position)  //프리팹 생성
    {
        roomData.Add(data);
        rooms.Add(Instantiate(data.roomPrefab, position, Quaternion.identity, MapList.transform));
        roomScripts.Add(rooms[rooms.Count-1].GetComponent<Room>());
        roomScripts[rooms.Count-1].roomType = RoomType.Normal;
        roomScripts[rooms.Count - 1].Directions = data.roomType;
    }
    
    private void CreateRoom(RoomDirection roomType, Vector3 position, StageTheme theme)  //일반 방 생성
    {
        RoomData rand;
        List<RoomDirection> tempType = new List<RoomDirection>();
        while (true)
        {
            tempType.Add(GetOppositeDirection(roomType));
            rand = mapController.GetRoom(tempType, theme, false);

            //남은 방 갯수와 길의 갯수 비교
            if (roomNumber - roomPrecess >= rand.roomType.Length + leftRoomData.Count - 1)
            {
                break;
            }
        }
        
        GenerateRoom(rand, GetRoomPosition(roomType, position));

        for (int i = 0; i < rand.roomType.Length; i++) //남은 길 목록에 새로생긴 방의 길 추가
        {
            if (GetOppositeDirection(roomType) != rand.roomType[i])
            {
                leftRoomData.Add(new leftRoom(rooms[roomPrecess].transform.position,
                    rand.roomType[i]));
            }
        }
        roomPrecess++;
    }

    private RoomDirection GetOppositeDirection(RoomDirection dir) //반대방향 리턴
    {
        switch (dir)
        {
            case RoomDirection.North: return RoomDirection.South;
            case RoomDirection.South: return RoomDirection.North;
            case RoomDirection.East:  return RoomDirection.West;
            case RoomDirection.West:  return RoomDirection.East;
            default: return dir;
        }
    }

    private Vector3 GetRoomPosition(RoomDirection roomType, Vector3 position) //방향에 따라 위치 수정후 리턴
    {
        switch (roomType)  //위치 수정
        {
            case RoomDirection.North: return (position + new Vector3(0, y_interval, 0));
            case RoomDirection.South: return (position + new Vector3(0, -y_interval, 0));
            case RoomDirection.East: return (position + new Vector3(x_interval, 0, 0));
            case RoomDirection.West: return (position + new Vector3(-x_interval, 0, 0));
        }
        return position;
    }
    
    private void CreateLastRoom(RoomDirection roomType, Vector3 position, StageTheme theme)  //끄트머리 방 생성
    {
        rooms.Add(Instantiate(GenerateLastRoom(roomType, theme),
            GetRoomPosition(roomType, position), Quaternion.identity, MapList.transform));
        roomScripts.Add(rooms[(rooms.Count - 1)].GetComponent<Room>());
        
        RoomDirection[] tempType = new RoomDirection[1];
        tempType[0] = roomType;
        roomScripts[rooms.Count - 1].Directions = tempType;
        roomPrecess++;
    }
    
    private GameObject GenerateLastRoom(RoomDirection roomType, StageTheme theme)  //방향에 맞는 끄트머리 방 리턴
    {
        List<RoomData> list =  new List<RoomData>();

        foreach (RoomData data in mapController.GetRoomDataList(theme, true))
        {
            if (data.roomType[0] == GetOppositeDirection(roomType))
            {
                list.Add(data);
            }
        }
        
        RoomData rand = list[Random.Range(0, list.Count)];
        roomData.Add(rand);
        return rand.roomPrefab;
    }

    public void ClearMap() //맵 초기화
    {
        if (rooms.Count != 0)
        {
            foreach (var roomObj in rooms)
            {
                if (roomObj != null)
                    Destroy(roomObj);
            }
            
            rooms.Clear();
        }
        
        
        roomScripts.Clear();
        roomData.Clear();
        leftRoomData.Clear();
        lastRoomNum.Clear();
        mapDecorationController.ResetDecorations();
        EnemySpawnManager.ResetEnemySpawnManager();
    
        roomPrecess = 0;
        bossRoomNumber = -1;
    }
    
    private void SetBossRoom()  //포탈방을 랜덤하게 지정
    {
        bossRoomNumber = lastRoomNum[Random.Range(0,lastRoomNum.Count)];
        mapDecorationController.CreateBossRoomPortal(roomScripts[bossRoomNumber].transform.position);
        roomScripts[bossRoomNumber].roomType = RoomType.BossPortal;
    }
    
    private void SetShopRoom(string sceneName)  //상점방을 랜덤하게 지정
    {
        while (true)
        {
            shopRoomNumber = lastRoomNum[Random.Range(0,lastRoomNum.Count)];
            if (shopRoomNumber != bossRoomNumber)
                break;
        }

        mapDecorationController.RepositionShopNPC(roomScripts[shopRoomNumber].transform.position, sceneName);
        roomScripts[shopRoomNumber].roomType = RoomType.Shop;
    }
    
    public Room GetBossRoomScript() //보스방 스크립트 리턴
    {
        return roomScripts[bossRoomNumber];
    }

    // public void CreateRoomDecoration()  //방 장식물, 장애물 생성
    // {
    //     for (int i = 0; i < roomScripts.Count; i++)
    //     {
    //         // if (i != bossRoomNumber && i != shopRoomNumber && roomScripts[i].roomShape == RoomShape.Cell) 
    //         //     mapDecorationController.CreateDecoration(roomScripts[i].transform.position);
    //         
    //         if(roomScripts[i].roomType == RoomType.Normal)
    //             mapDecorationController.CreateObstacle(roomScripts[i]);
    //     }
    // }

    public void DoorControl(bool isClosed) // 모든 방의 문 여닫기
    {
        foreach (Room room in roomScripts)
        {
            if(room.door != null)
                room.door.SetActive(isClosed);
            
            if (room.roomType == RoomType.Start)
            {
                if(room.hole != null)
                    room.hole.SetActive(false);
            }
        }
    }

    public void CreateBossRoom()
    {
        rooms = new List<GameObject>();
        roomScripts = new List<Room>();

        List<RoomData> tempList = mapController.GetBossRoom(stageTheme);

        GameObject room2 = Instantiate(tempList[1].roomPrefab, new Vector3(53, 8, 0), Quaternion.identity, MapList.transform);
        rooms.Add(room2);
        roomScripts.Add(room2.GetComponent<Room>());
        
        GameObject room1 = Instantiate(tempList[0].roomPrefab, new Vector3(-5, 8, 0), Quaternion.identity, MapList.transform);
        rooms.Add(room1);
        roomScripts.Add(room1.GetComponent<Room>());

        
    }

    public void StageToEnemySpawnManager() // EnemySpawnManager에 스테이지 전달
    {
        EnemySpawnManager.stageIndex = stage - 1;
    }
    
    public void ResetStage(object param)
    {
        stage = 0;
        StoreManager.Instance.purchaseManager.GeneratePurchaseList();
    }
}