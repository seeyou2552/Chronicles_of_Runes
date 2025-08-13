using _02_Scripts.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniMapController : Singleton<MiniMapController>
{
    [SerializeField] private GameObject canvas;
    
    [Header("List")]
    [SerializeField] private GameObject roadList;
    [SerializeField] private GameObject cellList;
    private List<GameObject> roadObjects = new List<GameObject>();
    private List<GameObject> cellObjects = new List<GameObject>();
    private List<GameObject> mapMarker = new List<GameObject>();
    
    [Header("Objects")]
    [SerializeField] private GameObject cell;
    [SerializeField] private GameObject road_EW;
    [SerializeField] private GameObject road_SN;
    [SerializeField] private Image shopMarker;
    [SerializeField] private Image portalMarker;

    public float roomGapX; public float roomGapY;
    
    private PlayerInputActions inputActions;
    private bool drawed = false;

    private async void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            EventBus.Subscribe("MapDrawComplete", DrawMap);
            EventBus.Subscribe("FirstTouch", RevealMap);


            await UniTask.Delay(1000);
            inputActions = SaveManager.Instance.inputActions;
            if (inputActions == null) return;
            inputActions.Player.Map.performed += OnMiniMapPerformed;
        }
    }
    /*
    private void OnEnable()
    {
        Logger.Log("활성화");
        EventBus.Subscribe("MapDrawComplete", DrawMap);
        EventBus.Subscribe("FirstTouch", RevealMap);
        
        inputActions = SaveManager.Instance.inputActions;
        if (inputActions == null) return;
        inputActions.Player.Map.performed += OnMiniMapPerformed;
    }
    */

    /*
    private void OnDisable()
    {
        Logger.Log("디스에이블");
        EventBus.Unsubscribe("MapDrawComplete", DrawMap);
        EventBus.Unsubscribe("FirstTouch", RevealMap);

        // 입력 해제
        if (inputActions != null)
            inputActions.Player.Map.performed -= OnMiniMapPerformed;

        ClearMap();
    }
    */

    private void OnDestroy()
    {
        
        if (Instance == this)
        {
            EventBus.Unsubscribe("MapDrawComplete", DrawMap);
            EventBus.Unsubscribe("FirstTouch", RevealMap);


        }
        
    }
    public void DrawMap(object value)
    {
        

        ClearMap();
        List<Room> rooms = value as List<Room>;
        Vector3 origin = rooms[0].transform.position;
        Vector3 screenPosition = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        if (rooms.Count < 4)
        {
            drawed = false;
            return;
        }
        
        foreach (Room room in rooms)
        {
            Vector3 relativePos = room.transform.position - origin;
            Vector3 tempPosition = new Vector3((relativePos.x / 58 * roomGapX + screenPosition.x) ,
                (relativePos.y / 40 * roomGapY + screenPosition.y));
            
            cellObjects.Add(Instantiate(cell, tempPosition, Quaternion.identity, cellList.transform));
            cellObjects[cellObjects.Count - 1].gameObject.SetActive(false);
            room.MapNumber = cellObjects.Count - 1;
            
            Vector3 roadPosition = new Vector3();
            GameObject tempObj = road_EW;
        
            if (room.Directions.Length != 1)
            {
                for (int i = 0; i < room.Directions.Length; i++)
                {
                    switch (room.Directions[i])
                    {
                        case RoomDirection.East:
                            roadPosition = new Vector3(tempPosition.x + roomGapX / 2, tempPosition.y);
                            tempObj = road_EW;
                            break;
                        case RoomDirection.North:
                            roadPosition = new Vector3(tempPosition.x, tempPosition.y + roomGapY / 2);
                            tempObj = road_SN;
                            break;
                        case RoomDirection.South:
                            roadPosition = new Vector3(tempPosition.x, tempPosition.y - roomGapY / 2);
                            tempObj = road_SN;
                            break;
                        case RoomDirection.West:
                            roadPosition = new Vector3(tempPosition.x - roomGapX / 2, tempPosition.y);
                            tempObj = road_EW;
                            break;
                    }
                    roadObjects.Add(Instantiate(tempObj, new Vector3(roadPosition.x, roadPosition.y),
                        Quaternion.identity, roadList.transform));
                    roadObjects[roadObjects.Count - 1].gameObject.SetActive(false);
                }
            }
            
            
            if(room.roomType == RoomType.Start )
            {
                RevealMap(room);
            }
        }
        
        cellObjects[0].gameObject.SetActive(true);
        cellList.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        roadList.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        drawed = true;
    }
    
    public void RevealMap(object value)
    {
        Room room = value as Room;
        

        for (int i = 0; i < cellObjects.Count; i++)
        {
            if (i == room.MapNumber)
            {
                cellObjects[i].gameObject.SetActive(true);
                
                if (room.roomType == RoomType.Shop)
                {
                    mapMarker.Add(Instantiate(shopMarker, cellObjects[i].transform.position, Quaternion.identity, cellList.transform).gameObject);
                }
                
                if (room.roomType == RoomType.BossPortal)
                {
                    mapMarker.Add(Instantiate(portalMarker, cellObjects[i].transform.position, Quaternion.identity,
                        cellList.transform).gameObject);
                }

                foreach (GameObject road in roadObjects)
                {
                    if (Vector3.Distance(cellObjects[i].transform.position, road.transform.position) < 67f)
                    {
                        //들어간 방 주위의 길을 드러냄
                        road.gameObject.SetActive(true);
                    }
                }
                
                break;
            }
        }
    }

    private void ClearMap()
    {
        if (roadObjects.Count != 0)
        {
            foreach (var roomObj in roadObjects)
            {
                if (roomObj != null)
                    Destroy(roomObj);
            }
            
            roadObjects.Clear();
        }
        
        if (cellObjects.Count != 0)
        {
            foreach (var roomObj in cellObjects)
            {
                if (roomObj != null)
                    Destroy(roomObj);
            }
            
            cellObjects.Clear();
        }

        if (mapMarker.Count != 0)
        {
            foreach (var roomObj in mapMarker)
            {
                if (roomObj != null)
                    Destroy(roomObj);
            }
            
            mapMarker.Clear();
        }
        
        
        drawed = false;
    }
    
    private void OnMiniMapPerformed(InputAction.CallbackContext ctx)
    {
        

        if (UIManager.Instance.IsUIOpen<PauseUI>())
            return;

        if (StaticPopupManager.Instance.IsShowing)
            return;

        if (UIManager.Instance.IsUIOpen<LevelSelectUI>())
            return;

        OnOffMiniMap(); // 실제 미니맵 토글 함수 호출
    }
    
    public void OnOffMiniMap()
    {
        if (!drawed)
            return;


        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            Logger.Log("리턴중");
            return;
        }


        if (canvas.activeSelf)
        {
            canvas.SetActive(false);
        }
        else
        {
            canvas.SetActive(true);
        }
    }
}
