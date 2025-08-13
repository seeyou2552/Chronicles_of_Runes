#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class ItemDataCSVImporter : EditorWindow
{
    private TextAsset csvFile;
    private string outputPath = "Assets/09_ScriptableObject/Item";
    private bool updateExisting = true;

    [MenuItem("Tools/ItemData/Import Item CSV")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataCSVImporter>("Item CSV Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("ItemData CSV Importer", EditorStyles.boldLabel);

        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        updateExisting = EditorGUILayout.Toggle("Update Existing Assets", updateExisting);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Sample CSV"))
        {
            GenerateSampleCSV();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Import Items from CSV"))
        {
            ImportFromCSV();
        }
    }

    private void GenerateSampleCSV()
    {
        string sampleCSV =
            "id,name,icon,description,itemType,price,skillSO,skillImage,runeSO,hpRecover,mpRecover\n";

        string path = EditorUtility.SaveFilePanel("Save Sample CSV", "Assets", "Sample_ItemData", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, sampleCSV);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "샘플 CSV가 저장되었습니다.", "확인");
        }
    }

    private void ImportFromCSV()
    {
        List<ItemData> createdItems = new List<ItemData>();
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("오류", "CSV 파일을 선택하세요!", "확인");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        if (lines.Length < 2)
        {
            Debug.LogError("CSV 데이터가 없습니다.");
            return;
        }

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        int created = 0, updated = 0, skipped = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            Debug.Log($"[Row {i + 1}] 처리 중: {line}");

            List<string> values = SplitCSVLine(line);
            if (values.Count < 5)
            {
                Debug.LogWarning($"[Row {i + 1}] 값이 부족하여 건너뜁니다.");
                continue;
            }

            int id = int.Parse(values[0]);
            string name = values[1].Trim();
            string iconPath = values[2].Trim();
            string description = values[3].Trim();
            string itemTypeStr = values[4].Trim();
            int price = int.TryParse(values[5], out var p) ? p : 0;

            if (!System.Enum.TryParse<ItemType>(itemTypeStr, out var itemType))
            {
                Debug.LogError($"[Row {i + 1}] itemType 파싱 실패: '{itemTypeStr}'");
                continue;
            }

            Sprite icon = LoadAssetAtPath<Sprite>(iconPath);
            if (icon == null)
                Debug.LogWarning($"[Row {i + 1}] 아이콘 로드 실패: {iconPath}");

            string assetPath = Path.Combine(outputPath, name + ".asset");
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            bool isNew = item == null;

            if (isNew && !updateExisting)
            {
                Debug.Log($"[Row {i + 1}] 기존 SO 없음 + 업데이트 모드 꺼짐 → 스킵");
                skipped++;
                continue;
            }

            if (isNew)
            {
                switch (itemType)
                {
                    case ItemType.SkillBook: item = ScriptableObject.CreateInstance<SkillBookData>(); break;
                    case ItemType.Rune: item = ScriptableObject.CreateInstance<RuneData>(); break;
                    case ItemType.Potion: item = ScriptableObject.CreateInstance<PotionData>(); break;
                }
                Debug.Log($"[Row {i + 1}] 새 SO 생성: {itemType}");
            }

            if (item == null)
            {
                Debug.LogError($"[Row {i + 1}] ScriptableObject 생성 실패 - itemType: {itemType}");
                continue;
            }

            item.id = id;
            item.displayName = name;
            item.icon = icon;
            item.description = description;
            item.itemType = itemType;
            item.price = price;

            if (item is SkillBookData skillItem && values.Count > 6)
            {
                skillItem.skillSO = LoadAssetAtPath<Skill>(values[6].Trim());
                skillItem.skillImage = LoadAssetAtPath<Sprite>(values[7].Trim());

                if (skillItem.skillSO == null)
                    Debug.LogWarning($"[Row {i + 1}] SkillSO 로드 실패: {values[6].Trim()}");

                if (skillItem.skillImage == null)
                    Debug.LogWarning($"[Row {i + 1}] SkillImage 로드 실패: {values[7].Trim()}");
            }
            else if (item is RuneData runeItem && values.Count > 7)
            {
                runeItem.runeSO = LoadAssetAtPath<Rune>(values[8].Trim());
                if (runeItem.runeSO == null)
                    Debug.LogWarning($"[Row {i + 1}] RuneSO 로드 실패: {values[8].Trim()}");
            }
            else if (item is PotionData potionItem)
            {
                if (values.Count > 8) potionItem.hpRecover = float.TryParse(values[9], out var hp) ? hp : 0f;
                if (values.Count > 9) potionItem.mpRecover = float.TryParse(values[10], out var mp) ? mp : 0f;
                Debug.Log($"[Row {i + 1}] Potion 설정: hpRecover={potionItem.hpRecover}, mpRecover={potionItem.mpRecover}");
            }

            if (isNew)
            {
                AssetDatabase.CreateAsset(item, assetPath);
                Debug.Log($"[Row {i + 1}] SO 생성 완료: {assetPath}");
                created++;
                createdItems.Add(item);
            }
            else
            {
                EditorUtility.SetDirty(item);
                Debug.Log($"[Row {i + 1}] SO 업데이트 완료: {assetPath}");
                updated++;
                createdItems.Add(item);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        GenerateItemTable(createdItems);

        EditorUtility.DisplayDialog("완료", $"Item SO 생성 완료\n새로 생성: {created}\n업데이트: {updated}\n건너뜀: {skipped}", "확인");
    }

    private T LoadAssetAtPath<T>(string assetPath) where T : UnityEngine.Object
    {
        if (!assetPath.StartsWith("Assets/"))
        {
            Debug.LogWarning($"경로가 잘못되었습니다: {assetPath}");
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<T>(assetPath);
    }

    private static List<string> SplitCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    currentField += '\"';
                    i++; // skip the escaped quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        result.Add(currentField);
        return result;
    }
    
    private void GenerateItemTable(List<ItemData> allItems)
    {
        var table = ScriptableObject.CreateInstance<ItemTable>();
        table.items = allItems;

        string savePath = "Assets/09_ScriptableObject/ItemTable.asset";
        AssetDatabase.CreateAsset(table, savePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"ItemTable 생성 완료: {savePath}");
    }
}
#endif
