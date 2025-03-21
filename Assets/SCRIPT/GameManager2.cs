using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using ClearSky.Controller;
using ClearSky.Player;
using System;
using System.Collections.Generic;
using ClearSky.Enemy;
using System.Reflection;
using UnityEngine.SceneManagement;

public class GameManager2 : BaseGameManager
{

    [Header("Artifact Search System")]
    public int artifactsClaimed = 0;
    public int requiredArtifacts = 5;
    public TextMeshProUGUI countText;
    public GameObject tracker;
    public override int ArtifactsClaimed => artifactsClaimed;

    [Header("Barrel System")]
    public Collider2D[] barrelColliders;
    public Collider2D[] BarrelColliders => barrelColliders;


    [Header("Zones")]
    public GameObject[] zones;

  
    [Header("Enemy System")]
    public GameObject enemyPrefab; // Enemy prefab
    public Transform[] enemySpawnPoints; // Array of spawn points
    private List<GameObject> activeEnemies = new List<GameObject>(); // Track active enemies
    private int currentSpawnIndex = 0;

    [Header("Player & Dialogue")]
    public Stage2Dialogue stage2Dialogue;
    public GameObject ShowCannotPassMessage;

    protected override void Start()
    {
        base.Start();
        //load player new position
        if (GameDataManager.Instance != null)
        {
            Vector3 spawnPosition;

            // ✅ Load saved position only if a save exists
            if (PlayerPrefs.HasKey("PlayerPosX") && PlayerPrefs.HasKey("PlayerPosY") && PlayerPrefs.HasKey("PlayerPosZ"))
            {
                spawnPosition = GameDataManager.Instance.LoadPlayerPosition();
                Debug.Log($"[GameManager2] Loaded saved player position: {spawnPosition}");
            }
            else
            {
                // ✅ If no save exists, use the default Stage 2 position
                spawnPosition = new Vector3(-7.63f, -2.69f, 0f);
                Debug.Log($"[GameManager2] No saved position. Using default Stage 2 spawn: {spawnPosition}");
            }

            if (playerController != null)
            {
                playerController.transform.position = spawnPosition;
            }
            else
            {
                Debug.LogError("[GameManager2] PlayerController is NULL! Cannot set player position.");
            }
        }
        else
        {
            Debug.LogError("[GameManager2] GameDataManager instance is NULL!");
        }
        // Load barrel states
        for (int i = 0; i < barrelColliders.Length; i++)
        {
            if (GameDataManager.Instance.LoadBarrelState(i))
            {
                DisableBarrelCollider(i);
            }
        }
        RestoreBarrelStates();
        InitializeGame();
        InitializeWalls();
        InitializePlayerStats();
        playerStats = FindObjectOfType<PlayerStats>();


        if (stage2Dialogue != null)
        {
            stage2Dialogue.StartInitialDialogue();
        }
        else
        {
            Debug.LogWarning("Stage2Dialogue is not assigned or missing.");
        }

        foreach (var barrel in barrelColliders)
        {
            Debug.Log($"Assigned Barrel Collider: {barrel.name}");
        }
        confirmationPanel.SetActive(false);
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
        if (tracker != null)
        {
            tracker.SetActive(false);
        }
        for (int i = 0; i < enemySpawnPoints.Length; i++)
        {
            Debug.Log($"[Start] Spawn Point {i}: {enemySpawnPoints[i].position}");
        }
        UpdateArtifactCountUI();
    }

    /// <summary>
    ///              STAGE 2 STARTING POINT
    /// </summary>
    public override void ProceedAfterInitialDialogue()
    {
        Debug.Log("Proceeding after initial dialogue.");
        StartCoroutine(HandlePostDialogueTransition());
    }

    private IEnumerator HandlePostDialogueTransition()
    {
        yield return new WaitUntil(() => !stage2Dialogue.dialoguePanel.activeSelf);
        currentLevel = 1;
        StartCoroutine(ShowIndicatorPanel(currentLevel));
    }

