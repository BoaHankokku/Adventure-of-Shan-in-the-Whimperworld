using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Cutscene3Dialogue : BaseCutsceneManager
{
    private string[] cutsceneDialogues = new string[]
    {
        "Another day brought another lesson for Shan in her dreams as she whispered to herself, \"Ynah is right! I should wash my teeth every day, and I don't want my teeth to become like those scary tooth army.\"",
        "Just then, her mother walked into her room and saw that she was already awake. \"Oh, my sweetheart, are you ready for school? It's already Monday!\" Shan nodded with excitement, knowing she would see her friends again.",
        "While in school, during a lesson, Shan noticed something wrong with her classmate sitting in front of her. She saw the classmate take the pencil from their seatmate and start bullying them.",
        "During a lesson, Shan noticed her classmate sitting in front of her looking unusually sad. She saw that their seatmate had taken their pencil, teasing them quietly while they sat in silence, staring down at their desk.",
        "Eventually, the teacher noticed something was amiss with the two classmates. \"What's wrong with you two?\" the teacher asked.",
        "\"She took my pencil,\" the bully claimed. \"No, teacher, he's the one who took it,\" the poor girl explained. Shan knew everything...",
        "...that had happened but was too scared to speak up for the poor girl, so she remained silent. \"I need to speak with the parents of both of you,\" the teacher decided.",
        "Finally, the class was dismissed, and Shan returned home. While doing her homework, she unconsciously fell asleep, only to be plagued by another nightmare"
    };

    public override void StartCutscene(int cutsceneIndex, string[] dialogues, GlobalCutsceneState startRange)
    {
        int rangeStart = 40;
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
            if (savedState > 0)
            {
                Debug.Log($"Loaded saved cutscene progress: {savedState}");
                StartCutscene(savedState, cutsceneDialogues, GlobalCutsceneState.Dialogue3_Cutscene3);
                return;
            }
        }

        StartCutscene(40, cutsceneDialogues, GlobalCutsceneState.Dialogue3_Cutscene3);

        // Save the initial cutscene progress
        SaveManager.Instance.SaveGame(null, (int)GlobalCutsceneState.Dialogue3_Cutscene3);
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
                int rangeStart = 40;
                int rangeEnd = rangeStart + cutsceneDialogues.Length - 1;

                if (nextCutscene <= rangeEnd)
                {
                    StartCutscene(nextCutscene, cutsceneDialogues, GlobalCutsceneState.Dialogue3_Cutscene3);
                }
                else
                {
                    EndCutscene();
                }
            }
        }
    }
}
