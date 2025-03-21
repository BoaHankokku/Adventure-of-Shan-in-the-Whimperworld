using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ClearSky.Enemy;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject selectionPanel;
    public GameObject howToPlayPanel;
    public GameObject optionsPanel;
    public GameObject startNewGameConfirmationPanel;

    [Header("Buttons for Selection Panel")]
    public Button startGameButton; // Start Game / Start New Game Button
    public Button continueButton; // Continue Button (appears if there's saved progress)
    public Button howToPlayButton; // Button to show "How To Play" panel
    public Button howToPlayBackButton; // Button to return from "How To Play" panel to selection
    public Button optionsButton; // Button to show "Options" panel
    public Button optionsBackButton; // Button to return from "Options" panel to selection
    public Button quitButton; // Quit button to exit the game

    [Header("Buttons for Start New Game Confirmation")]
    public Button confirmStartNewGameButton; // Button to confirm starting a new game (overwriting progress)
    public Button continueLastSaveButton; // Button to continue the last saved game
    public Button backToSelectionButton; // Button to go back from the confirmation panel to selection panel

    private const string SavedSceneKey = "LastSavedScene";
    private const string GameStartedKey = "GameStarted";

    private bool hasSaveData;

    void Start()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.CurrentSceneType = GameDataManager.SceneType.MainMenu;
            Debug.Log("SceneType set to MainMenu.");
        }
        SetupUI();
        ShowPanel(selectionPanel); // Show the main selection panel by default
        UpdateUIState(); // Update the button states based on the saved progress
    }

    private void SetupUI()
    {
        // Main menu button listeners
        startGameButton.onClick.AddListener(HandleStartGame);
        continueButton.onClick.AddListener(ContinueGame);
        howToPlayButton.onClick.AddListener(() => ShowPanel(howToPlayPanel));
        howToPlayBackButton.onClick.AddListener(() => ShowPanel(selectionPanel));
        optionsButton.onClick.AddListener(() => ShowPanel(optionsPanel));
        optionsBackButton.onClick.AddListener(() => ShowPanel(selectionPanel));
        quitButton.onClick.AddListener(QuitGame);

        // Confirmation panel button listeners
        confirmStartNewGameButton.onClick.AddListener(StartNewGame);
        continueLastSaveButton.onClick.AddListener(ContinueGame); // Continue if the user disagrees
        backToSelectionButton.onClick.AddListener(() => ShowPanel(selectionPanel));
    }

    private void UpdateUIState()
    {
        if (startGameButton == null || continueButton == null)
        {
            Debug.LogError("StartGameButton or ContinueButton is not assigned.");
            return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager instance is null. Ensure it is instantiated in the Bootstrap scene.");
            return;
        }

        hasSaveData = SaveManager.Instance.HasSaveData();
        Debug.Log($"UpdateUIState called. HasSaveData: {hasSaveData}");

        startGameButton.GetComponentInChildren<TextMeshProUGUI>().text = hasSaveData ? "START NEW GAME" : "START GAME";
        continueButton.gameObject.SetActive(hasSaveData);
    }



    private void HandleStartGame()
    {
        hasSaveData = SaveManager.Instance.HasSaveData();

        if (hasSaveData)
        {
            // If save data exists, show confirmation panel to overwrite
            ShowPanel(startNewGameConfirmationPanel);
        }
        else
        {
            // No save data, start a new game immediately
            PlayGame();
        }
    }

    private void PlayGame()
    {
        PlayerPrefs.SetInt(GameStartedKey, 1); // Mark game as started
        PlayerPrefs.Save();
        SceneManager.LoadScene("Cutscene1"); // Transition to the first cutscene
    }

    private void StartNewGame()
    {
        GameDataManager.Instance.ClearProgress(); // Clear all saved progress
        PlayGame(); // Begin the game from the first cutscene
    }

    private void ContinueGame()
    {
        if (PlayerPrefs.HasKey("LastSavedScene"))
        {
            string lastSavedScene = PlayerPrefs.GetString("LastSavedScene");
            Debug.Log($"Continuing game from scene: {lastSavedScene}");

            // Subscribe to sceneLoaded event to ensure data is loaded after scene is fully initialized
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Load the saved scene
            SceneManager.LoadScene(lastSavedScene);
        }
        else
        {
            Debug.LogWarning("No save data available. Starting a new game.");
            PlayGame(); // Start a new game if no save data exists
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe to avoid duplicate calls
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Load saved gameplay state
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.LoadGame();
        }

        // Call to disable defeated enemies
        EnemyBase.DisableAllDefeatedEnemies();

        // Add this line here
        PlayerPrefs.Save();
        Debug.Log("Player progress saved after scene load.");
    }

    private void ShowPanel(GameObject panelToShow)
    {
        // Hide all panels
        selectionPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        optionsPanel.SetActive(false);
        startNewGameConfirmationPanel.SetActive(false);

        // Show the requested panel
        panelToShow.SetActive(true);
        if (panelToShow == selectionPanel)
        {
            UpdateUIState(); // Refresh the "Continue" button and other UI elements
        }
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
