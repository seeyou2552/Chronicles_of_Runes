#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SpawnDataImporter : EditorWindow
{
    private TextAsset csvFile;
    private string savePath = "Assets/09_ScriptableObject/SpawnData";
    private Dictionary<string, EnemyDataSo> enemyDataCache;

    [MenuItem("Tools/Monster/Import Spawn CSV")]
    public static void ShowWindow() => GetWindow<SpawnDataImporter>("Spawn CSV Importer");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Spawn CSV Importer", EditorStyles.boldLabel);

        // CSV File 선택 필드
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File (Optional)", csvFile, typeof(TextAsset), false);

        // 저장 경로
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        GUILayout.Space(10);

        if (GUILayout.Button("Create Sample CSV"))
        {
            CreateSampleCSV();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Import CSV and Generate SOs"))
        {
            string csvText = null;

            if (csvFile != null)
            {
                csvText = csvFile.text;
            }
            else
            {
                // csvFile이 없으면 샘플 CSV 자동 사용 시도
                string fallbackPath = "Assets/09_ScriptableObject/SpawnData/SampleSpawnData.csv";
                csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(fallbackPath);

                if (csvFile != null)
                {
                    Debug.Log("CSV 파일이 선택되지 않아 기본 샘플 CSV를 사용합니다.");
                    csvText = csvFile.text;
                }
                else
                {
                    Debug.LogError("CSV 파일을 선택하지 않았고, 기본 샘플 CSV도 존재하지 않습니다.");
                    return;
                }
            }

            ImportCSV(csvText);
        }
    }

    private void CreateSampleCSV()
    {
        string folderPath = Application.dataPath + "/09_ScriptableObject/SpawnData";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = folderPath + "/SampleSpawnData.csv";

        string sampleContent =
@"StageKey,RoomKey,EnemyKey,SpawnCount,BossEnemyKey
Stage01,Room01,Goblin,2,
Stage01,Room01,GoblinArcher,2,
Stage01,Room02,Enemy01,3,
Stage01,,,,SkeletonMage,
Stage02,Room01,Goblin,4,";


        File.WriteAllText(filePath, sampleContent);

        Debug.Log($"샘플 CSV가 생성되었습니다: {filePath}");
        AssetDatabase.Refresh();
    }

    private void ImportCSV(string csvText)
    {
        // 1. 기존 Stages 폴더 삭제 후 재생성
        string stagesFolderPath = Path.Combine(savePath, "Stages");
        if (Directory.Exists(stagesFolderPath))
        {
            FileUtil.DeleteFileOrDirectory(stagesFolderPath);
            FileUtil.DeleteFileOrDirectory(stagesFolderPath + ".meta");
        }
        Directory.CreateDirectory(stagesFolderPath);
        AssetDatabase.Refresh();

        CacheEnemyDataSos();

        Dictionary<string, StageDataSo> stageMap = new();
        Dictionary<string, RoomSpawnDataSo> roomMap = new();

        string[] lines = csvText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        int lineIndex = 0;
        if (lines.Length > 0 && lines[0].StartsWith("StageKey"))
        {
            lineIndex = 1;
        }

        for (; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var tokens = line.Split(',');

            if (tokens.Length < 4)
            {
                Debug.LogWarning($"잘못된 CSV 라인 무시: {line}");
                continue;
            }

            string stageKey = tokens[0].Trim();
            string roomKey = tokens[1].Trim();
            string enemyKey = tokens[2].Trim();
            int spawnCount = 1;
            int.TryParse(tokens[3].Trim(), out spawnCount);
            string bossEnemyKey = tokens.Length > 4 ? tokens[4].Trim() : "";

            // 보스 데이터만 있는 라인 처리 (roomKey가 비어 있음)
            bool isBossOnlyLine = string.IsNullOrEmpty(roomKey) && !string.IsNullOrEmpty(bossEnemyKey);

            // Stage 생성 및 캐싱
            string stageFolder = $"{savePath}/Stages/{stageKey}";
            EnsureDirectory(stageFolder);
            string stagePath = $"{stageFolder}/{stageKey}.asset";

            if (!stageMap.TryGetValue(stageKey, out StageDataSo stageSo))
            {
                stageSo = AssetDatabase.LoadAssetAtPath<StageDataSo>(stagePath);
                if (stageSo == null)
                {
                    stageSo = ScriptableObject.CreateInstance<StageDataSo>();
                    stageSo.roomSpawnDataList = new List<RoomSpawnDataSo>();
                    AssetDatabase.CreateAsset(stageSo, stagePath);
                }
                stageMap[stageKey] = stageSo;
            }

            // 보스 등록만 하고 넘어감
            if (isBossOnlyLine)
            {
                if (enemyDataCache.TryGetValue(bossEnemyKey, out EnemyDataSo bossData))
                {
                    stageSo.bossEnemyData = bossData;
                    EditorUtility.SetDirty(stageSo);
                }
                else
                {
                    Debug.LogWarning($"보스 EnemyDataSo를 찾을 수 없습니다: {bossEnemyKey}");
                }
                continue;
            }

            if (!enemyDataCache.TryGetValue(enemyKey, out EnemyDataSo enemyDataSo))
            {
                Debug.LogWarning($"EnemyDataSo 없음: {enemyKey}");
                continue;
            }

            // Room 및 EnemySpawnData 생성
            string roomFolder = $"{stageFolder}/{roomKey}";
            EnsureDirectory(roomFolder);
            string roomPath = $"{roomFolder}/{roomKey}.asset";

            if (!roomMap.TryGetValue($"{stageKey}_{roomKey}", out RoomSpawnDataSo roomSo))
            {
                roomSo = AssetDatabase.LoadAssetAtPath<RoomSpawnDataSo>(roomPath);
                if (roomSo == null)
                {
                    roomSo = ScriptableObject.CreateInstance<RoomSpawnDataSo>();
                    roomSo.enemyDataSo = new List<EnemySpawnDataSo>();
                    AssetDatabase.CreateAsset(roomSo, roomPath);
                }
                roomMap[$"{stageKey}_{roomKey}"] = roomSo;
            }

            string enemySpawnFolder = $"{roomFolder}/SpawnEnemyData";
            EnsureDirectory(enemySpawnFolder);
            string enemySpawnPath = $"{enemySpawnFolder}/{enemyKey}_SpawnData.asset";

            EnemySpawnDataSo enemySpawnSo = AssetDatabase.LoadAssetAtPath<EnemySpawnDataSo>(enemySpawnPath);
            if (enemySpawnSo == null)
            {
                enemySpawnSo = ScriptableObject.CreateInstance<EnemySpawnDataSo>();
                AssetDatabase.CreateAsset(enemySpawnSo, enemySpawnPath);
            }

            enemySpawnSo.enemyDataSo = enemyDataSo;
            enemySpawnSo.spawnCnt = spawnCount;
            EditorUtility.SetDirty(enemySpawnSo);

            if (!roomSo.enemyDataSo.Contains(enemySpawnSo))
            {
                roomSo.enemyDataSo.Add(enemySpawnSo);
                EditorUtility.SetDirty(roomSo);
            }

            if (!stageSo.roomSpawnDataList.Contains(roomSo))
            {
                stageSo.roomSpawnDataList.Add(roomSo);
                EditorUtility.SetDirty(stageSo);
            }

            // 보스 지정이 있는 경우
            if (!string.IsNullOrEmpty(bossEnemyKey))
            {
                if (enemyDataCache.TryGetValue(bossEnemyKey, out EnemyDataSo bossData))
                {
                    stageSo.bossEnemyData = bossData;
                    EditorUtility.SetDirty(stageSo);
                }
                else
                {
                    Debug.LogWarning($"보스 EnemyDataSo를 찾을 수 없습니다: {bossEnemyKey}");
                }
            }
        }

        CreateOrUpdateSpawnDataSo(stageMap.Values);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CSV 기반 SO 생성 및 SpawnDataSo 등록 완료!");

        
    }

    private void CreateOrUpdateSpawnDataSo(IEnumerable<StageDataSo> stages)
    {
        string spawnDataPath = $"{savePath}/SpawnDataSo.asset";

        SpawnData spawnDataSo = AssetDatabase.LoadAssetAtPath<SpawnData>(spawnDataPath);
        if (spawnDataSo == null)
        {
            spawnDataSo = ScriptableObject.CreateInstance<SpawnData>();
            AssetDatabase.CreateAsset(spawnDataSo, spawnDataPath);
        }

        if (spawnDataSo.stageDataList == null)
            spawnDataSo.stageDataList = new List<StageDataSo>();
        else
            spawnDataSo.stageDataList.Clear();

        foreach (var stage in stages)
        {
            spawnDataSo.stageDataList.Add(stage);
        }

        EditorUtility.SetDirty(spawnDataSo);
        AssignSpawnDataToPrefab(spawnDataSo);
    }

    private void CacheEnemyDataSos()
    {
        enemyDataCache = new Dictionary<string, EnemyDataSo>();

        string[] guids = AssetDatabase.FindAssets("t:EnemyDataSo", new[] { savePath });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyDataSo enemyData = AssetDatabase.LoadAssetAtPath<EnemyDataSo>(path);
            if (enemyData != null && !string.IsNullOrEmpty(enemyData.enemyName))
            {
                if (!enemyDataCache.ContainsKey(enemyData.enemyName))
                    enemyDataCache.Add(enemyData.enemyName, enemyData);
            }
        }

        Debug.Log($"EnemyDataSo 캐싱 완료: {enemyDataCache.Count}개");
    }

    private void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            var found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    private void AssignSpawnDataToPrefab(SpawnData spawnDataSo)
    {
        string prefabPath = "Assets/03_Prefabs/Map/MapManager.prefab";

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError("프리팹을 불러오지 못했습니다: " + prefabPath);
            return;
        }

        Transform enemySpawnManagerTr = FindChildRecursive(prefabRoot.transform, "EnemySpawnManager");
        if (enemySpawnManagerTr == null)
        {
            Debug.LogError("EnemySpawnManager 오브젝트를 찾을 수 없습니다 (MapManager 프리팹 안)");
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return;
        }

        var spawnManager = enemySpawnManagerTr.GetComponent<EnemySpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("EnemySpawnManager 컴포넌트를 찾을 수 없습니다.");
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return;
        }

        spawnManager.spawnData = spawnDataSo;
        EditorUtility.SetDirty(spawnManager);
        EditorUtility.SetDirty(prefabRoot);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        AssetDatabase.SaveAssets();
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("SpawnDataSo가 MapManager 프리팹 내 EnemySpawnManager에 성공적으로 할당되었습니다.");
    }
}
#endif