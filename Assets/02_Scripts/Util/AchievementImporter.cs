#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class AchievementDataImporter : EditorWindow
{
    private TextAsset csvFile;
    private string outputPath = "Assets/09_ScriptableObject/Achievement";

    [MenuItem("Tools/Import Achievement CSV")]
    public static void ShowWindow()
    {
        GetWindow<AchievementDataImporter>("Achievement CSV Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV → AchievementData Generator", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        if (GUILayout.Button("Import"))
        {
            if (csvFile == null)
            {
                Debug.LogError("CSV 파일을 선택하세요.");
                return;
            }

            ImportCSV(csvFile.text);
        }
    }

    private void ImportCSV(string csvText)
    {
        string[] lines = csvText.Split('\n');
        if (lines.Length <= 1) return;

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 3)
            {
                Debug.LogWarning($"잘못된 줄: {line}");
                continue;
            }

            string iconPath = values[0].Trim();
            string displayName = values[1].Trim();
            string description = values[2].Trim();

            AchievementData data = ScriptableObject.CreateInstance<AchievementData>();

            Sprite icon = Resources.Load<Sprite>(iconPath);
            if (icon == null)
                Debug.LogWarning($"아이콘을 찾을 수 없습니다: {iconPath}");

            typeof(AchievementData).GetField("icon", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(data, icon);
            typeof(AchievementData).GetField("iconPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(data, iconPath);
            data.displayName = displayName;
            data.description = description;

            string assetName = $"{displayName.Replace(" ", "_")}.asset";
            string fullPath = Path.Combine(outputPath, assetName);
            AssetDatabase.CreateAsset(data, fullPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("AchievementData 생성 완료!");
    }
}
#endif
