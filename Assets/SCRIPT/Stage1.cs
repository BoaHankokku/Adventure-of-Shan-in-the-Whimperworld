using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using ClearSky.Controller;
using ClearSky.Player;
using UnityEngine.SceneManagement;
using ClearSky.Enemy;
public class GameManager : BaseGameManager
{
    private GameDataManager dataManager;

    [Header("Stage 1 Specific")]
    public GameObject firstEnemy;
    private bool firstEnemyDefeatedDialogueShown = false;
    public Stage1Dialogue stage1Dialogue;
    private bool referencesInitialized = false;

    public GameObject merchantZone, bossZone, donutDynamoZone;

    protected override void Start()
    {
        base.Start();
        InitializeReferences();

        if (GameDataManager.Instance != null && GameDataManager.Instance.CurrentSceneType == GameDataManager.SceneType.Gameplay)
        {
            LoadGameProgress();
        }

        stage1Dialogue?.StartInitialDialogue();
        currentLevel = 0;

    }
   
    // ENEMY DEFEATED METHOD:
    public override void EnemyDefeated(GameObject defeatedEnemy)
    {
        Debug.Log($"EnemyDefeated called with: {defeatedEnemy.name}");
        if (!firstEnemyDefeatedDialogueShown && defeatedEnemy == firstEnemy)
        {
            firstEnemyDefeatedDialogueShown = true;
            TriggerDialogue("Completion");
            currentLevel = 1;
            StartCoroutine(ShowIndicatorPanel(currentLevel));
            return;
        }
        enemiesDefeated++;
        CheckObjectiveCompletion(currentLevel, enemiesDefeated);
        if (defeatedEnemy.GetComponent<TrueFormEnemy>() != null)
        {
            TriggerDialogue("AfterTrueForm");
            return;
        }
        if (defeatedEnemy.GetComponent<DynamoEnemy>() != null)
        {
            TriggerDialogue("AfterDonutDynamo");
            return;
        }
        Debug.Log($"[GameManager] Enemy defeated! Total enemies defeated: {enemiesDefeated}");
       
    }
    // OBJECTIVE COMPLETION AND SHOW REWARD PANEL:
    protected override bool CheckObjectiveCompletion(int currentLevel, int enemiesDefeated)
    {
        bool objectiveCompleted = currentLevel switch
        {
            1 => enemiesDefeated >= 3,
            2 => enemiesDefeated >= 3,
            3 => enemiesDefeated >= 1,
            4 => enemiesDefeated >= 5,
            5 => enemiesDefeated >= 1,
            _ => false,
        };

        if (objectiveCompleted)
        {
            Debug.Log($"[GameManager] Objective for level {currentLevel} completed! Showing reward panel.");

            // Double-check before calling ShowRewardPanel
            if (rewardPanels != null && rewardPanels.Length >= currentLevel)
            {
                ShowRewardPanel(currentLevel);
            }
            else
            {
                Debug.LogError($"[GameManager] rewardPanels array is null or out of bounds! Current Level: {currentLevel}");
            }
        }

        return objectiveCompleted;
    }

    protected override void ShowRewardPanel(int levelIndex)
    {
        if (rewardPanels == null || levelIndex - 1 >= rewardPanels.Length) return;

        rewardPanels[levelIndex - 1]?.SetActive(true);
        AddRewards(levelIndex);
        SetupContinueButton(levelIndex - 1, levelIndex);
    }
    protected override void ActivateNextWallSet(int levelIndex)
    {
        switch (levelIndex)
        {
            case 1:
                ActivateWallSet(WallSet1, WallSet2);
                break;
            case 2:
                ActivateWallSet(WallSet2, WallSet3);
                break;
            case 3:
                ActivateWallSet(WallSet3, WallSet4);
                break;
            case 4:
                ActivateWallSet(WallSet4, WallSet5);
                break;
            case 5:
                ActivateWallSet(WallSet5, null);
                break;
            default:
                Debug.LogWarning($"Unexpected level index: {levelIndex}");
                break;
        }
    }

    // REWARD SYSTEM FOR STAGE 1:
    private void AddRewards(int levelIndex)
    {
        switch (levelIndex)
        {
            case 1:
                GameDataManager.Instance.AddPotions(1, 1);
                playerController.playerStats.AddExp(100);
                break;
            case 2:
                GameDataManager.Instance.AddPotions(2, 2);
                playerController.playerStats.AddExp(100);
                break;
            case 3:
                GameDataManager.Instance.AddPotions(3, 3);
                playerController.playerStats.AddExp(100);
                break;
            case 4:
                GameDataManager.Instance.AddPotions(5, 5);
                playerController.playerStats.AddExp(100);
                break;
            case 5:
                GameDataManager.Instance.AddPotions(6, 6);
                playerController.playerStats.AddExp(100);
                break;
        }
    }

