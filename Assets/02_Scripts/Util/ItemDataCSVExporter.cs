#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class ItemDataCSVExporter : EditorWindow
{
    private string targetFolder = "Assets/09_ScriptableObject/Item";
    private string exportFileName = "Exported_ItemData.csv";

    [MenuItem("Tools/ItemData/Export Item CSV")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataCSVExporter>("Item CSV Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("ItemData CSV Exporter", EditorStyles.boldLabel);

        targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
        exportFileName = EditorGUILayout.TextField("Export File Name", exportFileName);

        GUILayout.Space(10);

        if (GUILayout.Button("Export to CSV"))
        {
            ExportToCSV();
        }
    }

    private void ExportToCSV()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { targetFolder });
        if (guids.Length == 0)
        {
            Debug.LogWarning("선택한 폴더에 ItemData SO가 없습니다.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("id,name,icon,description,itemType,price,skillSO,skillImage,runeSO,hpRecover,mpRecover");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item == null) continue;

            int idx = item.id;
            string name = item.displayName;
            string iconPath = item.icon ? AssetDatabase.GetAssetPath(item.icon) : "";
            string description = item.description?.Replace(",", " ");
            string type = item.itemType.ToString();
            string price = item.price.ToString();

            string skillSO = "", skillImage = "", runeSO = "", hp = "", mp = "";

            if (item is SkillBookData skill)
            {
                skillSO = skill.skillSO ? AssetDatabase.GetAssetPath(skill.skillSO) : "";
                skillImage = skill.skillImage ? AssetDatabase.GetAssetPath(skill.skillImage) : "";
            }
            else if (item is RuneData rune)
            {
                runeSO = rune.runeSO ? AssetDatabase.GetAssetPath(rune.runeSO) : "";
            }
            else if (item is PotionData potion)
            {
                hp = potion.hpRecover.ToString();
                mp = potion.mpRecover.ToString();
            }

            sb.AppendLine($"{idx},{name},{iconPath},{description},{type},{price},{skillSO},{skillImage},{runeSO},{hp},{mp}");
        }

        string pathSave = EditorUtility.SaveFilePanel("CSV로 내보내기", Application.dataPath, exportFileName, "csv");
        if (!string.IsNullOrEmpty(pathSave))
        {
            File.WriteAllText(pathSave, sb.ToString(), Encoding.UTF8);
            EditorUtility.DisplayDialog("완료", "CSV 내보내기 완료!", "확인");
            Debug.Log($"CSV 내보내기 성공: {pathSave}");
        }
    }
}
#endif
