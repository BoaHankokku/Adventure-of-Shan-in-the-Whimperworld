using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Cutscene2Dialogue : BaseCutsceneManager
{
    private string[] cutsceneDialogues = new string[]
    {
        "Shan woke up again next to her mother... wondering if that dream is real.",
        "Her mother prepared for breakfast, excitedly wondered what her mom had prepared, hoping for veggies...",
        "I wonder what mommy prepared for breakfast, I hope it's veggies...",
        "Shan received a piece of chocolate as a reward from her mom for being good.",
        "You must clean your teeth after eating that.",
        "Her mom reminds her as she prepared to water the plants in the backyard.",
        "When I come back, you should be done cleaning your teeth.",
        "I will just say I'm done. When in fact, I'm totally not, she thought to herself.",
        "It was getting late at night, and Shan was ready to go to sleep. Little did she know she will experience another nightmare again..."
    };

    public override void StartCutscene(int cutsceneIndex, string[] dialogues, GlobalCutsceneState startRange)
    {
        int rangeStart = (int)startRange; // Align with Dialogue2_Cutscene1 range start
        int rangeEnd = rangeStart + dialogues.Length - 1;

        if (cutsceneIndex < rangeStart || cutsceneIndex > rangeEnd)
        {
            Debug.LogWarning($"Invalid cutscene index: {cutsceneIndex}. Must be between {rangeStart} and {rangeEnd}.");
            return;
        }

        base.StartCutscene(cutsceneIndex, dialogues, startRange);
    }

    private void Start()
    {
        base.Start();

        if (cutscenePanels == null || cutscenePanels.Length == 0)
        {
            Debug.LogError("Cutscene panels not assigned.");
            return;
        }

        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            int savedState = PlayerPrefs.GetInt("CutsceneProgress", -1);
            if (savedState >= 31) // Ensure it starts from the correct cutscene range
            {
                Debug.Log($"Loaded saved cutscene progress: {savedState}");
                StartCutscene(savedState, cutsceneDialogues, GlobalCutsceneState.Dialogue2_Cutscene1);
                return;
            }
        }

        // Ensure it starts at 31 if no saved data exists
        StartCutscene(31, cutsceneDialogues, GlobalCutsceneState.Dialogue2_Cutscene1);

        // Save progress immediately
        SaveManager.Instance?.SaveGame(null, (int)GlobalCutsceneState.Dialogue2_Cutscene1);
    }




    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                isFastTyping = true;
            }
            else
            {
                int nextCutscene = (int)currentState + 1;
                int rangeStart = 31;
                int rangeEnd = rangeStart + cutsceneDialogues.Length - 1;

                if (nextCutscene <= rangeEnd)
                {
                    StartCutscene(nextCutscene, cutsceneDialogues, GlobalCutsceneState.Dialogue2_Cutscene1);
                }
                else
                {

                    EndCutscene();
                }
            }
        }
    }
}
