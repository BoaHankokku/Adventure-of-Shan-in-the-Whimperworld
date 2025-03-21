using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.CurrentSceneType = GameDataManager.SceneType.MainMenu;
            Debug.Log("SceneType set to MainMenu.");
        }
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
