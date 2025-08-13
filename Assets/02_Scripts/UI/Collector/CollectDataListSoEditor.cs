#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CollectDataListSo))]
public class CollectDataListSoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CollectDataListSo collector = (CollectDataListSo)target;

        if (GUILayout.Button("데이터 수집"))
        {
            CollectAllData(collector);
            EditorUtility.SetDirty(collector); // 변경사항 저장
            AssetDatabase.SaveAssets();
        }
    }

    private void CollectAllData(CollectDataListSo collector)
    {
        // 경로에 맞게 찾아서 각각 리스트에 담기
        collector.skillList.skillData = LoadAssetsAtPath<SkillBookData>("Assets/09_ScriptableObject");
        collector.runeList.runeData = LoadAssetsAtPath<RuneData>("Assets/09_ScriptableObject");
        collector.itemList.itemData = LoadAssetsAtPath<PotionData>("Assets/09_ScriptableObject/Potion");
        collector.achievementList.achievementData = LoadAssetsAtPath<AchievementData>("Assets/09_ScriptableObject/Achievement");

        Debug.Log("[CollectDataListSoEditor] 데이터 수집 완료!");
    }

    private List<T> LoadAssetsAtPath<T>(string path) where T : ScriptableObject
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { path });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
                assets.Add(asset);
        }

        return assets;
    }
}
#endif