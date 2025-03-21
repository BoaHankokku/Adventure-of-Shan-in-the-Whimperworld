using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using ClearSky.Controller;
using ClearSky.Player;
using UnityEngine.SceneManagement;
using ClearSky.Enemy;
using ES3Types;



public abstract class BaseGameManager : MonoBehaviour
{
    public static BaseGameManager Instance { get; protected set; }

    [Header("Player & Dialogue")]
    public SimplePlayerController playerController;
    public PlayerStats playerStats;

    [Header("Level Tracking")]
    public int currentLevel = 0;

    [Header("Enemies Defeated")]
    public int enemiesDefeated = 0;

    [Header("Wall Sets")]
    public GameObject WallSet1;
    public GameObject WallSet2;
    public GameObject WallSet3;
    public GameObject WallSet4;
    public GameObject WallSet5;

    [Header("UI")]
    public Button continueButton;
    public GameObject[] rewardPanels;
    public GameObject[] indicatorPanels;

    [Header("Barrel System")]
    public GameObject confirmationPanel;
    public TextMeshProUGUI confirmationText;
    public Button yesButton;
    public Button noButton;
    public int currentBarrelIndex;
    public virtual int ArtifactsClaimed => 0;

    // Abstract Methods to Implement in Subclasses
    public abstract void EnemyDefeated(GameObject defeatedEnemy);
    protected abstract bool CheckObjectiveCompletion(int currentLevel, int enemiesDefeated);
    protected abstract void ShowRewardPanel(int levelIndex);
    protected abstract void ActivateNextWallSet(int levelIndex);
    public abstract void ActivateZone(int zoneIndex);
    protected virtual void Awake()
    {
        if (Instance == null || Instance.GetType() != this.GetType())
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log($"[BaseGameManager] Instance assigned to {this.GetType().Name}");
        }
        else
        {
            Debug.LogWarning($"[BaseGameManager] Duplicate instance detected. Destroying this instance.");
            Destroy(gameObject);
        }
    }
    protected virtual void Start()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.CurrentSceneType = GameDataManager.SceneType.Gameplay;
            Debug.Log("SceneType set to Gameplay.");
        }
        InitializeWalls();
    }
    /// <summary>
    ///                    SCENE MANAGEMENT
    /// </summary>
    protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeWalls();
    }
    protected virtual void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    /// <summary>
    ///                    WALLS, LEVELS AND REWARDS
    /// </summary>
   public virtual void InitializeWalls()
    {
        WallSet1?.SetActive(true); // Activate the first wall
        WallSet2?.SetActive(false);
        WallSet3?.SetActive(false);
        WallSet4?.SetActive(false);
        WallSet5?.SetActive(false);

        Debug.Log("[BaseGameManager] Walls initialized. WallSet1 is active, others inactive.");
    }
    protected virtual void ActivateWallSet(GameObject currentWallSet, GameObject nextWallSet)
    {
        if (currentWallSet != null) currentWallSet.SetActive(false);
        if (nextWallSet != null) nextWallSet.SetActive(true);
    }
    protected virtual void LoadWallState(int activeWallIndex)
    {
        switch (activeWallIndex)
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
                Debug.LogWarning("[BaseGameManager] Invalid wall state index.");
                break;
        }
    }

    protected virtual void SetupContinueButton(int rewardIndex, int levelIndex)
    {
        if (continueButton == null)
        {
            Debug.LogError("[BaseGameManager] ContinueButton is null. Ensure it is properly initialized.");
            return;
        }

        if (rewardPanels == null || rewardIndex < 0 || rewardIndex >= rewardPanels.Length)
        {
            Debug.LogError("[BaseGameManager] Invalid reward panel index.");
            return;
        }

        // Configure the continue button
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            rewardPanels[rewardIndex]?.SetActive(false); // Hide the reward panel
            continueButton.gameObject.SetActive(false); // Deactivate the continue button

            ActivateNextWallSet(levelIndex); // Activate the next wall set
            currentLevel++;
            enemiesDefeated = 0;

            Debug.Log($"[BaseGameManager] Continue to level {currentLevel}.");
        });

        Debug.Log("[BaseGameManager] Continue button setup completed.");
    }

    protected virtual IEnumerator ShowIndicatorPanel(int level)
    {
        if (level <= 0 || level > indicatorPanels.Length) yield break;

        indicatorPanels[level - 1]?.SetActive(true);
        yield return new WaitForSeconds(2);
        indicatorPanels[level - 1]?.SetActive(false);
    }


    /// <summary>
    ///                     FOR STAGE 2
    /// </summary>
   
 
    public virtual void HandleBarrelCollision(Collider2D barrelCollider, Collider2D[] barrelColliders)
    {
        int barrelIndex = GetBarrelIndex(barrelCollider, barrelColliders);
        if (barrelIndex >= 0)
        {
            currentBarrelIndex = barrelIndex;
            ShowConfirmationPanel();
        }
        else
        {
            Debug.LogWarning("Barrel collider not recognized.");
        }
    }
    protected void ShowConfirmationPanel()
    {
        if (confirmationPanel == null || confirmationText == null)
        {
            Debug.LogError("Confirmation panel or text is not assigned in the Inspector!");
            return;
        }

        confirmationText.text = "Encountered a barrel. Do you wish to search it for artifacts?";
        confirmationPanel.SetActive(true);

        yesButton?.onClick.RemoveAllListeners();
        yesButton?.onClick.AddListener(OnYesButtonClicked);

        noButton?.onClick.RemoveAllListeners();
        noButton?.onClick.AddListener(OnNoButtonClicked);
    }
    protected virtual void OnYesButtonClicked()
    {
        Debug.Log($"Yes Button clicked. Current Barrel Index: {currentBarrelIndex}");
        // Barrel-specific logic should be implemented in derived classes
    }
    protected virtual void OnNoButtonClicked()
    {
        Debug.Log("No Button clicked. Ignoring barrel interaction.");
        CloseConfirmationPanel();
    }
    protected void CloseConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }
    protected virtual void OnBarrelCollision(int barrelIndex)
    {
        Debug.Log($"BaseGameManager: Barrel collision detected at index {barrelIndex}. Override this method in derived classes.");
    }
    protected int GetBarrelIndex(Collider2D barrelCollider, Collider2D[] barrelColliders)
    {
        for (int i = 0; i < barrelColliders.Length; i++)
        {
            if (barrelColliders[i] == barrelCollider)
            {
                return i;
            }
        }
        return -1;
    }

    public virtual void ProceedAfterInitialDialogue()
    {
        Debug.Log("[BaseGameManager] ProceedAfterInitialDialogue not implemented.");
    }

    /// <summary>
    ///                   INITIALIZE REFERENCES
    /// </summary>
    public virtual void InitializeReferences()
    {
        playerController = FindObjectOfType<SimplePlayerController>();
        playerStats = FindObjectOfType<PlayerStats>();

        if (continueButton == null)
        {
            GameObject continueButtonObj = GameObject.Find("Continuebtn");
            if (continueButtonObj != null)
            {
                continueButton = continueButtonObj.GetComponent<Button>();
            }
            else
            {
                Debug.LogWarning("[BaseGameManager] Continue button not found in scene!");
            }
        }

        // Reinitialize Reward Panels
        if (rewardPanels == null || rewardPanels.Length == 0)
        {
            GameObject rewardContainer = GameObject.Find("RewardPanelContainer");
            if (rewardContainer != null)
            {
                rewardPanels = new GameObject[rewardContainer.transform.childCount];
                for (int i = 0; i < rewardPanels.Length; i++)
                {
                    rewardPanels[i] = rewardContainer.transform.GetChild(i).gameObject;
                }
                Debug.Log("[BaseGameManager] Reward panels reinitialized.");
            }
            else
            {
                Debug.LogWarning("[BaseGameManager] Reward panel container not found!");
            }
        }

        // Reinitialize Indicator Panels
        if (indicatorPanels == null || indicatorPanels.Length == 0)
        {
            GameObject indicatorContainer = GameObject.Find("IndicatorPanelContainer");
            if (indicatorContainer != null)
            {
                indicatorPanels = new GameObject[indicatorContainer.transform.childCount];
                for (int i = 0; i < indicatorPanels.Length; i++)
                {
                    indicatorPanels[i] = indicatorContainer.transform.GetChild(i).gameObject;
                }
                Debug.Log("[BaseGameManager] Indicator panels reinitialized.");
            }
            else
            {
                Debug.LogWarning("[BaseGameManager] Indicator panel container not found!");
            }
        }

        Debug.Log("[BaseGameManager] References initialized after load.");
    }

}
