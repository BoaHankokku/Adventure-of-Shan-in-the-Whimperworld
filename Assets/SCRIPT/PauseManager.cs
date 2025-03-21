using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject PausePanel;
    public GameObject OptionsPanel;
    public GameObject BackToMainMenuPanel;

    [Header("Options UI")]
    public Slider OptionsMusicSlider;
    public TMP_Dropdown OptionsGraphicsDropdown;
    public Button OptionsBackButton;

    [Header("Pause UI")]
    public Button ContinueButton;
    public Button SaveGameButton;
    public Button OptionsButton;
    public Button BackToMainMenuButton;
    public TextMeshProUGUI SaveGameText;
    public Button PauseButton;

    [Header("Back to Main Menu UI")]
    public Button ConfirmBackToMainMenuButton;
    public Button GoBackAndSaveButton;
    public Button ExitGameButton;

    private bool isPaused = false;

    void Start()
    {
        ContinueButton.onClick.AddListener(ContinueGame);
        SaveGameButton.onClick.AddListener(SaveGame);
        OptionsButton.onClick.AddListener(ShowOptionsPanel);
        BackToMainMenuButton.onClick.AddListener(ShowBackToMainMenuPanel);
        PauseButton.onClick.AddListener(ShowPauseMenu);

        ConfirmBackToMainMenuButton.onClick.AddListener(ContinueGame);
        GoBackAndSaveButton.onClick.AddListener(SaveAndBackToMainMenu);
        ExitGameButton.onClick.AddListener(ExitGame);

        OptionsBackButton.onClick.AddListener(BackToPausePanel);
        OptionsMusicSlider.onValueChanged.AddListener(SetMusicVolume);
        OptionsGraphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);
        OptionsGraphicsDropdown.value = QualitySettings.GetQualityLevel();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ContinueGame(); // Resume the game
            }
            else
            {
                ShowPauseMenu(); // Show the pause menu
            }
        }
    }

    public void ShowPauseMenu()
    {
        isPaused = true;
        PausePanel.SetActive(true);
        Time.timeScale = 0f;

        // Reset Save Game Text
        if (SaveGameText != null)
            SaveGameText.text = "Save Game"; // Reset text when opening the pause menu
    }
    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveGameText.text = "Saving Game...";
            SaveManager.Instance.SaveGame(SaveGameText); // Save and update the text
            Debug.Log("Game progress successfully overwritten.");
        }
        else
        {
            SaveGameText.text = "Save Failed";
            Debug.LogError("SaveManager instance is null. Ensure it's properly initialized.");
        }
    }

    public void ContinueGame()
    {
        isPaused = false;
        PausePanel.SetActive(false);
        BackToMainMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }


    public void ShowOptionsPanel()
    {
        PausePanel.SetActive(false);
        OptionsPanel.SetActive(true);
    }

    public void BackToPausePanel()
    {
        OptionsPanel.SetActive(false);
        PausePanel.SetActive(true);
    }

    public void ShowBackToMainMenuPanel()
    {
        PausePanel.SetActive(false);
        BackToMainMenuPanel.SetActive(true);
    }

    public void SaveAndBackToMainMenu()
    {
        SaveGame();
        BackToMainMenu();
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.instance.SetMusicVolume(volume);
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
    }

}
