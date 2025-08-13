using _02_Scripts.Inventory;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SaveManager : Singleton<SaveManager>
{
    private const int SLOT_COUNT = 3;
    private DateTime sessionStart;

    public SaveSlotData[] slots = new SaveSlotData[SLOT_COUNT];
    private const string KEY_BINDINGS_PREF_KEY = "KeyBindings";

    // 키 설정
    public PlayerInputActions inputActions { get; private set; }
    public Dictionary<InputIndex, string> keyBindings = new();

    public int SelectedIndex { get; private set; } = -1;
    
    // 로드 직후 저장 방지를 위한 플래그
    public bool isLoadingSlotData = false;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        LoadAllSlots();

        inputActions = new PlayerInputActions();
        inputActions.Enable();

        LoadKeyBindingsFromPrefs();
        foreach (InputIndex idx in Enum.GetValues(typeof(InputIndex)))
        {
            var act = GetActionFor(idx);
            if (act != null)
            {
                if (keyBindings.TryGetValue(idx, out var path))
                {
                    act.ApplyBindingOverride(0, path);
                }
                else if (act.bindings.Count > 0)
                {
                    keyBindings[idx] = act.bindings[0].effectivePath;
                }
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GameManager.Instance.state != GameState.MainMenu && 
            GameManager.Instance.state != GameState.Synopsis &&
            GameManager.Instance.state != GameState.Ending &&
            GameManager.Instance.state != GameState.Init &&
            SelectedIndex >= 0) 
        {
            Debug.Log("저장 실행");
            // 로드 직후 저장 방지를 위해 약간의 지연 후 저장
            if(GameManager.Instance.tempState != GameState.MainMenu) StartCoroutine(DelayedSaveOnSceneLoad());
        }
    }
    
    private System.Collections.IEnumerator DelayedSaveOnSceneLoad()
    {
        // 로드 중이면 대기
        while (isLoadingSlotData)
        {
            yield return YieldCache.WaitForSeconds(0.1f);
        }
        
        // 추가로 1초 더 대기 (UI 완전 복원 보장)
        yield return YieldCache.WaitForSeconds(1.0f);
        
        Logger.Log("씬로드 저장 (지연)");
        SaveToSelectedSlot();
    }

    #region 슬롯별 파일 저장/로드

    private string SlotFilePath(int idx)
    {
        return Path.Combine(Application.persistentDataPath, $"slot{idx}.json");
    }

    public void LoadAllSlots()
    {
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            string path = SlotFilePath(i);
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    slots[i] = JsonUtility.FromJson<SaveSlotData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Slot {i} 로드 실패: {e.Message}");
                    slots[i] = null;
                }
            }
            else
            {
                slots[i] = null;
            }
        }
    }

    public void SaveAllSlots()
    {
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            SaveSlot(i);
        }
    }

    public void SaveSlot(int idx)
    {
        if (idx < 0 || idx >= SLOT_COUNT) return;

        try
        {
            string path = SlotFilePath(idx);
            if (slots[idx] != null)
            {
                string json = JsonUtility.ToJson(slots[idx], true);
                File.WriteAllText(path, json);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Slot {idx} 저장 실패: {e.Message}");
        }
    }

    public void CreateNewSlot(int idx, string playerName)
    {
        var now = DateTime.Now;
        slots[idx] = new SaveSlotData
        {
            playerName       = playerName,
            saveTime         = now.ToString("생성일 yyyy-MM-dd HH:mm"),
            elapsedTime      = "누적시간 0시간 0분 0초",
            totalSeconds     = 0,
            
            // 게임 데이터를 직접 슬롯에 저장
            gameState        = GameState.Synopsis,
            StageIndex       = 0,
            IsTutorialComplete = false,
            levelData        = DungeonLevel.Easy,
            Gold             = 50,
            totalDamage      = 0,
            totalDamageTaken = 0,
            killCount        = 0,
            runeGetCount     = 0,
            magicGetCount    = 0,
            highScore        = 0,
            inventoryItems   = new List<InventoryItemSave>(),
            potionSlots      = new List<InventoryItemSave>(),
            skillSlots       = new List<SkillSlotSave>(),
            
            playerPosition   = Vector3.zero,
            hasSavedPosition = false
        };
        SaveSlot(idx);
    }

    public void DeleteSlot(int idx)
    {
        slots[idx] = null;
        SaveSlot(idx);
    }

    public void SaveToSelectedSlot()
    {
        if (SelectedIndex < 0) return;

        var slot = slots[SelectedIndex];
        if (slot == null)
        {
            Debug.LogError($"[SaveManager] Slot {SelectedIndex} is null!");
            return;
        }

        // 현재 게임 데이터를 슬롯에 직접 저장
        CollectCurrentGameData(slot);

        // 게임 매니저 데이터 저장
        slot.gameState          = GameManager.Instance.state;
        if (slot.gameState == GameState.Dungeon)
        {
            slot.StageIndex         = GameManager.Instance.sceneFlowManager.currentStageIndex-1;    
        }
        else
        {
            slot.StageIndex         = GameManager.Instance.sceneFlowManager.currentStageIndex;
        }
        
        slot.IsTutorialComplete = GameManager.Instance.IsTutorialComplete;
        slot.levelData          = GameManager.Instance.dungeonLevel;
        slot.Gold               = InventoryManager.Instance.Gold;
        slot.totalDamage        = GameManager.Instance.totalDamage;
        slot.totalDamageTaken   = GameManager.Instance.totalDamageTaken;
        slot.killCount          = GameManager.Instance.killCount;
        slot.runeGetCount       = GameManager.Instance.runeGetCount;
        slot.magicGetCount      = GameManager.Instance.magicGetCount;
        slot.highScore          = GameManager.Instance.highScore;
        slot.ItemDictionaryStr = ItemDictToString();
        slot.AchievDictionaryStr = AchievementDictToString();

        slot.saveTime = DateTime.Now.ToString("저장일 yyyy-MM-dd HH:mm");

        // 플레이어 위치 저장
        if (PlayerController.Instance != null)
        {
            slot.playerPosition    = PlayerController.Instance.transform.position;
            slot.hasSavedPosition  = true;
        }

        Logger.Log("저장");
        SaveSessionElapsedTime();
        SaveSlot(SelectedIndex);
    }

    public void LoadSelectedSlotData()
    {
        if (SelectedIndex < 0) return;
        var slot = slots[SelectedIndex];
        if (slot == null) return;
        
        // 로드 시작 플래그 설정
        isLoadingSlotData = true;
        Debug.Log("[SaveManager] 슬롯 데이터 로드 시작");
        
        // 게임 매니저에 슬롯 데이터 적용
        GameManager.Instance.sceneFlowManager.currentStageIndex = slot.StageIndex;
        GameManager.Instance.IsTutorialComplete                = slot.IsTutorialComplete;
        GameManager.Instance.ChangeDungeonLevel(slot.levelData);
        Logger.Log(slot.gameState.ToString());
        GameManager.Instance.ChangeState(slot.gameState);
        Logger.Log("LoadState");

        // 인벤토리 매니저에 골드 적용
        InventoryManager.Instance.Gold = slot.Gold;
        
        // 콜랙터에 저장
        CollectorManager.Instance.itemDataDic = StringToItemDict(slot.ItemDictionaryStr);
        CollectorManager.Instance.achievDataDic = StringToAchievementDict(slot.AchievDictionaryStr);

        // UI에 인벤토리/포션/스킬 복원
        ApplySlotDataToUI(slot);
    }

    private void ApplySlotDataToUI(SaveSlotData slot)
    {
        StartCoroutine(ApplySlotDataCoroutine(slot));
    }

    private System.Collections.IEnumerator ApplySlotDataCoroutine(SaveSlotData slot)
    {
        yield return new WaitForEndOfFrame();
        
        // SaveData 구조체를 임시로 생성하여 기존 이벤트 시스템과 호환
        var tempSaveData = new SaveData
        {
            gameState      = slot.gameState,
            StageIndex     = slot.StageIndex,
            IsTutorialComplete = slot.IsTutorialComplete,
            levelData      = slot.levelData,
            Gold           = slot.Gold,
            totalDamage    = slot.totalDamage,
            totalDamageTaken   = slot.totalDamageTaken, 
            killCount      = slot.killCount,
            runeGetCount   = slot.runeGetCount,
            magicGetCount  = slot.magicGetCount,
            highScore      = slot.highScore,
            inventoryItems = slot.inventoryItems,
            potionSlots    = slot.potionSlots,
            skillSlots     = slot.skillSlots
        };
        
        Debug.Log($"[SaveManager] LoadEvent 발행 - 스킬 슬롯 {tempSaveData.skillSlots.Count}개");
        EventBus.Publish(new LoadEvent { saveData = tempSaveData });
        
        // UI 복원이 완료될 때까지 충분히 대기
        yield return YieldCache.WaitForSeconds(2.0f);
        
        // 스킬 슬롯이 제대로 로드되었는지 확인
        var skillSlots = InventoryManager.Instance.inventoryUIManager.skillSlots;
        int loadedSkills = 0;
        foreach (var skillSlot in skillSlots)
        {
            if (skillSlot.CurrentSkill != null)
                loadedSkills++;
        }
        
        // Debug.Log($"[SaveManager] 스킬 로드 완료 확인: {loadedSkills}개 스킬 복원됨");
        // foreach (var skillSlot in InventoryManager.Instance.activeSkillSlots)
        // {
        //     skillSlot.RenewSkill();
        // }
        
        // isLoadingSlotData = false;
        // Debug.Log("[SaveManager] 슬롯 데이터 로드 완료");
    }

    public SaveSlotData GetSlotData(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SLOT_COUNT) return null;
        return slots[slotIndex];
    }

    #endregion

    #region 게임 상태 수집

    private void CollectCurrentGameData(SaveSlotData slot)
    {
        slot.inventoryItems.Clear();
        slot.potionSlots.Clear();
        slot.skillSlots.Clear();

        GetItemListInInventory(slot);
        GetSkillListInSlots(slot);
        GetPotionListInSlots(slot);
    }

    public void GetItemListInInventory(SaveSlotData slot)
    {
        slot.inventoryItems.Clear();
        var inventorySlots = InventoryManager.Instance.inventoryUIManager.inventorySlots;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].TryGetComponentInChildren<DraggableItem>(out var item))
            {
                var data = item.itemData;
                var entry = new InventoryItemSave
                {
                    itemType        = data.itemType,
                    slotIndex       = i,
                    dataID          = data.id,
                    attachedRuneIDs = new List<int>()
                };

                if (item.TryGetComponent<SkillItemController>(out var skillCtrl))
                {
                    entry.skillLevel = skillCtrl.level;
                    foreach (var rune in skillCtrl.attachedRunes)
                        entry.attachedRuneIDs.Add(rune.id);
                }

                if (item.TryGetComponent<PotionAmount>(out var potionAmt))
                    entry.potionAmount = potionAmt.curPotionAmount;

                slot.inventoryItems.Add(entry);
            }
        }
    }

    public void GetSkillListInSlots(SaveSlotData slot)
    {
        slot.skillSlots.Clear();
        var skillSlots = InventoryManager.Instance.inventoryUIManager.skillSlots;
        
        Debug.Log($"[SaveManager] 스킬 슬롯 저장 - 총 {skillSlots.Length}개 슬롯");
        
        foreach (var slotCtrl in skillSlots)
        {
            Debug.Log($"[SaveManager] 슬롯 {slotCtrl.slotIndex}: CurrentSkill={slotCtrl.CurrentSkill?.displayName}, skillItemCtrl={slotCtrl.skillItemCtrl != null}");
            
            if (slotCtrl.CurrentSkill == null || slotCtrl.skillItemCtrl == null) continue;

            var s = new SkillSlotSave
            {
                slotIndex = (int)slotCtrl.slotIndex,
                hasSkill  = true,
                skillID   = slotCtrl.CurrentSkill.id,
                skillLevel= slotCtrl.skillItemCtrl.level,
                runeIDs   = new List<int>()
            };
            foreach (var rune in slotCtrl.skillItemCtrl.attachedRunes)
                s.runeIDs.Add(rune.id);

            Debug.Log($"[SaveManager] 저장할 스킬: {slotCtrl.CurrentSkill.displayName}, 레벨: {s.skillLevel}, 룬: {s.runeIDs.Count}개");
            slot.skillSlots.Add(s);
        }
        
        Debug.Log($"[SaveManager] 총 {slot.skillSlots.Count}개 스킬 저장됨");
    }

    public void GetPotionListInSlots(SaveSlotData slot)
    {
        slot.potionSlots.Clear();
        var potionSlots = InventoryManager.Instance.inventoryUIManager.potionSlots;
        foreach (var slotCtrl in potionSlots)
        {
            if (!slotCtrl.TryGetComponentInChildren<DraggableItem>(out var item)) continue;
            if (!item.TryGetComponent<PotionAmount>(out var amt)) continue;

            var p = new InventoryItemSave
            {
                itemType        = ItemType.Potion,
                slotIndex       = (int)slotCtrl.slotIndex,
                dataID          = item.itemData.id,
                potionAmount    = amt.curPotionAmount,
                attachedRuneIDs = new List<int>()
            };
            slot.potionSlots.Add(p);
        }
    }

    #endregion

    #region 세션 & 위치 관리

    public void SelectSlot(int idx)
    {
        SelectedIndex = idx;
        sessionStart  = DateTime.Now;
    }

    public void SaveSessionElapsedTime()
    {
        if (SelectedIndex < 0) return;
        var now = DateTime.Now;
        var span = now - sessionStart;

        var slot = slots[SelectedIndex];
        slot.totalSeconds += (long)span.TotalSeconds;
        sessionStart = now;

        // 수동 분해
        long totalSec = slot.totalSeconds;
        if (totalSec < 0) totalSec = 0;

        long hours   = totalSec / 3600;
        long minutes = (totalSec % 3600) / 60;
        long seconds = totalSec % 60;

        slot.elapsedTime =
            $"누적시간 {hours}시간 {minutes}분 {seconds}초";
    }

    public void SaveGameState()
    {
        if (SelectedIndex < 0) return;
        var slot = slots[SelectedIndex];
        if (PlayerController.Instance != null)
        {
            slot.playerPosition   = PlayerController.Instance.transform.position;
            slot.hasSavedPosition = true;
        }
        SaveSlot(SelectedIndex);
    }

    private void LoadGameState()
    {
        if (SelectedIndex < 0) return;
        var slot = slots[SelectedIndex];
        if (slot.hasSavedPosition && slot.playerPosition != Vector3.zero && PlayerController.Instance != null)
            PlayerController.Instance.transform.position = slot.playerPosition;
    }

    #endregion

    #region 키 바인딩 저장/로드

    public void ApplyKeyBindings()
    {
        foreach (var kv in keyBindings)
        {
            var act = GetActionFor(kv.Key);
            if (act != null)
                act.ApplyBindingOverride(0, kv.Value);
        }
    }

    public InputAction GetActionFor(InputIndex idx)
    {
        var p = inputActions.Player;
        return idx switch
        {
            InputIndex.Skill1   => p.Use_Skill_1,
            InputIndex.Skill2   => p.Use_Skill_2,
            InputIndex.Skill3   => p.Use_Skill_3,
            InputIndex.Skill4   => p.Use_Skill_4,
            InputIndex.Potion1  => p.Use_Potion_1,
            InputIndex.Potion2  => p.Use_Potion_2,
            InputIndex.Interact => p.NPCInteration,
            InputIndex.Inventory=> p.Inventory,
            InputIndex.Map=> p.Map,
            _ => null
        };
    }

    public void SaveKeyBindingsToPrefs()
    {
        var entries = new List<KeyBindingEntry>();
        foreach (var kv in keyBindings)
        {
            entries.Add(new KeyBindingEntry
            {
                slot = kv.Key.ToString(),
                path = kv.Value
            });
        }
        string json = JsonUtility.ToJson(new KeyBindingWrapper { bindings = entries });
        PlayerPrefs.SetString(KEY_BINDINGS_PREF_KEY, json);
        PlayerPrefs.Save();
    }

    public void LoadKeyBindingsFromPrefs()
    {
        keyBindings.Clear();
        if (!PlayerPrefs.HasKey(KEY_BINDINGS_PREF_KEY)) return;
        string json = PlayerPrefs.GetString(KEY_BINDINGS_PREF_KEY);
        var wrapper = JsonUtility.FromJson<KeyBindingWrapper>(json);
        foreach (var entry in wrapper.bindings)
        {
            if (Enum.TryParse(entry.slot, out InputIndex idx))
                keyBindings[idx] = entry.path;
        }
    }

    [Serializable]
    private class KeyBindingWrapper { public List<KeyBindingEntry> bindings; }
    [Serializable]
    public class KeyBindingEntry  { public string slot, path; }
    #endregion
    
      #region Dictionary JSON 직렬화/역직렬화 (ItemData / AchievementData)

    [Serializable]
    private class ItemDictEntry
    {
        public string key;
        public ItemData value;
    }

    [Serializable]
    private class ItemDictWrapper
    {
        public List<ItemDictEntry> entries = new List<ItemDictEntry>();
    }

    [Serializable]
    private class AchievDictEntry
    {
        public string key;
        public AchievementData value;
    }

    [Serializable]
    private class AchievDictWrapper
    {
        public List<AchievDictEntry> entries = new List<AchievDictEntry>();
    }

    public string ItemDictToString()
    {
        ItemDictWrapper wrapper = new ItemDictWrapper();
        foreach (var kv in CollectorManager.Instance.itemDataDic)
        {
            ItemDictEntry e = new ItemDictEntry();
            e.key = kv.Key;
            e.value = kv.Value; // ScriptableObject 참조 그대로 저장
            wrapper.entries.Add(e);
        }
        return JsonUtility.ToJson(wrapper);
    }

    public Dictionary<string, ItemData> StringToItemDict(string json)
    {
        Dictionary<string, ItemData> dict = new Dictionary<string, ItemData>();
        if (string.IsNullOrEmpty(json))
            return dict;

        ItemDictWrapper wrapper = JsonUtility.FromJson<ItemDictWrapper>(json);
        if (wrapper != null && wrapper.entries != null)
        {
            for (int i = 0; i < wrapper.entries.Count; i++)
            {
                var e = wrapper.entries[i];
                if (!string.IsNullOrEmpty(e.key))
                    dict[e.key] = e.value;
            }
        }
        return dict;
    }

    public string AchievementDictToString()
    {
        AchievDictWrapper wrapper = new AchievDictWrapper();
        foreach (var kv in CollectorManager.Instance.achievDataDic)
        {
            AchievDictEntry e = new AchievDictEntry();
            e.key = kv.Key;
            e.value = kv.Value; // ScriptableObject 참조 그대로 저장
            wrapper.entries.Add(e);
        }
        return JsonUtility.ToJson(wrapper);
    }
    
    public Dictionary<string, AchievementData> StringToAchievementDict(string json)
    {
        Dictionary<string, AchievementData> dict = new Dictionary<string, AchievementData>();
        if (string.IsNullOrEmpty(json))
            return dict;

        AchievDictWrapper wrapper = JsonUtility.FromJson<AchievDictWrapper>(json);
        if (wrapper != null && wrapper.entries != null)
        {
            for (int i = 0; i < wrapper.entries.Count; i++)
            {
                var e = wrapper.entries[i];
                if (!string.IsNullOrEmpty(e.key))
                    dict[e.key] = e.value;
            }
        }
        return dict;
    }

    #endregion
}

// 로드 이벤트 (기존 호환성을 위해 유지)
public class LoadEvent { public SaveData saveData; }