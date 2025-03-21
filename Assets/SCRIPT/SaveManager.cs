using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using static GameDataManager;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SaveManager instance created.");
        }
        else
        {
            Debug.LogWarning("Multiple SaveManager instances detected. Destroying the duplicate.");
            Destroy(gameObject);
        }
    }

    public void SaveGame(TextMeshProUGUI saveGameText, int? cutsceneState = null)
    {
        if (saveGameText != null)
            saveGameText.text = "Saving Game...";

        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastSavedScene", currentSceneName); // Overwrite save state

        // Save cutscene state if applicable
        if (cutsceneState.HasValue && SceneManager.GetActiveScene().name.Contains("Cutscene"))
        {
            SaveCutsceneProgress(cutsceneState.Value);
        }

        else if (GameDataManager.Instance.CurrentSceneType == SceneType.Gameplay)
        {
            SaveGameplayProgress();
        }

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SaveGame();
            Debug.Log("Game saved from SaveManager.");
        }

        PlayerPrefs.Save();
        Debug.Log($"Game progress saved. Scene: {currentSceneName}");

        if (saveGameText != null)
            saveGameText.text = "Game Saved!";
    }
    private void SaveCutsceneProgress(int cutsceneState)
    {
        PlayerPrefs.SetInt("CutsceneProgress", cutsceneState);
        Debug.Log($"Cutscene progress saved: State {cutsceneState}");
    }

    private void SaveGameplayProgress()
    {
        GameDataManager.Instance?.SaveGame();
        Debug.Log("Gameplay progress saved.");
    }
    public static void SaveDefeatedMonsters(HashSet<string> defeatedMonsters)
    {
        if (defeatedMonsters == null) return;

        string saveString = string.Join(",", defeatedMonsters);
        PlayerPrefs.SetString("DefeatedMonsters", saveString);
        PlayerPrefs.Save();
        Debug.Log("Defeated monsters saved.");
    }

    public static HashSet<string> LoadDefeatedMonsters()
    {
        string savedData = PlayerPrefs.GetString("DefeatedMonsters", "");
        if (string.IsNullOrEmpty(savedData)) return null;

        HashSet<string> defeatedMonsters = new HashSet<string>(savedData.Split(','));
        Debug.Log("Defeated monsters loaded.");
        return defeatedMonsters;
    }
    // Method to check if there is any saved data
    public bool HasSaveData()
    {
        bool hasData = PlayerPrefs.HasKey("LastSavedScene");
        Debug.Log($"Checking HasSaveData: {hasData}");
        return hasData;
    }

}
