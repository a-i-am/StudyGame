using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConceptArchiveManager : MonoBehaviour
{
    public static ConceptArchiveManager Instance { get; private set; }

    public event Action<ConceptData> OnConceptUnlocked;

    private HashSet<string> unlockedConceptIds = new HashSet<string>();
    private string saveFilePath;

    [Serializable]
    private class SaveData
    {
        public List<string> unlockedIds = new List<string>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "unlocked_concepts.json");
        LoadSaveData();
    }

    public bool TryUnlockConcept(ConceptData concept)
    {
        if (concept == null || string.IsNullOrEmpty(concept.conceptId)) return false;
        if (unlockedConceptIds.Contains(concept.conceptId)) return false;

        unlockedConceptIds.Add(concept.conceptId);
        SaveSaveData();
        OnConceptUnlocked?.Invoke(concept);
        return true;
    }

    public void ForceNotifyConceptUnlocked(ConceptData concept)
    {
        if (concept != null)
        {
            OnConceptUnlocked?.Invoke(concept);
        }
    }

    public bool IsUnlocked(string conceptId)
    {
        if (string.IsNullOrEmpty(conceptId)) return false;
        return unlockedConceptIds.Contains(conceptId);
    }

    public bool IsUnlocked(ConceptData concept)
    {
        if (concept == null) return false;
        return IsUnlocked(concept.conceptId);
    }

    public bool HasMasteredPrerequisites(ConceptData concept)
    {
        if (concept == null || concept.prerequisites == null) return true;
        foreach (ConceptData prereq in concept.prerequisites)
        {
            if (!IsUnlocked(prereq)) return false;
        }
        return true;
    }

    public void ResetSaveData()
    {
        unlockedConceptIds.Clear();
        if (File.Exists(saveFilePath))
        {
            try
            {
                File.Delete(saveFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save file: {ex.Message}");
            }
        }
        Debug.Log("[ConceptArchiveManager] Save data reset successfully.");
    }

    private void LoadSaveData()
    {
        unlockedConceptIds.Clear();
        if (!File.Exists(saveFilePath)) return;

        try
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data != null && data.unlockedIds != null)
            {
                foreach (string id in data.unlockedIds)
                {
                    unlockedConceptIds.Add(id);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load concept archive data: {ex.Message}");
        }
    }

    private void SaveSaveData()
    {
        try
        {
            SaveData data = new SaveData();
            data.unlockedIds.AddRange(unlockedConceptIds);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save concept archive data: {ex.Message}");
        }
    }
}
