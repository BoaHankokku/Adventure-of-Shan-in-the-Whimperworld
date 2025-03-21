using UnityEngine;

public class BaseCutsceneManager : MonoBehaviour
{
    protected int currentCutsceneIndex;
    protected int currentDialogueIndex;

    protected virtual void SaveCutsceneState(int cutsceneNumber)
    {
        PlayerPrefs.SetInt($"Cutscene{cutsceneNumber}Index", currentCutsceneIndex);
        PlayerPrefs.SetInt($"Cutscene{cutsceneNumber}DialogueIndex", currentDialogueIndex);
        PlayerPrefs.Save();
        Debug.Log($"Cutscene {cutsceneNumber} state saved.");
    }

    protected virtual void LoadCutsceneState(int cutsceneNumber)
    {
        if (PlayerPrefs.HasKey($"Cutscene{cutsceneNumber}Index") && PlayerPrefs.HasKey($"Cutscene{cutsceneNumber}DialogueIndex"))
        {
            currentCutsceneIndex = PlayerPrefs.GetInt($"Cutscene{cutsceneNumber}Index");
            currentDialogueIndex = PlayerPrefs.GetInt($"Cutscene{cutsceneNumber}DialogueIndex");
            Debug.Log($"Cutscene {cutsceneNumber} state loaded. Cutscene Index: {currentCutsceneIndex}, Dialogue Index: {currentDialogueIndex}");
            StartCutscene(currentCutsceneIndex);
        }
        else
        {
            Debug.Log($"No saved state found for Cutscene {cutsceneNumber}. Starting from the beginning.");
            StartCutscene(1);
        }
    }

    protected virtual void StartCutscene(int cutsceneNumber)
    {
        currentCutsceneIndex = cutsceneNumber;
        currentDialogueIndex = 0; // Reset dialogue index for new cutscene
        SaveCutsceneState(cutsceneNumber); // Save state whenever a new cutscene starts

        // Implement the logic to start the cutscene in the derived class
    }
}
