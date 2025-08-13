#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(SpawnData))]
public class SpawnDataEditor : Editor
{
    private bool showStages = true;

    private List<bool> stageFoldouts = new List<bool>();
    private List<List<bool>> roomFoldouts = new List<List<bool>>();
    private List<List<List<bool>>> enemyFoldouts = new List<List<List<bool>>>();

    public override void OnInspectorGUI()
    {
        SpawnData spawnData = (SpawnData)target;

        base.OnInspectorGUI();

        if (spawnData.stageDataList == null || spawnData.stageDataList.Count == 0)
        {
            EditorGUILayout.HelpBox("StageDataList가 비어있습니다.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        showStages = EditorGUILayout.Foldout(showStages, "Stages", true);
        if (!showStages) return;

        ResizeStageFoldouts(spawnData);

        EditorGUI.indentLevel++;

        for (int stageIndex = 0; stageIndex < spawnData.stageDataList.Count; stageIndex++)
        {
            var stage = spawnData.stageDataList[stageIndex];
            if (stage == null) continue;

            EditorGUILayout.BeginHorizontal();

            stageFoldouts[stageIndex] = EditorGUILayout.Foldout(stageFoldouts[stageIndex], $"Stage: {stage.name}", true);

            if (GUILayout.Button("+ Room", GUILayout.Width(70)))
            {
                Undo.RecordObject(stage, "Add Room");
                var newRoom = CreateInstance<RoomSpawnDataSo>();
                newRoom.name = "RoomSo 추가할것";
                stage.roomSpawnDataList.Add(newRoom);

                EditorUtility.SetDirty(stage);
                ResizeRoomFoldouts(stageIndex, stage);
                Repaint();
                EditorGUILayout.EndHorizontal();
                continue;
            }

            EditorGUILayout.EndHorizontal();

            if (!stageFoldouts[stageIndex]) continue;

            // bossEnemyData 표시
            EditorGUI.indentLevel++;
            var newBossEnemyData = (EnemyDataSo)EditorGUILayout.ObjectField("Boss Enemy Data", stage.bossEnemyData, typeof(EnemyDataSo), false);
            if (newBossEnemyData != stage.bossEnemyData)
            {
                Undo.RecordObject(stage, "Change Boss Enemy Data");
                stage.bossEnemyData = newBossEnemyData;
                EditorUtility.SetDirty(stage);
            }
            EditorGUI.indentLevel--;

            ResizeRoomFoldouts(stageIndex, stage);

            EditorGUI.indentLevel++;

            for (int roomIndex = 0; roomIndex < stage.roomSpawnDataList.Count; roomIndex++)
            {
                var room = stage.roomSpawnDataList[roomIndex];
                if (room == null) continue;

                EditorGUILayout.BeginHorizontal();

                roomFoldouts[stageIndex][roomIndex] = EditorGUILayout.Foldout(roomFoldouts[stageIndex][roomIndex], $"Room: {room.name}", true);

                if (GUILayout.Button("+ Enemy", GUILayout.Width(70)))
                {
                    Undo.RecordObject(room, "Add Enemy");
                    var newEnemy = CreateInstance<EnemySpawnDataSo>();
                    newEnemy.name = "EnemySo 넣어줄것";
                    room.enemyDataSo.Add(newEnemy);

                    EditorUtility.SetDirty(room);
                    ResizeEnemyFoldouts(stageIndex, roomIndex, room);
                    Repaint();
                }

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    Undo.RecordObject(stage, "Remove Room");
                    stage.roomSpawnDataList.RemoveAt(roomIndex);

                    EditorUtility.SetDirty(stage);
                    ResizeRoomFoldouts(stageIndex, stage);
                    Repaint();
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (!roomFoldouts[stageIndex][roomIndex]) continue;

                EditorGUI.indentLevel++;

                var newRoomSO = (RoomSpawnDataSo)EditorGUILayout.ObjectField("Room SO", room, typeof(RoomSpawnDataSo), false);
                if (newRoomSO != room)
                {
                    stage.roomSpawnDataList[roomIndex] = newRoomSO;
                    EditorUtility.SetDirty(stage);
                    continue;
                }

                ResizeEnemyFoldouts(stageIndex, roomIndex, room);

                for (int enemyIndex = 0; enemyIndex < room.enemyDataSo.Count; enemyIndex++)
                {
                    var enemy = room.enemyDataSo[enemyIndex];
                    if (enemy == null) continue;

                    EditorGUILayout.BeginHorizontal();

                    enemyFoldouts[stageIndex][roomIndex][enemyIndex] = EditorGUILayout.Foldout(enemyFoldouts[stageIndex][roomIndex][enemyIndex], enemy.name, true);

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        Undo.RecordObject(room, "Remove Enemy");
                        room.enemyDataSo.RemoveAt(enemyIndex);

                        EditorUtility.SetDirty(room);
                        ResizeEnemyFoldouts(stageIndex, roomIndex, room);
                        Repaint();
                        EditorGUILayout.EndHorizontal();
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                    if (!enemyFoldouts[stageIndex][roomIndex][enemyIndex]) continue;

                    EditorGUI.indentLevel++;

                    var newEnemySO = (EnemySpawnDataSo)EditorGUILayout.ObjectField("Enemy SO", enemy, typeof(EnemySpawnDataSo), false);
                    if (newEnemySO != enemy)
                    {
                        room.enemyDataSo[enemyIndex] = newEnemySO;
                        EditorUtility.SetDirty(room);
                        continue;
                    }

                    if (newEnemySO != null)
                    {
                        newEnemySO.spawnCnt = EditorGUILayout.IntField("Spawn Count", newEnemySO.spawnCnt);

                        var newEnemyData = (EnemyDataSo)EditorGUILayout.ObjectField("EnemyDataSo", newEnemySO.enemyDataSo, typeof(EnemyDataSo), false);
                        if (newEnemyData != newEnemySO.enemyDataSo)
                        {
                            newEnemySO.enemyDataSo = newEnemyData;
                            EditorUtility.SetDirty(newEnemySO);
                            continue;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
    }

    private void ResizeStageFoldouts(SpawnData spawnData)
    {
        while (stageFoldouts.Count < spawnData.stageDataList.Count)
            stageFoldouts.Add(false);
        while (stageFoldouts.Count > spawnData.stageDataList.Count)
            stageFoldouts.RemoveAt(stageFoldouts.Count - 1);

        while (roomFoldouts.Count < spawnData.stageDataList.Count)
            roomFoldouts.Add(new List<bool>());
        while (roomFoldouts.Count > spawnData.stageDataList.Count)
            roomFoldouts.RemoveAt(roomFoldouts.Count - 1);

        while (enemyFoldouts.Count < spawnData.stageDataList.Count)
            enemyFoldouts.Add(new List<List<bool>>());
        while (enemyFoldouts.Count > spawnData.stageDataList.Count)
            enemyFoldouts.RemoveAt(enemyFoldouts.Count - 1);
    }

    private void ResizeRoomFoldouts(int stageIndex, StageDataSo stage)
    {
        if (stage.roomSpawnDataList == null) return;

        var roomList = roomFoldouts[stageIndex];
        while (roomList.Count < stage.roomSpawnDataList.Count)
            roomList.Add(false);
        while (roomList.Count > stage.roomSpawnDataList.Count)
            roomList.RemoveAt(roomList.Count - 1);

        var enemyList = enemyFoldouts[stageIndex];
        while (enemyList.Count < stage.roomSpawnDataList.Count)
            enemyList.Add(new List<bool>());
        while (enemyList.Count > stage.roomSpawnDataList.Count)
            enemyList.RemoveAt(enemyList.Count - 1);
    }

    private void ResizeEnemyFoldouts(int stageIndex, int roomIndex, RoomSpawnDataSo room)
    {
        if (room.enemyDataSo == null)
        {
            enemyFoldouts[stageIndex][roomIndex] = new List<bool>();
            return;
        }

        var list = enemyFoldouts[stageIndex][roomIndex];

        while (list.Count < room.enemyDataSo.Count)
            list.Add(false);
        while (list.Count > room.enemyDataSo.Count)
            list.RemoveAt(list.Count - 1);
    }
}
#endif