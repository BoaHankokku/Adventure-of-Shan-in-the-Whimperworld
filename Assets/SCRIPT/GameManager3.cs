using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using ClearSky.Controller;
using ClearSky.Player;
using UnityEngine.SceneManagement;

public class GameManager3 : BaseGameManager
{
    [Header("Player & Dialogue")]
    public Stage3Dialogue stage3Dialogue;
    

    protected override void Start()
    {
        base.Start(); // Call the BaseGameManager's Start method
        InitializeGame();
        LoadPlayerState();
        stage3Dialogue.StartInitialDialogue();
    }

    private void InitializeGame()
    {
        foreach (var panel in rewardPanels) panel.SetActive(false);
        foreach (var panel in indicatorPanels) panel.SetActive(false);
    }

    public override void ProceedAfterInitialDialogue()
    {
        Debug.Log("Proceeding after initial dialogue.");
        currentLevel = 1;
    }

    private IEnumerator HandlePostDialogueTransition()
    {
        yield return new WaitUntil(() => !stage3Dialogue.dialoguePanel.activeSelf);
        currentLevel = 1;
        StartCoroutine(ShowIndicatorPanel(currentLevel));
    }

    private IEnumerator ShowIndicatorPanel(int level)
    {
        if (level - 1 < indicatorPanels.Length)
        {
            indicatorPanels[level - 1].SetActive(true);
            yield return new WaitForSeconds(2);
            indicatorPanels[level - 1].SetActive(false);
        }
        else
        {
            Debug.LogWarning("Invalid level index for indicator panel.");
        }
    }

    public override void EnemyDefeated(GameObject defeatedEnemy)
    {
        enemiesDefeated++;
        Debug.Log($"Enemies defeated: {enemiesDefeated}");
        CheckObjectiveCompletion(currentLevel, enemiesDefeated);
    }

    protected override bool CheckObjectiveCompletion(int currentLevel, int enemiesDefeated)
    {
        bool objectiveCompleted = currentLevel switch
        {
            1 => enemiesDefeated >= 2,
            2 => enemiesDefeated >= 2,
            3 => enemiesDefeated >= 1,
            4 => enemiesDefeated >= 3,
            5 => enemiesDefeated >= 1,
            _ => false
        };

        if (objectiveCompleted)
        {
            ShowRewardPanel(currentLevel);
        }

        return objectiveCompleted;
    }

    protected override void ShowRewardPanel(int levelIndex)
    {
        int panelIndex = levelIndex - 1;
        if (panelIndex < 0 || panelIndex >= rewardPanels.Length) return;

        rewardPanels[panelIndex].SetActive(true);
        GrantRewards(levelIndex);

        SavePlayerState();
        SetupContinueButton(panelIndex, levelIndex);
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

    private void GrantRewards(int levelIndex)
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
            default:
                Debug.LogWarning("Unexpected level index for rewards.");
                break;
        }
    }

    private void SetupContinueButton(int rewardIndex, int levelIndex)
    {
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            rewardPanels[rewardIndex].SetActive(false);
            continueButton.gameObject.SetActive(false);

            currentLevel++;
            enemiesDefeated = 0;
            StartCoroutine(ShowIndicatorPanel(currentLevel));
        });
    }


    public override void ActivateZone(int zoneIndex)
    {
        switch (zoneIndex)
        {
            case 1:
                TriggerHeartDialogue();
                break;
            case 2:
                TriggerRFormEncounter();
                break;
            default:
                Debug.LogWarning("Invalid zone index.");
                break;
        }
    }

    private void TriggerHeartDialogue()
    {
        stage3Dialogue?.StartHeartboss();
    }

    private void TriggerRFormEncounter()
    {
        stage3Dialogue?.StartRform();
    }

    private void SavePlayerState()
    {
        GameDataManager.Instance.SavePlayerTransform(playerController.transform);
        GameDataManager.Instance.SavePlayerStats();
    }

    public override void InitializeReferences()
    {
        base.InitializeReferences(); // Call the base implementation

        // Add any additional initialization logic specific to GameManager3
        if (stage3Dialogue == null)
            stage3Dialogue = FindObjectOfType<Stage3Dialogue>();

        Debug.Log("[GameManager3] References initialized.");
    }

    private void LoadPlayerState()
    {
        Vector3 position;
        float health, mana;
        int level, artifacts, enemies;

        GameDataManager.Instance.LoadProgress(out position, out health, out mana, out level, out artifacts, out enemies);

        playerController.transform.position = position;
        playerStats.health = health;
        playerStats.mana = mana;
        currentLevel = level;
        enemiesDefeated = enemies;

        Debug.Log("Player state loaded successfully.");
    }
}
