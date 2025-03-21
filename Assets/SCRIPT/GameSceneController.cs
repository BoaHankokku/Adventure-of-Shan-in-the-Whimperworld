using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Cutscene1Dialogue : BaseCutsceneManager
{
    private string[] cutsceneDialogues = new string[]
    {
        "Shan is a very imaginative child...\nShe likes to dream about things...",
        "One day she woke up and her mom was lying next to her...",
        "While mom was getting breakfast ready, Shan was talking about the dream she had last night.",
        "Mom, I dreamt about my dreamland again.",
        "What was that dreamland about?",
        "My dreamland, Whimperworld, is full of fantasy where houses look like 'hassle and grettle.",
        "Before that, eat this delicious food I prepared...",
        "A veggieesss? Oh noooo, I don't want that.",
        "You need to eat your veggies to be strong and healthy.",
        "*THROWS AWAY THE FOOD* NO! I don't want...",
        "It was getting late at night, and Shan was ready to go to sleep..",
        "Her mother tucked her into the bed, and she went to sleep quickly because she knew another fantastic dream would come to her, only for a nightmare to follow.",
        "What is this place? This isn't the dream I always imagined, and this house doesn't look like the Hansel and Gretel house I wanted. It feels more like the haunted houses I see in movies.",
        "After Shan entered the haunted house, a violet mist swirled around her, shimmering with magical energy. From within the mist emerged a girl with vibrant violet hair, her presence radiating an aura of mystical power.",
        "This was Ynah, a girl who possessed a strong angelic power. Her lilac eyes glowed softly as she stepped forward to greet Shan",
        "Who are you?...",
        "I am Ynah, guardian of the Whimperworld. You have entered a realm where dreams and nightmares intertwine.",
        "You are in a lucid dream, Shan, where you have control and awareness. I sense a great destiny awaits you here.",
        "A lucid dream? Control and awareness? I don't understand... I thought this was just a haunted house. What's happening to me?",
        "I understand your confusion, Shan. I have seen all that you have done.",
        "This is your trial, where you must face the monsters within to progress through each stage and ultimately find your escape.",
        "Ynah, understanding Shan's confusion and sensing her fear, closed her eyes and began to pray softly. A gentle glow surrounded Shan...",
        "...and she felt a warm, comforting sensation wash over her. Suddenly, a surge of magical energy enveloped Shan, transforming her into her superhero form...",
        "Her clothes shimmered with newfound power, and she felt a surge of courage and strength coursing through her.",
        "What is this?...",
        "That is the Blue Scepter. It can cast different types of magic to help you on your journey. Use it wisely...",
        "for it will aid you in overcoming the challenges and monsters you will face in this realm. Come, there is something you need to see.",
        "Where are we?",
        "*GROMP*",
        "Ynah's form wavered and transformed into a swirling purple mist, hovering protectively around Shan.\r\n\r\n\"Quickly, Shan,\" her voice echoed softly from within the mist.\r\n",
    };

    public override void StartCutscene(int cutsceneIndex, string[] cutsceneDialogues, GlobalCutsceneState startRange)
    {
        if (cutsceneIndex < 1 || cutsceneIndex > cutsceneDialogues.Length)
        {
            Debug.LogWarning("Invalid cutscene index.");
            return;
        }

        base.StartCutscene(cutsceneIndex, cutsceneDialogues, startRange);
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
                StartCutscene(savedState, cutsceneDialogues, GlobalCutsceneState.Dialogue1_Cutscene1);
                return;
            }
        }

        StartCutscene(1, cutsceneDialogues, GlobalCutsceneState.Dialogue1_Cutscene1);

        // Save the initial cutscene progress
        SaveManager.Instance.SaveGame(null, (int)GlobalCutsceneState.Dialogue1_Cutscene1);
    }



    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // On mouse click or tap
        {
            if (isTyping)
            {
                isFastTyping = true; // Speed up typewriter
            }
            else
            {
                int nextCutscene = (int)currentState + 1;

                // Ensure we don't exceed the dialogue array length
                if (nextCutscene <= cutsceneDialogues.Length)
                {
                    StartCutscene(nextCutscene, cutsceneDialogues, GlobalCutsceneState.Dialogue1_Cutscene1);
                }
                else
                {
                    EndCutscene(); // Transition to the next scene
                }
            }
        }
    }
}
