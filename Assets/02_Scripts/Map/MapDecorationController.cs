using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class MapDecorationController : MonoBehaviour
{
    public List<GameObject> dungeonDecorations = new List<GameObject>();
    public List<GameObject> obstacle = new List<GameObject>();
    public GameObject bossRoomPortal;
    public GameObject shopNPC;
    public GameObject shopDecoration;
    public GameObject spawnedDecorationList;
    
    private List<GameObject> decorationGroupList = new List<GameObject>();
    private List<DecorationScriptList> decorationScriptList = new List<DecorationScriptList>();
    private List<TilemapRenderer> tilemapRendererList = new List<TilemapRenderer>();

    public void CreateDecoration(Vector3 position) // 맵 장식을 방 모양에 따라 생성
    {
        decorationGroupList.Add(Instantiate(dungeonDecorations[Random.Range(0, dungeonDecorations.Count)], position, Quaternion.identity,
            spawnedDecorationList.transform));
        decorationScriptList.Add(decorationGroupList[(decorationGroupList.Count - 1)].GetComponent<DecorationScriptList>());
        
        foreach (TilemapRenderer decoration in decorationScriptList[decorationScriptList.Count-1].tilemapRendererList)
        {
            tilemapRendererList.Add(decoration);
        }
    }

    public void CreateObstacle(Room room)
    {
        PolygonCollider2D floorCollider = room.floorCollider;
        if (floorCollider == null) return;
        int obstacleCount;
        int count = 0;
        
        Bounds bounds = floorCollider.bounds;
        if(room.roomShape == RoomShape.Cell)
            obstacleCount = Random.Range(3, 6);
        else
            obstacleCount = Random.Range(0, 4);
        

        float minEdgeDistance = 5f; // 문 근처 거리 제한
        float minDistanceBetweenObstacles = 10f; // 장애물 간 최소 거리
        int maxTries = 20;

        List<Vector3> placedPositions = new List<Vector3>();

        while (count < obstacleCount)
        {
            bool placed = false;

            for (int i = 0; i < maxTries; i++)
            {
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float y = Random.Range(bounds.min.y, bounds.max.y);
                Vector3 randomPos = new Vector3(x, y, 0);

                // 콜라이더 내부 & 문 근처 피하기
                if (!floorCollider.OverlapPoint(randomPos)) continue;
                if (IsNearEdge(randomPos, bounds, minEdgeDistance)) continue;
                if (IsTooCloseToOtherObstacles(randomPos, placedPositions, minDistanceBetweenObstacles)) continue;

                // 생성
                GameObject obj = Instantiate(obstacle[Random.Range(0, obstacle.Count)], randomPos, Quaternion.identity, spawnedDecorationList.transform);
                tilemapRendererList.Add(obj.GetComponent<TilemapRenderer>());
                placedPositions.Add(randomPos);
                placed = true;
                break;
            }

            if (placed) count++;
            else break;
        }
    }
    
    private bool IsNearEdge(Vector3 pos, Bounds bounds, float threshold)
    {
        return
            Mathf.Abs(pos.x - bounds.min.x) < threshold ||
            Mathf.Abs(pos.x - bounds.max.x) < threshold ||
            Mathf.Abs(pos.y - bounds.min.y) < threshold ||
            Mathf.Abs(pos.y - bounds.max.y) < threshold;
    }
    
    private bool IsTooCloseToOtherObstacles(Vector3 pos, List<Vector3> placed, float minDistance)
    {
        foreach (var p in placed)
        {
            if ((pos - p).sqrMagnitude < minDistance * minDistance)
                return true;
        }
        return false;
    }
    
    public void CreateBossRoomPortal(Vector3 position)  //보스방에 포탈 생성
    {
        tilemapRendererList.Add(Instantiate(bossRoomPortal,new Vector3(position.x+5, position.y-8, 0), Quaternion.identity, spawnedDecorationList.transform).GetComponent<TilemapRenderer>());
    }

    public void RepositionShopNPC(Vector3 position, string sceneName)  //상점방에 상인을 이동시킴
    {
        if (sceneName != "BossScene")
        {
            shopNPC.transform.position = new Vector3(position.x + 5, position.y - 8, 0);
            Instantiate(shopDecoration, new Vector3(position.x + 5, position.y - 8, 0), Quaternion.identity, spawnedDecorationList.transform);
            // tilemapRendererList.Add(Instantiate(shopDecoration, new Vector3(position.x + 5, position.y - 8, 0),
            //     Quaternion.identity, spawnedDecorationList.transform).GetComponent<TilemapRenderer>());
        }
        else
            shopNPC.gameObject.SetActive(false);
    }
    
    public void ResetDecorations()  //리셋
    {
        if (decorationGroupList.Count > 0)
        {
            foreach (var roomObj in decorationGroupList)
            {
                if (roomObj != null)
                    Destroy(roomObj);
            }
            
            decorationGroupList.Clear();
        }
    }

    void LateUpdate()  //플레이어와 장식물의 레이어 조정
    {
        Vector3 playerVector = PlayerController.Instance.gameObject.transform.position;
        foreach (TilemapRenderer decoration in tilemapRendererList)
        {
            if (playerVector.y > decoration.gameObject.transform.position.y)
            {
                decoration.sortingOrder = 105;
            }
            else 
            {
                decoration.sortingOrder = 74;
            }
        }
    }
}
