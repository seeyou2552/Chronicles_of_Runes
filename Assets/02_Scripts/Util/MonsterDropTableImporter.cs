#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MonsterDropTableImporter
{
    private const string inputCsvPath = "Assets/11_Data/MonsterDropTable.csv";
    private const string outputAssetPath = "Assets/09_ScriptableObject/MonsterDropTable.asset";

        [MenuItem("Tools/Monster/Make Monster Drop Table (Single Asset)")]
    public static void ImportDropTables()
    {
        if (!File.Exists(inputCsvPath))
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + inputCsvPath);
            return;
        }

        string[] lines = File.ReadAllLines(inputCsvPath);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 데이터가 없습니다.");
            return;
        }

        Dictionary<string, MonsterDrop> monsterMap = new();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] tokens = lines[i].Split(',');

            if (tokens.Length != 4)
            {
                Debug.LogWarning($"[Line {i}] 잘못된 형식: {lines[i]}");
                continue;
            }

            string monsterName = tokens[0].Trim();
            int itemId = int.Parse(tokens[1].Trim());
            float chance = float.Parse(tokens[2].Trim());

            if (!monsterMap.TryGetValue(monsterName, out var monsterDrop))
            {
                monsterDrop = new MonsterDrop();
                monsterDrop.MonsterName = monsterName;
                monsterDrop.ItemDropList = new List<ItemDropData>();

                if (int.TryParse(tokens[3].Trim(), out int dropGold))
                    monsterDrop.DropGold = dropGold;
                else
                    monsterDrop.DropGold = 0;

                monsterMap[monsterName] = monsterDrop;
            }

            monsterDrop.ItemDropList.Add(new ItemDropData
            {
                itemId = itemId,
                chance = chance
            });
        }

        // 최종 테이블 생성
        MonsterDropTable dropTable = ScriptableObject.CreateInstance<MonsterDropTable>();
        dropTable.MonsterDropList = new List<MonsterDrop>(monsterMap.Values);

        // 디렉토리 생성
        string folder = Path.GetDirectoryName(outputAssetPath);
        Directory.CreateDirectory(folder);

        // 기존 파일 있으면 삭제
        if (File.Exists(outputAssetPath))
        {
            AssetDatabase.DeleteAsset(outputAssetPath);
        }

        AssetDatabase.CreateAsset(dropTable, outputAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✔ MonsterDropTable.asset 생성 완료 ({dropTable.MonsterDropList.Count}종 몬스터)");
    }

    [MenuItem("Tools/Monster/Create Sample MonsterDropTable.csv")]
    public static void CreateSampleCSV()
    {
        string dir = Path.GetDirectoryName(inputCsvPath);
        Directory.CreateDirectory(dir);

        string[] sampleLines = new[]
        {
            "MonsterName,ItemId,Chance,DropGold"
        };

        File.WriteAllLines(inputCsvPath, sampleLines);
        AssetDatabase.Refresh();

        Debug.Log($"샘플 CSV 파일이 생성되었습니다: {inputCsvPath}");
    }
}
#endif
