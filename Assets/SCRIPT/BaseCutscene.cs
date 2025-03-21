using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class BaseCutsceneManager : MonoBehaviour
{
    // Global Cutscene State with Scene-Specific Ranges
    public enum GlobalCutsceneState
    {
        None = 0,
        Dialogue1_Cutscene1 = 1,
        Dialogue1_Cutscene29 = 30,
        Dialogue2_Cutscene1 = 31,
        Dialogue2_Cutscene9 = 39,
        Dialogue3_Cutscene3 = 40,
        Dialogue3_Cutscene9 = 47,
        // Add more ranges for other scenes
    }

    [Header("Common Cutscene Settings")]
    public RectTransform dialogueBoxTransform;
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.05f;
    public Vector2 rightPosition = new Vector2(451, 26);
    public Vector2 bottomPosition = new Vector2(-88, -344);
    public GameObject[] cutscenePanels;
    public Button SkipButton;
    public string targetSceneName;

    [Header("Dialogue Positions")]
    public int[] rightPositionDialogues;

    protected string fullText;
    protected string currentText = "";
    protected Coroutine typingCoroutine;
    protected bool isTyping = false;
    protected bool isFastTyping = false;

    protected GlobalCutsceneState currentState = GlobalCutsceneState.None;

    private const string CutsceneProgressKey = "CutsceneProgress";

    // Shared Method: Start Cutscene
    protected virtual void Start()
    {
        if (SceneManager.GetActiveScene().name.Contains("Cutscene"))
        {
            Debug.Log("[BaseCutsceneManager] Running in Cutscene. No need for GameDataManager.");
        }

        if (SkipButton != null)
        {
            // Bind the skip button to end the cutscene
            SkipButton.onClick.AddListener(EndCutscene);
        }
        else
        {
            Debug.LogWarning("SkipButton is not assigned in the Inspector.");
        }
    }

    public void ShowCutscenePanel(int cutsceneIndex)
    {
        foreach (var panel in cutscenePanels)
        {
            panel.SetActive(false); // Hide all panels
        }

        if (cutsceneIndex > 0 && cutsceneIndex <= cutscenePanels.Length)
        {
            cutscenePanels[cutsceneIndex - 1].SetActive(true); // Show the specific panel
        }
    }
    public virtual void StartCutscene(int cutsceneIndex, string[] dialogues, GlobalCutsceneState startRange)
    {
        int rangeStart = (int)startRange;
        int rangeEnd = rangeStart + dialogues.Length - 1;

        if (cutsceneIndex < rangeStart || cutsceneIndex > rangeEnd)
        {
            Debug.LogWarning($"Invalid cutscene index. Must be between {rangeStart} and {rangeEnd}.");
            return;
        }

        currentState = (GlobalCutsceneState)cutsceneIndex;
        SaveProgress(); // Save progress whenever a new cutscene starts

        ShowCutscenePanel(cutsceneIndex - rangeStart + 1);

        SetDialoguePosition(cutsceneIndex);

        StartTypewriterEffect(dialogues[cutsceneIndex - rangeStart]);

        Debug.Log($"Started cutscene {cutsceneIndex} in range {rangeStart} to {rangeEnd}. Showing panel {cutsceneIndex - rangeStart + 1}.");
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[BaseCutsceneManager] SaveManager is NULL! Cannot save cutscene progress.");
        }
        else
        {
            SaveManager.Instance.SaveGame(null, (int)currentState);
        }

    }

    // Shared Method: Typewriter Effect
    public void StartTypewriterEffect(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("Attempting to display an empty or null dialogue.");
            dialogueText.text = "";
            return;
        }

        fullText = text;
        currentText = "";
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        isFastTyping = false;

        for (int i = 0; i <= fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i);
            dialogueText.text = currentText;
            yield return new WaitForSeconds(isFastTyping ? typingSpeed / 10f : typingSpeed);
        }

        isTyping = false;
    }

    // Shared Method: Set Dialogue Box Position
    protected void SetDialoguePosition(int cutsceneIndex)
    {
        if (System.Array.Exists(rightPositionDialogues, index => index == cutsceneIndex))
        {
            dialogueBoxTransform.anchoredPosition = rightPosition;
            dialogueText.color = Color.white; // White text for right position
            dialogueText.alignment = TextAlignmentOptions.Center; // Center alignment
            dialogueText.fontSize = 60f;
            Debug.Log($"Dialogue {cutsceneIndex} positioned on the right.");
        }
        else
        {
            dialogueBoxTransform.anchoredPosition = bottomPosition;
            dialogueText.color = Color.black; // Black text for bottom position
            dialogueText.alignment = TextAlignmentOptions.Justified; // Justified alignment
            dialogueText.fontSize = 50f;
            Debug.Log($"Dialogue {cutsceneIndex} positioned at the bottom.");
        }
    }

    // Shared Method: Save Progress
    public void SaveProgress()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastSavedScene", currentScene); // Save the current scene
        PlayerPrefs.Save();
        Debug.Log($"Progress saved for scene: {currentScene}");
    }


    // Shared Method: Load Progress
    public GlobalCutsceneState LoadProgress()
    {
        if (PlayerPrefs.HasKey(CutsceneProgressKey))
        {
            int savedState = PlayerPrefs.GetInt(CutsceneProgressKey);
            Debug.Log($"Progress loaded: {(GlobalCutsceneState)savedState}");
            return (GlobalCutsceneState)savedState;
        }
        return GlobalCutsceneState.None; // Default state if no progress is saved
    }

    // Shared Method: Stop Cutscene (Optional)
    public void StopCutscene()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        currentText = "";
        dialogueText.text = "";
        Debug.Log("Cutscene stopped.");
    }

    // Shared Method: End Cutscene
    public virtual void EndCutscene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("No target scene specified in the Inspector.");
            return;
        }

        SaveManager.Instance.SaveGame(null, (int)currentState);
        SceneManager.LoadScene(targetSceneName);
        Debug.Log($"Cutscene skipped. Transitioning to scene: {targetSceneName}");
    }
}
