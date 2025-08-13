using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class EnemySpawnManager : MonoBehaviour
{
    //임시 나중에 매니저에서 가져오는느낌으로 가야할듯?
    public int stageIndex = 0;
    public GameObject spawnedEnemyList;
    public List<GameObject> spawnedEnemies = new List<GameObject>();
    public SpawnData spawnData;
    public GameObject crate;

    public List<string> spawnSFXList;
    
    private List<Room> spawnedPositions = new List<Room>();

    private List<EnemyDataSo> enemyList = new List<EnemyDataSo>();

    private int leftEnemyCount;
    private Room room;

    int poolPrefabCnt = 20;
    int poolVFXCnt = 10;
    int poolProjectileCnt = 20;
    GameObject go;

    public GameObject spawnerPrefab;

    private void OnEnable()
    {
        EventBus.Subscribe("PlayerEnterRoom", PlayerEnteredRoom);
        EventBus.Subscribe("MonsterDead", LeftEnemyCount);
        EventBus.Subscribe(LoadState.LoadComplete, CallCreatePoolAsync);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("PlayerEnterRoom", PlayerEnteredRoom);
        EventBus.Unsubscribe("MonsterDead", LeftEnemyCount);
        EventBus.Unsubscribe(LoadState.LoadComplete, CallCreatePoolAsync);
    }


    private async void Start()
    {
        //1회만 생성할것들
        if (spawnData == null) { return; }

        ObjectPoolManager.Instance.CreatePool("Spawner", spawnerPrefab, 15);
        await UniTask.Yield();
        //흠
        GameObject tempGo = new GameObject("TargetPos");
        if (!ObjectPoolManager.Instance.pools.ContainsKey(tempGo.name))
        {
            ObjectPoolManager.Instance.CreatePool("TargetPos", tempGo, 30);
            await UniTask.Yield();
        }
        if (!ObjectPoolManager.Instance.pools.ContainsKey(WarningEffect.Instance.lineRendererPrefab.name))
        {
            ObjectPoolManager.Instance.CreatePool("LineWarningEffect", WarningEffect.Instance.lineRendererPrefab, 25);
            await UniTask.Yield();
        }
        if (!ObjectPoolManager.Instance.pools.ContainsKey(WarningEffect.Instance.CircleEffectPrefab.name))
        {
            ObjectPoolManager.Instance.CreatePool("CircleWarningEffect", WarningEffect.Instance.CircleEffectPrefab, 10);
            await UniTask.Yield();
        }
        //임시
        Destroy(tempGo);


        //흠 필요한가?

        foreach (var stage in spawnData.stageDataList)
        {
            if (stage.bossEnemyData != null)
            {
                if (ObjectPoolManager.Instance.pools == null || !ObjectPoolManager.Instance.pools.ContainsKey(stage.bossEnemyData.enemyPrefab.name))
                {
                    ObjectPoolManager.Instance.CreatePool(stage.bossEnemyData.enemyPrefab.name, stage.bossEnemyData.enemyPrefab, 1);
                }

                //특수몹기준
                //패턴 수행용 드론
                PatternDroneContainer patternDroneContainer = stage.bossEnemyData.enemyPrefab.GetComponent<PatternDroneContainer>();
                if (patternDroneContainer != null)
                {
                    foreach (var drone in patternDroneContainer.droneList)
                    {
                        if (!ObjectPoolManager.Instance.pools.ContainsKey(drone.name))
                        {
                            ObjectPoolManager.Instance.CreatePool(drone.name, drone, 5);
                        }
                    }
                }
            }
        }

    }

    public void CreatePool(object obj)
    {
        //해당 스테이지까지만 돌리기

        var stage = spawnData.stageDataList[GameManager.Instance.sceneFlowManager.currentStageIndex - 1];


            foreach (var room in stage.roomSpawnDataList)
            {
                foreach (var enemy in room.enemyDataSo)
                {
                    if (enemy.enemyDataSo == null || enemy.enemyDataSo.enemyPrefab == null)
                    {
                        Logger.Log("EnemyDataSo 또는 EnemyPrefab이 null입니다.");
                    }

                    string enemyKey = enemy.enemyDataSo.enemyPrefab.name;

                    if (ObjectPoolManager.Instance.pools == null || !ObjectPoolManager.Instance.pools.ContainsKey(enemyKey))
                    {

                        ObjectPoolManager.Instance.CreatePool(enemyKey, enemy.enemyDataSo.enemyPrefab, poolPrefabCnt);

                    }

                    /*
                    go = ObjectPoolManager.Instance.Get(enemyKey);

                    go.transform.position = new Vector3(300, 300, 300);

                    var enemyController = go.GetComponent<EnemyController>();
                    */


                    var enemyController = enemy.enemyDataSo.enemyPrefab.GetComponent<EnemyController>();
                if (enemyController != null && enemyController.attackPatternList != null)
                {
                    foreach (var pattern in enemyController.attackPatternList)
                    {
                        if (pattern == null) continue;

                        if (pattern.VFX != null)
                        {
                            string vfxKey = pattern.VFX.name;
                            if (!ObjectPoolManager.Instance.pools.ContainsKey(vfxKey))
                            {
                                ObjectPoolManager.Instance.CreatePool(vfxKey, pattern.VFX, poolVFXCnt);
                            }
                        }

                        if (pattern.projectileSo != null && pattern.projectileSo.obj != null)
                        {
                            string projectileKey = pattern.projectileSo.obj.name;
                            if (!ObjectPoolManager.Instance.pools.ContainsKey(projectileKey))
                            {
                                ObjectPoolManager.Instance.CreatePool(projectileKey, pattern.projectileSo.obj, poolProjectileCnt);
                            }
                        }
                    }
                }

                
            }
        }
    }

    public async void CallCreatePoolAsync(object obj)
    {
        await CreatePoolAsync();
    }

    //몬스터 생성시마다 프레임을 넘기다보니 갯수 많으면 프레임 많이 밀릴수도
    //풀 생성 전 입장하는거 주의해야하긴함 근데 몬스터만이면 그렇게 안밀릴듯
    public async UniTask CreatePoolAsync()
    {
        var stage = spawnData.stageDataList[GameManager.Instance.sceneFlowManager.currentStageIndex - 1];

        foreach (var room in stage.roomSpawnDataList)
        {
            foreach (var enemy in room.enemyDataSo)
            {
                string enemyKey = enemy.enemyDataSo.enemyPrefab.name;
                if (!ObjectPoolManager.Instance.pools.ContainsKey(enemyKey))
                {
                    ObjectPoolManager.Instance.CreatePool(enemyKey, enemy.enemyDataSo.enemyPrefab, poolPrefabCnt);
                    var enemyController = enemy.enemyDataSo.enemyPrefab.GetComponent<EnemyController>();

                    if (enemyController != null && enemyController.attackPatternList != null)
                    {
                        foreach (var pattern in enemyController.attackPatternList)
                        {
                            if (pattern == null) continue;

                            if (pattern.VFX != null)
                            {
                                string vfxKey = pattern.VFX.name;
                                if (!ObjectPoolManager.Instance.pools.ContainsKey(vfxKey))
                                {
                                    ObjectPoolManager.Instance.CreatePool(vfxKey, pattern.VFX, poolVFXCnt);
                                }
                            }

                            if (pattern.projectileSo != null && pattern.projectileSo.obj != null)
                            {
                                string projectileKey = pattern.projectileSo.obj.name;
                                if (!ObjectPoolManager.Instance.pools.ContainsKey(projectileKey))
                                {
                                    ObjectPoolManager.Instance.CreatePool(projectileKey, pattern.projectileSo.obj, poolProjectileCnt);
                                }
                            }
                        }
                    }
                    await UniTask.Yield();
                }   
            }
        }
    }

    private async void PlayerEnteredRoom(object value)  //플레이어가 방에 들어오면 호출됨
    {
        room = value as Room;
        if (room == null || spawnedPositions.Contains(room)) return;

        if (room.roomType == RoomType.Boss)  // 입장한 방이 보스방일시
        {
            PlayerEnteredBossRoom(room);
            return;
        }

        spawnedPositions.Add(room);

        Vector2 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        float minDistance = 8.0f;  // 원하는 거리 이상 떨어져야 스폰


        //if (spawnData.spawnGroups == null || spawnData.spawnGroups.Count == 0) return;
        RoomSpawnDataSo tempMonsters = spawnData.stageDataList[GameManager.Instance.sceneFlowManager.currentStageIndex - 1].roomSpawnDataList[Random.Range(0, spawnData.stageDataList[GameManager.Instance.sceneFlowManager.currentStageIndex - 1].roomSpawnDataList.Count)];

        //따로 카운트
        foreach (EnemySpawnDataSo mon in tempMonsters.enemyDataSo)
        {
            for (int i = 0; i < mon.spawnCnt; i++)
             leftEnemyCount++; 
        }
                
        foreach (EnemySpawnDataSo mon in tempMonsters.enemyDataSo)
        {
            for (int i = 0; i < mon.spawnCnt; i++)
            {
                if (mon.enemyDataSo == null) continue;

                Vector2 spawnPos = GetRandomPointInPolygon(room.floorCollider, playerPos, minDistance);
                GameObject spawner = ObjectPoolManager.Instance.Get("Spawner");
                spawner.transform.position = spawnPos;


                SoundManager.PlaySFX(spawnSFXList[Random.Range(0, spawnSFXList.Count)].ToString());
                spawner.GetComponent<EnemySpawnEffect>().CircleSpawn(
                    spawnPos,
                    () =>
                    {
                        GameObject go = ObjectPoolManager.Instance.Get(mon.enemyDataSo.enemyPrefab.name);
                        go.transform.position = spawnPos;
                        go.transform.rotation = Quaternion.identity;

                        //하단 피벗 맞춤
                        AlignBottomPivot(go, spawnPos);

                        return go;
                    },
                    (spawnedEnemy) =>
                    {
                        EnemyController enemy = spawnedEnemy.GetComponent<EnemyController>();
                        enemy.room = room.transform;
                        enemy.roomCollider = room.floorCollider;
                        enemy.ConvertSoToStat(mon.enemyDataSo);
                        enemy.isDead = false;
                        GameManager.Instance.ChangeMonsterStat(enemy.enemyStat);
                        
                    }
                );

                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }
        }
    }

    private void AlignBottomPivot(GameObject go, Vector3 spawnPos)
    {
        Collider2D col = go.GetComponent<Collider2D>();
        if (col == null) return;

        // 로컬 크기 (회전 X 전제)
        Vector3 localSize = col.bounds.size;
        float halfHeight = localSize.y / 2f;


        float offset = 0.2f;
        // pivot을 하단으로 이동시키기 위해 y만 보정
        go.transform.position = spawnPos + new Vector3(0, halfHeight + offset, 0);
    }



    private void PlayerEnteredBossRoom(Room room)
    {
        //bossData = 보스들의 EnemyDataSo 리스트, room = 입장한 보스방 스크립트
        GameObject go = ObjectPoolManager.Instance.Get(spawnData.stageDataList[GameManager.Instance.sceneFlowManager.currentStageIndex - 1].bossEnemyData.enemyPrefab.name);
        Logger.Log(go.name);
        go.transform.position = room.floorCollider.bounds.center + Vector3.right * (room.floorCollider.bounds.size.x / 4f);




        EnemyController enemy = go.GetComponent<EnemyController>();
        enemy.room = room.transform;
        enemy.roomCollider = room.floorCollider;
        enemy.ConvertSoToStat(spawnData.stageDataList[GameManager.Instance.sceneFlowManager.currentStageIndex - 1].bossEnemyData);
        GameManager.Instance.ChangeMonsterStat(enemy.enemyStat);
        EventBus.Publish(EventType.BossSpawnCinematic, null);

    }
    
    private Vector2 GetRandomPointInPolygon(PolygonCollider2D poly, Vector2 playerPos, float minDistance)  //콜라이더 내 랜덤 좌표 리턴
    {
        Vector2 min = poly.bounds.min;
        Vector2 max = poly.bounds.max;

        for (int i = 0; i < 100; i++)
        {
            Vector2 point = new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y)
            );

            if (poly.OverlapPoint(point) && (playerPos - point).sqrMagnitude >= minDistance * minDistance)  // 원하는 거리 이상 떨어져야 스폰
                return point;
        }
        
        return poly.bounds.center;
    }

    public void ResetEnemySpawnManager()  //만들어진 에너미 스포너 삭제
    {
        if (spawnedEnemies.Count > 0)
        {
            foreach (var roomObj in spawnedEnemies)
            {
                if (roomObj != null)
                    Destroy(roomObj);
            }
            spawnedEnemies.Clear();
        }
    }

    public void LeftEnemyCount(object value) //남은 적의 수를 카운트, 0이하가 되면 클리어 판정
    {
        leftEnemyCount--;
        if (leftEnemyCount <= 0)
        {
            if (value is GameObject point && point != null)
            {
                EventBus.Publish("PlayerClearRoom", this);
                if(SceneManager.GetActiveScene().name == "BossScene")
                    Instantiate(crate, new Vector3(59, -8, 0), Quaternion.identity);
                else
                {
                    //Vector3 center = room.floorCollider.bounds.center; //point.transform.position;
                    Instantiate(crate, point.transform.position, Quaternion.identity); 
                }
                    
            }
            else
            {

            }

            EventBus.Publish("PlayerClearRoom", this);
        }
    }
    
    // void Update()
    // {
    //     if (spawnedEnemies.Count <= 0)
    //     {
    //         EventBus.Publish("PlayerClearRoom", this);
    //     }
    // }
}
