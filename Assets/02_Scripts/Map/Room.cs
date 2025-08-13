using UnityEngine;

public class Room : MonoBehaviour
{
    public PolygonCollider2D floorCollider;
    public GameObject door;
    public GameObject hole;
    public RoomShape roomShape;
    public RoomType roomType;

    public GameObject bossPortal;

    private  RoomDirection[] directions;
    private bool isEntered = false;
    private int mapNumber;
    
    private void OnEnable()
    {
        EventBus.Subscribe("PlayerClearRoom", PlayerClearRoom);
    }
    
    void OnDisable()
    {
        EventBus.Unsubscribe("PlayerClearRoom", PlayerClearRoom);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(isEntered) return;
            EventBus.Publish("FirstTouch", this);
        
        
            if ((roomType != RoomType.Normal && roomType != RoomType.Boss)) return;
            
            /*
            GridGraph gridGraph = AstarPath.active.data.gridGraph;

            gridGraph.center = new Vector3(transform.position.x + 6, transform.position.y - 9, transform.position.z);

            AstarPath.active.Scan();*/

            DoorClose();
            //RoomGimmickManager.Instance.StartGimmick(this);
            isEntered = true;
            EventBus.Publish("PlayerEnterRoom", this);
            EventBus.Publish("DoorClose", this);
        }
    }

    private void PlayerClearRoom(object value)
    {
        EventBus.Publish("DoorOpen", this);
        DoorOpen();
        //RoomGimmickManager.Instance.StopGimmick(this);
        if (roomType == RoomType.Boss)
        {
            if (bossPortal != null)
                bossPortal.SetActive(true);
            /*
            Vector3 portalPos = floorCollider.bounds.center;
            Instantiate(bossPortal);
            bossPortal.transform.position = portalPos;
            */
        }
    }

    public RoomDirection[] Directions
    {
        get { return directions; }
        set { directions = value; }
    }

    public int MapNumber
    {
        get { return mapNumber; }
        set { mapNumber = value; }
    }
    
    private void DoorOpen()
    {
        if (door != null)
        {
            door.SetActive(false);
        }
    }
    
    private void DoorClose()
    {
        if (door != null)
        {
            door.SetActive(true);
        }
    }
}