    // DIALOGUE TRIGGER METHODS:
    public override void ActivateZone(int zoneIndex)
    {
        switch (zoneIndex)
        {
            case 1: ShowMerchantDialogue(); ; break;
            case 2: StartDonutDynamoEncounter(); break;
            case 3: StartBossFight(); break;
            default: Debug.LogError("Unknown zone index"); break;
        }
    }
    public void TriggerDialogue(string dialogueKey)
    {
        if (stage1Dialogue == null)
        {
            Debug.LogError("[GameManager] Stage1Dialogue is not assigned!");
            return;
        }

        switch (dialogueKey)
        {
            case "Completion":
                stage1Dialogue.StartCompletionDialogue();
                break;
            case "AfterDonutDynamo":
                stage1Dialogue.AftertDonutDynamoEncounter();
                break;
            case "AfterTrueForm":
                stage1Dialogue.AfterTFormEncounter();
                break;
            default:
                Debug.LogError($"[GameManager] Unknown dialogue key: {dialogueKey}");
                break;
        }
    }
    private void ShowMerchantDialogue()
    {
        stage1Dialogue?.StartShanTrixiEncounterDialogue(GameObject.Find("TrixiMerchant"), merchantZone);
    }

    private void StartDonutDynamoEncounter()
    {
        stage1Dialogue?.StartDonutDynamoEncounter();
    }

    private void StartBossFight()
    {
        stage1Dialogue?.StartBossDialogue();
    }

    //INTIALIZE REF FOR STAGE1 AND ETC:
    public override void InitializeReferences()
    {
        if (referencesInitialized)
        {
            Debug.LogWarning("[BaseGameManager] InitializeReferences() already called. Skipping re-initialization.");
            return;
        }

        base.InitializeReferences();

        // Add any additional initialization logic specific to GameManager
        stage1Dialogue = stage1Dialogue ?? FindObjectOfType<Stage1Dialogue>();
        merchantZone = merchantZone ?? GameObject.Find("MerchantZone");
        bossZone = bossZone ?? GameObject.Find("level5BossZone");
        donutDynamoZone = donutDynamoZone ?? GameObject.Find("DynamoZone");
        firstEnemy = firstEnemy ?? GameObject.Find("FirstEnemy");

        referencesInitialized = true;
        Debug.Log("[GameManager] References initialized.");
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoaded(scene, mode);

        if (scene.name == "Stage1") // Ensure this is specific to Stage 1
        {
            Debug.Log("[GameManager] Reinitializing Stage1 components.");
            InitializeReferences();

            Stage1Dialogue stage1Dialogue = FindObjectOfType<Stage1Dialogue>();
            if (stage1Dialogue != null && GameDataManager.Instance.IsTrixiDestroyed())
            {
                if (stage1Dialogue.trixiObject != null)
                {
                    Destroy(stage1Dialogue.trixiObject);
                    Debug.Log("Trixi object destroyed via GameManager.");
                }
            }

                InitializeWalls();
        }
    }
    private void CleanUpBeforeReload()
    {
        // Example: Destroy duplicate instances of managers or unnecessary objects
        var duplicates = GameObject.FindGameObjectsWithTag("GameManager");
        foreach (var duplicate in duplicates)
        {
            if (duplicate != this.gameObject)
            {
                Debug.Log("[GameManager] Destroying duplicate GameManager instance.");
                Destroy(duplicate);
            }
        }
    }

    public void LoadGameProgress()
    {
        // Load data
        Vector3 position;
        float health, mana;
        int level, artifacts, enemies;

        GameDataManager.Instance.LoadProgress(out position, out health, out mana, out level, out artifacts, out enemies);

        // Restore player state
        if (playerController != null)
        {
            playerController.transform.position = position;
        }
        else
        {
            Debug.LogWarning("[GameManager] PlayerController is null. Cannot set player position.");
        }

        if (playerStats != null)
        {
            playerStats.health = health;
            playerStats.mana = mana;
        }
        else
        {
            Debug.LogWarning("[GameManager] PlayerStats is null. Cannot set health and mana.");
        }

        currentLevel = level;
        enemiesDefeated = enemies;

        EnemyBase.DisableAllDefeatedEnemies();

        Debug.Log("[GameManager] Game progress loaded successfully.");

        InitializeReferences();
    }


}