    private void InitializePlayerStats()
    {
        playerStats = FindObjectOfType<PlayerStats>();
    }
    private void Update()
    {
        if (artifactsClaimed >= requiredArtifacts && tracker != null && tracker.activeSelf)
        {
            tracker.SetActive(false);
            Debug.Log("Tracker disabled in Update as all artifacts are collected.");
        }
    }
    private void InitializeGame()
    {
        foreach (var panel in rewardPanels) panel.SetActive(false);
        foreach (var panel in indicatorPanels) panel.SetActive(false);
    }

    /// <summary>
    ///              BARREL INTERACTION SYSTEM
    /// </summary>
    public override void HandleBarrelCollision(Collider2D collision, Collider2D[] barrelColliders)
    {
        base.HandleBarrelCollision(collision, barrelColliders); // Use base logic
        if (tracker != null && !tracker.activeSelf)
        {
            tracker.SetActive(true);
        }
    }
    private void RestoreBarrelStates()
    {
        for (int i = 0; i < barrelColliders.Length; i++)
        {
            if (GameDataManager.Instance.LoadBarrelState(i))
            {
                DisableBarrelCollider(i);
            }
        }
    }

    private void ShowConfirmationPanel()
    {
        if (confirmationPanel == null || confirmationText == null)
        {
            Debug.LogError("Confirmation panel or text is not assigned in the Inspector!");
            return;
        }

        confirmationText.text = "Encountered a barrel. Do you wish to search it for artifacts?";
        confirmationPanel.SetActive(true);
    }
    private void OnYesButtonClicked()
    {
        base.OnYesButtonClicked();
        SearchBarrel(currentBarrelIndex);
        CloseConfirmationPanel();
    }

