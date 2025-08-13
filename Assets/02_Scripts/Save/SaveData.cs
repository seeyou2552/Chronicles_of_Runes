using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItemSave
{
    public ItemType itemType;
    public int slotIndex;               //어느 슬롯에 저장 되는지
    public int dataID;               // SO를 식별할 키 (예: data.name)
    public int skillLevel;              // SkillBookData 전용
    public List<int> attachedRuneIDs;// SkillBookData 전용
    public int potionAmount;            // PotionData 전용
}

[Serializable]
public class SkillSlotSave
{
    public int slotIndex;
    public bool hasSkill;
    public int skillID;
    public int skillLevel;
    public List<int> runeIDs;
}
[System.Serializable]
public class SaveSlotData
{
    // 슬롯 메타데이터
    public string playerName;
    public string saveTime;
    public string elapsedTime;
    public long totalSeconds;
    public Vector3 playerPosition;
    public bool hasSavedPosition;
    
    // 게임 상태 데이터 (기존 SaveData의 내용을 직접 포함)
    public GameState gameState;
    public int StageIndex;
    public bool IsTutorialComplete;
    public DungeonLevel levelData;
    public int Gold;

    // GameManager 점수 데이터
    public int totalDamage;
    public int totalDamageTaken;
    public int killCount;
    public int runeGetCount;
    public int magicGetCount;
    public int highScore;
    
    // 인벤토리 관련 데이터
    public List<InventoryItemSave> inventoryItems;
    public List<InventoryItemSave> potionSlots;
    public List<SkillSlotSave> skillSlots;
    
    public String ItemDictionaryStr;
    public String AchievDictionaryStr;
}
[Serializable]
public class SaveData
{
    public GameState gameState;
    public int StageIndex;
    public bool IsTutorialComplete;
    public DungeonLevel levelData;
    public int Gold;
    public int totalDamage;
    public int totalDamageTaken;
    public int killCount;
    public int runeGetCount;
    public int magicGetCount;
    public int highScore;
    public List<InventoryItemSave> inventoryItems = new();
    public List<InventoryItemSave> potionSlots = new();
    public List<SkillSlotSave> skillSlots = new();
    public String ItemDictionaryStr;
    public String AchievDictionaryStr;
}