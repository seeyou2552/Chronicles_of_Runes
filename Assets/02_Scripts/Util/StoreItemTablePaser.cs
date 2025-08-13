
// StoreItemInfo.cs
using UnityEngine;
// StoreItemLoader.cs
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

[System.Serializable]
public class StoreItemInfo
{
    public int itemId;
    public string itemName;
    public float chance;
    public int minCount;
    public int maxCount;
}


public static class StoreItemLoader
{
    public static List<StoreItemInfo> LoadStoreItems(string csvPath="Item/Store/StoreItemRates")
    {
        List<StoreItemInfo> items = new List<StoreItemInfo>();

        TextAsset csvFile = Resources.Load<TextAsset>(csvPath);
        if (csvFile == null)
        {
            Debug.LogError($"[StoreItemLoader] CSV 파일을 찾을 수 없습니다: Resources/{csvPath}.csv");
            return items;
        }

        using (StringReader reader = new StringReader(csvFile.text))
        {
            bool isFirstLine = true;
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                if (isFirstLine) { isFirstLine = false; continue; } // skip header
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] tokens = line.Split(',');
                if (tokens.Length < 5) continue;

                StoreItemInfo info = new StoreItemInfo
                {
                    itemId = int.Parse(tokens[0]),
                    itemName = tokens[1].Trim(),
                    chance = float.Parse(tokens[2]),
                    minCount = int.Parse(tokens[3]),
                    maxCount = int.Parse(tokens[4]),
                };

                items.Add(info);
            }
        }

        Debug.Log($"[StoreItemLoader] 상점 아이템 로드 완료. 총 {items.Count}개");
        return items;
    }


#if UNITY_EDITOR
    [MenuItem("Tools/Shop/Generate Sample StoreItem CSV")]
    public static void GenerateSampleCSV()
    {
        string folderPath = Application.dataPath + "/Resources/Item/Store/";
        string filePath = folderPath + "StoreItemRates.csv";

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("itemId,itemName,chance,minCount,maxCount");

        // 샘플 데이터 (원하는 만큼 수정 가능)
        // sb.AppendLine("0,HpPotion_L,0.3,1,2");
        // sb.AppendLine("1,MpPotion_M,0.2,1,3");
        // sb.AppendLine("2,MagicFloor,0.15,1,1");
        // sb.AppendLine("3,DarkRune,0.25,1,2");

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

        Debug.Log($"[StoreItemCSVGenerator] 샘플 CSV 생성 완료: {filePath}");
        AssetDatabase.Refresh();
    }
#endif
}

