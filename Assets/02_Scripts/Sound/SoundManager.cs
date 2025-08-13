using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 사운드 데이터 클래스
[System.Serializable]
public class SoundData
{
    public string id;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    public SoundType type;
}

public enum SoundType
{
    BGM,
    SFX,
    UI
}

// 메인 사운드 매니저 (싱글톤)
public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = new GameObject("SoundManager");
                instance = obj.AddComponent<SoundManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private Dictionary<string, SoundData> soundDict = new Dictionary<string, SoundData>();
    
    public float masterVolume = 1f;
    public static float bgmVolume = 1f;
    public static float sfxVolume = 1f;
    public static float uiVolume = 1f;

    private BGMPlayer bgmPlayer;
    private SFXPool sfxPool;
    private UIPlayer uiPlayer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // 사운드 플레이어 초기화
        bgmPlayer = new GameObject("BGMPlayer").AddComponent<BGMPlayer>();
        bgmPlayer.transform.SetParent(transform);
        bgmPlayer.Initialize();

        sfxPool = new GameObject("SFXPool").AddComponent<SFXPool>();
        sfxPool.transform.SetParent(transform);
        sfxPool.Initialize(10);

        uiPlayer = new GameObject("UIPlayer").AddComponent<UIPlayer>();
        uiPlayer.transform.SetParent(transform);
        uiPlayer.Initialize();

        LoadCSVAndBuildSoundTable();
    }

    private void LoadCSVAndBuildSoundTable()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Sounds/SampleSoundTable"); // Resources/Sounds/SampleSoundTable.csv
        if (csvFile == null)
        {
            Debug.LogError("[SoundManager] CSV 파일을 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 6) continue;

            string id = values[0].Trim();
            string path = values[1].Trim();
            if (soundDict.ContainsKey(id)) continue;

            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning($"[SoundManager] AudioClip 로드 실패: {path}");
                continue;
            }

            SoundType type = (SoundType)System.Enum.Parse(typeof(SoundType), values[5].Trim());

            soundDict[id] = new SoundData
            {
                id = id,
                clip = clip,
                volume = float.Parse(values[2]),
                pitch = float.Parse(values[3]),
                loop = bool.Parse(values[4]),
                type = type
            };
        }

        Debug.Log($"[SoundManager] 사운드 {soundDict.Count}개 로드 완료.");
    }

    private SoundData GetSound(string id)
    {
        if (soundDict.TryGetValue(id, out var data)) return data;
        Debug.LogWarning($"[SoundManager] 사운드 ID '{id}' 없음.");
        return null;
    }

    public static void PlayBGM(string id, float fadeTime = 0f)
    {
        var sound = Instance.GetSound(id);
        if (sound != null && sound.type == SoundType.BGM)
            Instance.bgmPlayer.Play(sound, fadeTime);
    }

    public static void PlaySFX(string id)
    {
        if(id == null) return;
        var sound = Instance.GetSound(id);
        if (sound != null && sound.type == SoundType.SFX)
            Instance.sfxPool.Play(sound);
    }

    public static void PlayUI(string id)
    {
        var sound = Instance.GetSound(id);
        if (sound != null && sound.type == SoundType.UI)
            Instance.uiPlayer.Play(sound);
    }

    public static void StopBGM(float fadeTime = 0f) => Instance.bgmPlayer.Stop(fadeTime);
    public static void StopAllSFX() => Instance.sfxPool.StopAll();
    public static void StopUI() => Instance.uiPlayer.Stop();

    public static void SetMasterVolume(float volume)
    {
        Instance.masterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = Instance.masterVolume;
    }

    public static void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        Instance.bgmPlayer.SetVolume(volume);
    }
    
    public static void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        Instance.sfxPool.SetVolume(volume);
    }

    public static bool IsBGMPlaying => Instance.bgmPlayer.IsPlaying;


    public void Preload(string BGMName)
    {
        var bossSound = GetSound(BGMName);
        if (bossSound != null && bossSound.clip != null)
        {
            // 아직 로드가 안됐다면 메모리에 올리기
            if (bossSound.clip.loadState != AudioDataLoadState.Loaded)
            {
                bossSound.clip.LoadAudioData();
            }
        }
    }
}