    private void OnNoButtonClicked()
    {
        // Update the confirmation text with a warning message
        confirmationText.text = "<color=red>You must search the barrel to be able to proceed.</color>";

        // Delay before closing the panel
        StartCoroutine(CloseConfirmationPanelAfterDelay());
    }
    private IEnumerator CloseConfirmationPanelAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Wait 2 seconds for the message to be visible
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Confirmation panel reference is missing!");
        }

        Debug.Log("Confirmation panel has been deactivated.");
    }

    private int GetBarrelIndex(Collider2D barrelCollider)
    {
        try
        {
            int index = int.Parse(barrelCollider.gameObject.name.Replace("BAR", ""));
            Debug.Log($"Barrel Index Parsed: {index}");
            return index;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse barrel index: {e.Message}");
            return -1; // Return an invalid index for debugging
        }
    }

    private void SearchBarrel(int barrelIndex)
    {
        Debug.Log($"SearchBarrel invoked with index: {barrelIndex}");

        switch (barrelIndex)
        {
            case 0: // First barrel: Claim an artifact
            case 3:
            case 4:
            case 6:
            case 9:
                confirmationText.text = "<color=blue>Searching the Barrel...</color>\n<color=green>Claimed Artifact!</color>";
                ClaimArtifact(); // Handle artifact claim
                DisableBarrelCollider(barrelIndex); // Disable the collider after action
                Debug.Log($"Artifact claimed! Total: {artifactsClaimed}");
                break;

            case 1: // Second barrel: Spawn an enemy
            case 2:
            case 5:
            case 7:
            case 8:
                confirmationText.text = "<color=blue>Searching the Barrel...</color>\n<color=red>Spawned Enemy!</color>";
                SpawnEnemy(barrelIndex); // Handle enemy spawn
                DisableBarrelCollider(barrelIndex); // Disable the collider after action
                Debug.Log($"Enemy spawned from barrel {barrelIndex}!");
                break;

            default:
                Debug.LogWarning("No defined behavior for this barrel.");
                break;
        }
    }
    private void DisableBarrelCollider(int barrelIndex)
    {
        if (barrelIndex < 0 || barrelIndex >= barrelColliders.Length)
        {
            GameDataManager.Instance.SaveBarrelState(barrelIndex, true);
            Debug.LogWarning($"Invalid barrel index {barrelIndex}. Unable to disable collider.");
            return;
        }

        Collider2D barrelCollider = barrelColliders[barrelIndex];
        if (barrelCollider != null)
        {
            barrelCollider.enabled = false; // Disable the collider to prevent further interactions
            Debug.Log($"Barrel collider at index {barrelIndex} has been disabled.");
        }
        else
        {
            Debug.LogWarning($"Barrel collider at index {barrelIndex} is null.");
        }
    }

    public void SpawnEnemy(int barrelIndex)
    {
        if (enemyPrefab == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogWarning("Enemy prefab or spawn points are not set!");
            return;
        }

        int spawnIndex = MapBarrelToSpawnPoint(barrelIndex);
        if (spawnIndex < 0 || spawnIndex >= enemySpawnPoints.Length)
        {
            Debug.LogError($"Invalid spawn index {spawnIndex} for barrel index {barrelIndex}.");
            return;
        }

        Transform spawnPoint = enemySpawnPoints[spawnIndex];
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        if (newEnemy != null)
        {
            // Deactivate the enemy right after instantiation
            newEnemy.SetActive(false);

            // Reset the enemy and activate it once it's ready
            EnemyAIStage2 enemyScript = newEnemy.GetComponent<EnemyAIStage2>();
            if (enemyScript != null)
            {
                enemyScript.ResetEnemy(100f, spawnPoint.position); // Reset attributes
                newEnemy.SetActive(true); // Finally, activate the enemy
            }
            else
            {
                Debug.LogError("[SpawnEnemy] EnemyAIStage2 script missing from spawned enemy!");
            }

            Debug.Log($"Enemy spawned at {spawnPoint.position}. Active: {newEnemy.activeSelf}");
        }
        else
        {
            Debug.LogError("Failed to instantiate enemyPrefab.");
        }
    }

    private int MapBarrelToSpawnPoint(int barrelIndex)
    {
        switch (barrelIndex)
        {
            case 1: return 0; // Map barrelIndex 1 to spawn point 0
            case 2: return 1; // Map barrelIndex 2 to spawn point 1
            case 5: return 2; // Map barrelIndex 5 to spawn point 2
            case 7: return 3; // Map barrelIndex 6 to spawn point 3
            case 8: return 4; // Map barrelIndex 8 to spawn point 4
            default: return 0; // Invalid mapping
        }
    }
    private void UpdateArtifactCountUI()
    {
        // Update the text to show the current artifact count
        if (countText != null)
        {
            countText.text = $"{artifactsClaimed} / {requiredArtifacts}";
            countText.ForceMeshUpdate();
            Debug.Log($"Updated Artifact Count UI: {countText.text}");
        }
        else
        {
            Debug.LogWarning("Count Text is not assigned!");
        }
        if (artifactsClaimed >= requiredArtifacts)
        {
            if (tracker != null && tracker.activeSelf)
            {
                tracker.SetActive(false);
                Debug.Log("Tracker disabled after collecting all artifacts.");
            }

            // Trigger the artifact completion dialogue
            if (stage2Dialogue != null)
            {
                stage2Dialogue.ArtifactCompletion();
            }
        }
    }

    private void ClaimArtifact()
    {
        artifactsClaimed++; // Increment artifacts count here
        Debug.Log($"Artifact claimed! Total: {artifactsClaimed}");

        // Update UI to show the claimed artifact count
        UpdateArtifactCountUI();
        if (artifactsClaimed >= 3)
        {
            Debug.Log("[ClaimArtifact] Triggering CheckObjectiveCompletion for Level 1");
            CheckObjectiveCompletion(currentLevel, enemiesDefeated); // Ensure level is correctly updated.
        }
        // Check if the player has collected all required artifacts
        if (artifactsClaimed >= requiredArtifacts)
        {
            stage2Dialogue?.ArtifactCompletion();
            Debug.Log("All artifacts collected!");

            // Disable the tracker GameObject
            if (tracker != null)
            {
                tracker.SetActive(false);
                Debug.Log("Tracker disabled after collecting all artifacts.");
            }
            else
            {
                Debug.LogWarning("Tracker GameObject is not assigned!");
            }

            SaveProgress();
            // Additional logic for completing artifact objectives
            CheckObjectiveCompletion(currentLevel, enemiesDefeated);
        }
    }

    /// <summary>
    ///              OBJECTIVES AND REWARDS SYSTEM FOR STAGE2
    /// </summary>
    protected override bool CheckObjectiveCompletion(int currentLevel, int enemiesDefeated)
    {
        Debug.Log($"[CheckObjectiveCompletion] Entering for Level: {currentLevel}");
        Debug.Log($"ArtifactsClaimed: {artifactsClaimed}, Required: {requiredArtifacts}, EnemiesDefeated: {enemiesDefeated}");

        bool objectiveCompleted = currentLevel switch
        {
            1 => artifactsClaimed >= 3, // Requires 3 artifacts for level 1
            2 => artifactsClaimed >= requiredArtifacts, // Requires all artifacts for level 2
            3 => enemiesDefeated >= 1, // Defeat 1 mini-boss for level 3
            4 => enemiesDefeated >= 3, // Defeat 5 enemies for level 4
            5 => enemiesDefeated >= 1, // Defeat final boss for level 5
            _ => false
        };

        if (objectiveCompleted)
        {
            Debug.Log($"Objective completed for level {currentLevel}");
            ShowRewardPanel(currentLevel); // Add a log inside ShowRewardPanel
        }
        else
        {
            Debug.Log($"Objective NOT completed for level {currentLevel}");
        }

        return objectiveCompleted;
    }

    private void HandleEnemyDefeat(GameObject defeatedEnemy)
    {

        enemiesDefeated++;
        Debug.Log($"Enemy defeated! Total defeated: {enemiesDefeated}");

        if (activeEnemies.Contains(defeatedEnemy))
        {
            activeEnemies.Remove(defeatedEnemy);
            Destroy(defeatedEnemy);
        }

        CheckObjectiveCompletion(currentLevel, enemiesDefeated); // Re-check completion status
    }
    protected override void ShowRewardPanel(int levelIndex)
    {
        int arrayIndex = levelIndex - 1; // Match levelIndex to rewardPanels index
        Debug.Log($"Showing reward panel for level {levelIndex}");
        if (arrayIndex < 0 || arrayIndex >= rewardPanels.Length) return;

        Debug.Log($"Showing reward panel for level {levelIndex}");
        rewardPanels[arrayIndex].SetActive(true); // Activate the appropriate reward panel

        // Assign rewards based on the level
        switch (levelIndex)
        {
            case 1: // Level 1 rewards
                GameDataManager.Instance.AddPotions(1, 1);
                playerController.playerStats.AddExp(100);
                break;

            case 2: // Level 2 rewards
                GameDataManager.Instance.AddPotions(2, 2);
                playerController.playerStats.AddExp(100);
                break;

            case 3: // Level 3 rewards
                GameDataManager.Instance.AddPotions(3, 3);
                playerController.playerStats.AddExp(100);
                break;

            case 4: // Level 4 rewards
                GameDataManager.Instance.AddPotions(5, 5);
                playerController.playerStats.AddExp(100);
                break;

            case 5: // Level 5 rewards
                GameDataManager.Instance.AddPotions(6, 6);
                playerController.playerStats.AddExp(100);
                break;
        }
        SetupContinueButton(arrayIndex, levelIndex); // Configure continue button with level index
    }
 
    private IEnumerator ShowIndicatorPanel(int level)
    {
        int panelIndex = level - 1;
        if (panelIndex < 0 || panelIndex >= indicatorPanels.Length) yield break;

        Debug.Log($"[ShowIndicatorPanelCoroutine] Showing panel for level {level}");
        indicatorPanels[panelIndex].SetActive(true); // Show indicator panel
        yield return new WaitForSeconds(2); // Keep the indicator panel visible for 2 seconds
        indicatorPanels[panelIndex].SetActive(false); // Hide the indicator panel
    }

    private void SetupContinueButton(int rewardIndex, int levelIndex)
    {
        continueButton.gameObject.SetActive(true); // Activate the continue button
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            rewardPanels[rewardIndex].SetActive(false); // Hide the current reward panel
            continueButton.gameObject.SetActive(false); // Deactivate the continue button

            // Manage wall sets
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
                    ActivateWallSet(WallSet5, null); // Final wall, no next wall
                    break;
                default:
                    Debug.LogWarning($"Unexpected level index: {levelIndex}");
                    break;
            }

            currentLevel++; // Increment the level after reward handling
            enemiesDefeated = 0; // Reset enemies defeated count for the next level
            StartCoroutine(ShowIndicatorPanel(currentLevel)); // Show the next level indicator
        });
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

    public override void EnemyDefeated(GameObject defeatedEnemy)
    {
        enemiesDefeated++;
        CheckObjectiveCompletion(currentLevel, enemiesDefeated);
    }

    /// <summary>
    ///          DIALOGUE SYSTEM FOR STAGE2
    /// </summary>
    public override void ActivateZone(int zoneIndex)
    {
        Debug.Log($"ActivateZone triggered for zoneIndex: {zoneIndex}");

        // Calculate the array index
        int arrayIndex = zoneIndex - 1;

        // Ensure the array index is valid
        if (arrayIndex < 0 || arrayIndex >= zones.Length)
        {
            Debug.LogError($"Invalid Zone Index: {zoneIndex}");
            return;
        }

        // Get the corresponding zone object
        GameObject zoneObject = zones[arrayIndex];

        // Handle special zone cases
        switch (zoneIndex)
        {
            case 1:
                Debug.Log("Zone 3: Starting RForm Encounter.");
                StartRazourmouthEncounter();
                DestroyZone(zoneObject, arrayIndex); // Destroy immediately
                return;

            case 2:
                Debug.Log("Zone 4: Starting Razormouth Encounter.");
                StartRForm();
                DestroyZone(zoneObject, arrayIndex); // Destroy immediately
                return;

            default:
                // Check if the objective for this zone is complete
                if (CheckObjectiveCompletion(zoneIndex, enemiesDefeated))
                {
                    Debug.Log($"Objective completed for zoneIndex: {zoneIndex}");
                    DestroyZone(zoneObject, arrayIndex);
                }
                else
                {
                    Debug.Log($"Objective not completed for zoneIndex: {zoneIndex}. Zone remains active.");
                }
                break;
        }
    }
    private void DestroyZone(GameObject zoneObject, int arrayIndex)
    {
        if (zoneObject == null)
        {
            Debug.LogWarning($"Zone at array index {arrayIndex} is null or already destroyed.");
            return;
        }

        Destroy(zoneObject); // Destroy the zone
        Debug.Log($"Zone at array index {arrayIndex} has been destroyed.");
    }
    private void StartRazourmouthEncounter()
    {
        stage2Dialogue?.StartRazormouth();
    }

    private void StartRForm()
    {
        stage2Dialogue?.StartRform();
    }

    public void PauseGame() => Time.timeScale = 0f;
    public void ResumeGame() => Time.timeScale = 1f;
    //others
    public override void InitializeReferences()
    {
        base.InitializeReferences(); // Call the base implementation

        // Add any additional initialization logic specific to GameManager2
        if (stage2Dialogue == null)
            stage2Dialogue = FindObjectOfType<Stage2Dialogue>();

        if (tracker == null)
            tracker = GameObject.Find("Tracker");

        Debug.Log("[GameManager2] References initialized.");
    }



    private void SaveProgress()
    {
        GameDataManager.Instance.SaveProgress(
            stage: 2,
            position: playerController.transform.position,
            health: playerStats.CurrentHealth,
            mana: playerStats.CurrentMana,
            level: currentLevel,
            artifacts: artifactsClaimed,
            enemies: enemiesDefeated
        );
    }
}