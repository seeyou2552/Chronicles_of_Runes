#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class StageRewardTableImporter : EditorWindow
{
    private const string csvPath = "Assets/11_Data/StageRewardTable.csv";
    private const string assetPath = "Assets/09_ScriptableObject/StageRewardTable.asset";

    [MenuItem("Tools/StageReward/Import or Create Table")]
    public static void ShowWindow()
    {
        GetWindow<StageRewardTableImporter>("Stage Reward Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Stage Reward Table Importer", EditorStyles.boldLabel);

        if (GUILayout.Button("▶ Import StageRewardTable from CSV"))
        {
            ImportTableFromCSV();
        }

        if (GUILayout.Button("Create Sample CSV"))
        {
            CreateSampleCSV();
        }
    }

    private void ImportTableFromCSV()
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV 파일이 존재하지 않습니다: {csvPath}");
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV에 유효한 데이터가 없습니다.");
            return;
        }

        Dictionary<int, List<StageRewardEntry>> tableMap = new();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] tokens = lines[i].Split(',');

            if (tokens.Length < 3)
            {
                Debug.LogWarning($"[Line {i + 1}] 잘못된 형식: {lines[i]}");
                continue;
            }

            if (!int.TryParse(tokens[0], out int stageNumber) ||
                !int.TryParse(tokens[1], out int itemId) ||
                !float.TryParse(tokens[2], out float weight))
            {
                Debug.LogWarning($"[Line {i + 1}] 파싱 실패: {lines[i]}");
                continue;
            }

            if (!tableMap.ContainsKey(stageNumber))
                tableMap[stageNumber] = new List<StageRewardEntry>();

            tableMap[stageNumber].Add(new StageRewardEntry
            {
                itemId = itemId,
                weight = weight
            });
        }

        // ScriptableObject 생성 또는 불러오기
        StageRewardTable table = AssetDatabase.LoadAssetAtPath<StageRewardTable>(assetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<StageRewardTable>();
            AssetDatabase.CreateAsset(table, assetPath);
        }

        table.LoadFromRaw(tableMap);
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("StageRewardTable.asset 생성 또는 갱신 완료!");
    }

    private void CreateSampleCSV()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(csvPath));

        var lines = new List<string>
        {
            "Stage,ItemId,Weight",
            "1,1,10",
            "1,2,15",
            "1,3,5",
            "2,4,20",
            "2,5,10",
            "2,6,5"
        };

        File.WriteAllLines(csvPath, lines);
        AssetDatabase.Refresh();

        Debug.Log(" 샘플 StageRewardTable.csv 생성 완료!");
    }
}
#endif
