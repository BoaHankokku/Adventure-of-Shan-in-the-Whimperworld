using ClearSky.Player;
using UnityEngine;
using System.Collections.Generic;
using ClearSky.Controller;
using ClearSky.Enemy;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public enum SceneType
    {

        Gameplay,
        Cutscene,
        MainMenu
    }

    public static GameDataManager Instance { get; private set; }

    private HashSet<string> defeatedMonsters = new HashSet<string>();
    public SceneType CurrentSceneType { get; set; } = SceneType.Gameplay;
    public bool IsSkill2Unlocked { get; set; }
    public bool IsSkill3Unlocked { get; set; }
    public bool IsSkill4Unlocked { get; set; }

    private HashSet<int> unlockedSkills = new HashSet<int>();
    private Dictionary<string, bool> dialogueFlags = new Dictionary<string, bool>();

    private PlayerStats playerStats;

    private bool isTrixiDestroyed = false;

    private int healthPotionCount = 0;
    private int manaPotionCount = 0;
    public int GetHealthPotionCount() => healthPotionCount;
    public int GetManaPotionCount() => manaPotionCount;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name.Contains("Cutscene"))
        {
            if (Instance != null)
            {
                Debug.Log("[GameDataManager] Not needed in Cutscene. Destroying instance.");
                Destroy(Instance.gameObject);
                Instance = null;
            }
            return;
        }


        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Debug.Log($"SceneType set to {CurrentSceneType}.");

        if (CurrentSceneType == SceneType.Gameplay)
        {
            InitializeGameplayScene();
        }
        else
        {
            Debug.Log($"No PlayerStats required for SceneType: {CurrentSceneType}.");
        }
    }
    private void InitializeGameplayScene()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats not found in Gameplay scene. Ensure it is assigned in the scene.");
        }
        else
        {
            Debug.Log("PlayerStats successfully initialized in Gameplay scene.");
        }

        // Load player stats when in the Gameplay scene
        LoadPlayerStats();
    }

    public bool IsStageCompleted(int stage)
    {
        return PlayerPrefs.GetInt($"Stage{stage}Completed", 0) == 1;
    }

    public bool IsDialogueCompleted(string dialogueKey)
    {
        return dialogueFlags.ContainsKey(dialogueKey) && dialogueFlags[dialogueKey];
    }

    private void InitializeGameData()
    {
        try
        {
            // Load initial potion counts
            healthPotionCount = PlayerPrefs.GetInt("HealthPotions", 0);
            manaPotionCount = PlayerPrefs.GetInt("ManaPotions", 0);

            LoadDefeatedMonsters();
            LoadPlayerStats();
            LoadDialogueFlags();

            Debug.Log("Game data successfully initialized.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during game data initialization: {ex.Message}");
        }
    }

    // Replace the existing SaveGame() method in GameDataManager.cs
    public void SaveGame()
    {
        Debug.Log("SaveGame called.");

        // Ensure the method is only called in a Gameplay scene
        if (CurrentSceneType != SceneType.Gameplay)
        {
            Debug.LogWarning("SaveGame called outside of Gameplay scene. Save operation aborted.");
            return;
        }

        // Ensure the BaseGameManager instance exists
        if (BaseGameManager.Instance == null)
        {
            Debug.LogWarning("BaseGameManager instance is null. Cannot save game data.");
            return;
        }

        // Save potions using BaseGameManager
        PlayerPrefs.SetInt("HealthPotions", healthPotionCount);
        PlayerPrefs.SetInt("ManaPotions", manaPotionCount);

        // Save unlocked skills
        foreach (int skillID in unlockedSkills)
        {
            PlayerPrefs.SetInt($"Skill_{skillID}_Unlocked", 1);
            Debug.Log($"Skill {skillID} saved as unlocked.");
        }

        // Save player stats
        if (playerStats == null)
        {
            playerStats = BaseGameManager.Instance.playerStats; // Use the playerStats reference from BaseGameManager
        }

        if (playerStats != null)
        {
            PlayerPrefs.SetFloat("PlayerHealth", playerStats.health);
            PlayerPrefs.SetFloat("PlayerMana", playerStats.mana);
            PlayerPrefs.SetInt("PlayerExp", playerStats.Exp);
            SavePlayerTransform(playerStats.transform);
            Debug.Log("Player stats saved successfully.");
        }
        else
        {
            Debug.LogWarning("PlayerStats is null. Skipping save for player stats.");
        }

        // Save other progress data
        SaveObjectives(BaseGameManager.Instance?.currentLevel ?? 0);
        SaveManager.SaveDefeatedMonsters(defeatedMonsters);
        SaveDialogueFlags();

        PlayerPrefs.Save();
        Debug.Log("Game data saved successfully.");
    }

    public bool IsSkillUnlocked(int skillID)
    {
        return PlayerPrefs.GetInt($"Skill_{skillID}_Unlocked", 0) == 1;
    }
    public void SaveProgress(int stage, Vector3 position, float health, float mana, int level, int artifacts, int enemies)
    {
        PlayerPrefs.SetInt($"Stage{stage}Completed", 1);
        PlayerPrefs.SetFloat("PlayerHealth", health);
        PlayerPrefs.SetFloat("PlayerMana", mana);
        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.SetInt("ArtifactsClaimed", artifacts);
        PlayerPrefs.SetInt("EnemiesDefeated", enemies);
        PlayerPrefs.Save();

        Debug.Log($"Progress with additional parameters saved for stage {stage}.");
    }
    public void SavePlayerStats()
    {
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            PlayerPrefs.SetFloat("PlayerHealth", playerStats.health);
            PlayerPrefs.SetFloat("PlayerMana", playerStats.mana);
            PlayerPrefs.SetInt("PlayerExp", playerStats.Exp);
            PlayerPrefs.Save();
            Debug.Log("Player stats saved.");
        }

    }
    public void LoadPlayerStats()
    {
        if (playerStats != null)
        {
            playerStats.health = PlayerPrefs.GetFloat("PlayerHealth", 100);
            playerStats.mana = PlayerPrefs.GetFloat("PlayerMana", 100);
            playerStats.Exp = PlayerPrefs.GetInt("PlayerExp", 0);
            Debug.Log("Player stats loaded.");
        }
        else
        {
            Debug.LogWarning("PlayerStats not found. Cannot load stats.");
        }
    }

    public void LoadGame()
    {
        Debug.Log("LoadGame called.");

        if (!PlayerPrefs.HasKey("LastSavedScene"))
        {
            Debug.LogWarning("[SaveManager] No saved game found.");
            return;
        }

        // Load the last saved scene
        string lastSavedScene = PlayerPrefs.GetString("LastSavedScene");
        SceneManager.LoadScene(lastSavedScene);

        // Ensure BaseGameManager is initialized
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            Debug.Log($"[SaveManager] Loaded scene: {scene.name}");

            // Ensure BaseGameManager is reinitialized
            BaseGameManager gameManager = FindObjectOfType<BaseGameManager>();
            if (gameManager != null)
            {
                gameManager.InitializeReferences(); // Reinitialize Reward and Indicator Panels
            }
            else
            {
                Debug.LogError("[SaveManager] BaseGameManager not found after loading scene.");
            }
        };

        // Load potions
        healthPotionCount = PlayerPrefs.GetInt("HealthPotions", 0);
        manaPotionCount = PlayerPrefs.GetInt("ManaPotions", 0);

        // Update the potion UI
        var playerController = FindObjectOfType<SimplePlayerController>();
        if (playerController != null)
        {
            playerController.UpdatePotionUI();
        }

        // Load unlocked skills
        unlockedSkills.Clear();
        for (int i = 2; i <= 4; i++)
        {
            if (PlayerPrefs.GetInt($"Skill_{i}_Unlocked", 0) == 1)
            {
                unlockedSkills.Add(i);
                Debug.Log($"Skill {i} loaded as unlocked.");
            }
        }

        // Apply unlocked skills to the player
        if (playerController != null)
        {
            foreach (int skillID in unlockedSkills)
            {
                playerController.UnlockSkill(skillID);
                Debug.Log($"Skill {skillID} applied to playerController.");
            }
        }

        // Load Player Stats
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.health = PlayerPrefs.GetFloat("PlayerHealth", 100);
            playerStats.mana = PlayerPrefs.GetFloat("PlayerMana", 100);
            playerStats.Exp = PlayerPrefs.GetInt("PlayerExp", 0);
            LoadPlayerTransform(playerStats.transform);
            Debug.Log("Player stats loaded.");
        }
        else
        {
            Debug.LogWarning("PlayerStats is null. Cannot load stats.");
        }

        Debug.Log("Game data loaded successfully.");
    }


    public void SavePlayerTransform(Transform playerTransform)
    {
        PlayerPrefs.SetFloat("PlayerPosX", playerTransform.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", playerTransform.position.y);
        PlayerPrefs.SetFloat("PlayerPosZ", playerTransform.position.z);
    }

    public void LoadPlayerTransform(Transform playerTransform)
    {
        float x = PlayerPrefs.GetFloat("PlayerPosX", 0);
        float y = PlayerPrefs.GetFloat("PlayerPosY", 0);
        float z = PlayerPrefs.GetFloat("PlayerPosZ", 0);
        playerTransform.position = new Vector3(x, y, z);
    }
    public Vector3 LoadPlayerPosition()
    {
        if (PlayerPrefs.HasKey("PlayerPosX") && PlayerPrefs.HasKey("PlayerPosY") && PlayerPrefs.HasKey("PlayerPosZ"))
        {
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            float z = PlayerPrefs.GetFloat("PlayerPosZ");

            Debug.Log($"[GameDataManager] Loaded player position: ({x}, {y}, {z})");
            return new Vector3(x, y, z);
        }

        Debug.LogWarning("[GameDataManager] No saved player position found, using default (0,0,0).");
        return Vector3.zero; // Default position if no saved data
    }

    private void LoadDefeatedMonsters()
    {
        string savedData = PlayerPrefs.GetString("DefeatedMonsters", "");
        defeatedMonsters = new HashSet<string>(savedData.Split(','));
    }

    private void LoadDialogueFlags()
    {
        foreach (var key in dialogueFlags.Keys)
        {
            dialogueFlags[key] = PlayerPrefs.GetInt($"Dialogue_{key}", 0) == 1;
        }

        Debug.Log("Dialogue flags loaded successfully.");
    }

    public void SaveDialogueFlags()
    {
        foreach (var key in dialogueFlags.Keys)
        {
            PlayerPrefs.SetInt($"Dialogue_{key}", dialogueFlags[key] ? 1 : 0);
        }
        PlayerPrefs.Save();
        Debug.Log("Dialogue flags saved.");
    }

    public void SetDialogueCompleted(string dialogueKey, bool isCompleted)
    {
        dialogueFlags[dialogueKey] = isCompleted;
        PlayerPrefs.SetInt($"Dialogue_{dialogueKey}", isCompleted ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Dialogue '{dialogueKey}' set to completed: {isCompleted}");
    }


    public void MarkMonsterAsDefeated(string monsterID)
    {
        if (!defeatedMonsters.Contains(monsterID))
        {
            defeatedMonsters.Add(monsterID);
            SaveDefeatedMonsters();
            Debug.Log($"Monster {monsterID} marked as defeated.");
        }
    }
    public bool IsMonsterDefeated(string monsterID)
    {
        return defeatedMonsters.Contains(monsterID);
    }
    private void SaveDefeatedMonsters()
    {
        PlayerPrefs.SetString("DefeatedMonsters", string.Join(",", defeatedMonsters));
        PlayerPrefs.Save();
        Debug.Log("Defeated monsters saved.");
    }
    public void SaveObjectives(int checkpointID)
    {
        PlayerPrefs.SetInt("CheckpointID", checkpointID);
        PlayerPrefs.Save();
        Debug.Log($"Objectives saved for checkpoint {checkpointID}.");
    }
    public void LoadGameProgress()
    {
        // Load data
        Vector3 position;
        float health, mana;
        int level, artifacts, enemies;

        GameDataManager.Instance.LoadProgress(out position, out health, out mana, out level, out artifacts, out enemies);

        // Restore player state
        var playerController = BaseGameManager.Instance?.playerController ?? FindObjectOfType<SimplePlayerController>();
        if (playerController != null)
        {
            playerController.transform.position = position;
        }
        else
        {
            Debug.LogWarning("PlayerController not found. Position not updated.");
        }

        var playerStats = BaseGameManager.Instance?.playerStats ?? FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.health = health;
            playerStats.mana = mana;
        }

        int currentLevel = BaseGameManager.Instance?.currentLevel ?? 1;
        int enemiesDefeated = BaseGameManager.Instance?.enemiesDefeated ?? 0;

        EnemyBase.DisableAllDefeatedEnemies();

        Debug.Log("[GameManager] Game progress loaded successfully.");
    }


    public void LoadProgress(out Vector3 position, out float health, out float mana, out int level, out int artifacts, out int enemies)
    {
        position = new Vector3(
            PlayerPrefs.GetFloat("PlayerPosX", 0),
            PlayerPrefs.GetFloat("PlayerPosY", 0),
            PlayerPrefs.GetFloat("PlayerPosZ", 0)
        );

        health = PlayerPrefs.GetFloat("PlayerHealth", 100);
        mana = PlayerPrefs.GetFloat("PlayerMana", 100);
        level = PlayerPrefs.GetInt("PlayerLevel", 1);
        artifacts = PlayerPrefs.GetInt("ArtifactsClaimed", 0);
        enemies = PlayerPrefs.GetInt("EnemiesDefeated", 0);

        Debug.Log("Progress with additional data loaded successfully.");
    }

    public void SaveProgress(int stage, Vector3 position, float health, float mana)
    {
        PlayerPrefs.SetInt($"Stage{stage}Completed", 1);
        PlayerPrefs.SetFloat("PlayerHealth", health);
        PlayerPrefs.SetFloat("PlayerMana", mana);
        PlayerPrefs.Save();

        Debug.Log($"Progress with health and mana saved for stage {stage}.");
    }
    public void SaveBarrelState(int barrelIndex, bool isDisabled)
    {
        PlayerPrefs.SetInt($"Barrel_{barrelIndex}", isDisabled ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Barrel state saved: Index = {barrelIndex}, Disabled = {isDisabled}");
    }

    public bool LoadBarrelState(int barrelIndex)
    {
        return PlayerPrefs.GetInt($"Barrel_{barrelIndex}", 0) == 1;
    }
    public void ClearProgress()
    {
        Debug.Log("Clearing all game progress...");

        // Delete all stored progress
        PlayerPrefs.DeleteAll();

        // Reset potion counts
        healthPotionCount = 0;
        manaPotionCount = 0;
        PlayerPrefs.SetInt("HealthPotions", healthPotionCount);
        PlayerPrefs.SetInt("ManaPotions", manaPotionCount);

        // Reset player stats
        PlayerPrefs.SetFloat("PlayerHealth", 100);
        PlayerPrefs.SetFloat("PlayerMana", 100);
        PlayerPrefs.SetInt("PlayerExp", 0);
        PlayerPrefs.SetInt("PlayerLevel", 1);

        // Reset skills and progress
        PlayerPrefs.DeleteKey("Skill_2_Unlocked");
        PlayerPrefs.DeleteKey("Skill_3_Unlocked");
        PlayerPrefs.DeleteKey("Skill_4_Unlocked");
        PlayerPrefs.DeleteKey("ArtifactsClaimed");
        PlayerPrefs.DeleteKey("EnemiesDefeated");

        // Reset position
        PlayerPrefs.SetFloat("PlayerPosX", 0);
        PlayerPrefs.SetFloat("PlayerPosY", 0);
        PlayerPrefs.SetFloat("PlayerPosZ", 0);

        // Clear additional progress
        PlayerPrefs.DeleteKey("Stage1Completed");
        PlayerPrefs.DeleteKey("Stage2Completed");
        PlayerPrefs.DeleteKey("Stage3Completed");

        // Clear in-memory data
        defeatedMonsters.Clear();
        dialogueFlags.Clear();
        unlockedSkills.Clear();

        PlayerPrefs.Save();

        // Notify the UI to update potion counts
        var playerController = FindObjectOfType<SimplePlayerController>();
        if (playerController != null)
        {
            playerController.UpdatePotionUI();
        }

        Debug.Log("All progress has been cleared, including potion counts.");
    }


    public void SaveWallState(int activeWallSet)
    {
        PlayerPrefs.SetInt("ActiveWallSet", activeWallSet);
        PlayerPrefs.Save();
        Debug.Log($"Wall state saved: ActiveWallSet = {activeWallSet}");
    }

    public int LoadWallState()
    {
        int activeWallSet = PlayerPrefs.GetInt("ActiveWallSet", 1); // Default to WallSet1
        Debug.Log($"Loaded wall state: ActiveWallSet = {activeWallSet}");
        return activeWallSet;
    }
    public void UnlockSkill(int skillID)
    {
        if (unlockedSkills.Add(skillID))
        {
            PlayerPrefs.SetInt($"Skill_{skillID}_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log($"Skill {skillID} unlocked and saved.");
        }
    }

    public void SetTrixiDestroyed(bool destroyed)
    {
        isTrixiDestroyed = destroyed;
        PlayerPrefs.SetInt("TrixiDestroyed", destroyed ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsTrixiDestroyed()
    {
        return PlayerPrefs.GetInt("TrixiDestroyed", 0) == 1;
    }


    // Potion count setters
    public void SetHealthPotionCount(int count)
    {
        healthPotionCount = count;
        PlayerPrefs.SetInt("HealthPotions", count);
        PlayerPrefs.Save();
    }

    public void SetManaPotionCount(int count)
    {
        manaPotionCount = count;
        PlayerPrefs.SetInt("ManaPotions", count);
        PlayerPrefs.Save();
    }

    public void AddPotions(int healthPotionCountToAdd, int manaPotionCountToAdd)
    {
        healthPotionCount += healthPotionCountToAdd;
        manaPotionCount += manaPotionCountToAdd;

        // Save the updated counts to PlayerPrefs
        PlayerPrefs.SetInt("HealthPotions", healthPotionCount);
        PlayerPrefs.SetInt("ManaPotions", manaPotionCount);
        PlayerPrefs.Save();

        // Notify UI to update
        var playerController = FindObjectOfType<SimplePlayerController>();
        if (playerController != null)
        {
            playerController.UpdatePotionUI();
        }
    }

    public static void CleanupInstance()
    {
        if (Instance != null)
        {
            Debug.Log("[GameDataManager] Cleaning up instance before scene transition.");
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }


}
