using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class SaveData
{
    // 해금된 다이어리 ID나 키워드를 누적 보관
    public List<string> unlockedDiaryIDs = new List<string>();
}

public class ArchiveManager : MonoBehaviour
{
    public static ArchiveManager Instance { get; private set; }
    
    private SaveData currentSaveData = new SaveData();
    private string savePath;

    private void Awake()
    {
        // 씬이 넘어가도 파괴되지 않는 싱글톤 세팅
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else 
        { 
            Destroy(gameObject); 
            return; 
        }

        savePath = Path.Combine(Application.persistentDataPath, "archiveData.json");
        LoadData();
    }

    public void UnlockDiary(string id)
    {
        if (!string.IsNullOrEmpty(id) && !currentSaveData.unlockedDiaryIDs.Contains(id))
        {
            currentSaveData.unlockedDiaryIDs.Add(id);
            SaveData();
            Debug.Log($"[Archive] 다이어리 해금됨: {id}");
        }
    }

    private void SaveData()
    {
        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(savePath, json);
    }

    private void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            currentSaveData = JsonUtility.FromJson<SaveData>(json);
        }
    }
}