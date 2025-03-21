using UnityEngine;

public class BootstrapManager : MonoBehaviour
{
    public GameObject saveManagerPrefab;
    public GameObject gameDataManagerPrefab;

    private void Awake()
    {
        if (SaveManager.Instance == null && saveManagerPrefab != null)
        {
            Instantiate(saveManagerPrefab);
        }
        if (GameDataManager.Instance == null && gameDataManagerPrefab != null)
        {
            Instantiate(gameDataManagerPrefab);
        }

        InitializeGameState(); // Call the game state initialization

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    private void Start()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.CurrentSceneType = GameDataManager.SceneType.MainMenu;
            Debug.Log("SceneType set to MainMenu.");
        }

    }
    private void InitializeGameState()
    {
        if (!PlayerPrefs.HasKey("LastSavedScene"))
        {
            PlayerPrefs.SetString("LastSavedScene", ""); // Clear saved scene on first launch
            PlayerPrefs.Save();
        }
    }
}
