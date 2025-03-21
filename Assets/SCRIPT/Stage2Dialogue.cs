using ClearSky;
using TMPro;
using UnityEngine;
using System.Collections;
using ClearSky.Controller;
using Unity.VisualScripting;
using UnityEngine.Device;
using ClearSky.Enemy;
using UnityEngine.SceneManagement;
using System;
using UnityEditor.Experimental;

public class Stage2Dialogue : MonoBehaviour
{
    [Header("Dialogue Elements")]
    public TextMeshProUGUI dialogueText, characterNameText;
    public GameObject dialoguePanel;
    public GameObject ynahIcon, shanIcon, rformIcon, Razor, trixiicon;
    [Header("Rform and Rtrueform")]
    public Boss2 DynamoEnemy;
    public Boss2 DynamoEnemy2;
    public GameObject RformStatsBar;
    public GameObject RtrueFormStatsBar;

    [Header("Dialogue Process")]
    private bool isTyping = false;
    private bool fastTyping = false;
    private Coroutine typingCoroutine;
    private string fullText;
    public float typingSpeed = 0.05f;
    public float fastTypingSpeed = 0.01f;
    public delegate void DialogueCompleteHandler();
    public event DialogueCompleteHandler OnInitialDialogueComplete;
    private SimplePlayerController playerController;
    private bool dialogueInProgress = false;
    private void Start()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.CurrentSceneType = GameDataManager.SceneType.Gameplay;
            Debug.Log("SceneType set to Gameplay.");
        }
        playerController = FindObjectOfType<SimplePlayerController>();
        StartInitialDialogue();
    }

    private void StartDialogue(string characterName, string dialogueLine, GameObject characterIcon)
    {
        fullText = dialogueLine;
        characterNameText.text = characterName;

        EnableCharacterIcon(characterIcon);

        // Stop any ongoing typing coroutine
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(fullText));

        // Show the dialogue panel and disable player input
        dialoguePanel.SetActive(true);
        playerController?.DisablePlayerInput();

        // Freeze player position
        if (playerController != null)
        {
            var playerRigidbody = playerController.GetComponent<Rigidbody2D>();
            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector2.zero; // Stop all movement
                playerRigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation; // Freeze X position and rotation
                Debug.Log("Player position frozen during dialogue.");
            }
            else
            {
                Debug.LogWarning("Rigidbody2D not found on playerController.");
            }

            // Set player's animator to idle state
            var playerAnimator = playerController.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("isRun", false); // Ensure running animation stops
                playerAnimator.SetBool("isJump", false); // Ensure jumping animation stops
                Debug.Log("Player animator set to idle state during dialogue.");
            }
            else
            {
                Debug.LogWarning("Player Animator not found on playerController.");
            }
        }
    }


    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        fastTyping = false;
        dialogueText.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            yield return new WaitForSeconds(fastTyping ? fastTypingSpeed : typingSpeed);
        }
        isTyping = false;
    }

    private void Update()
    {
        if (!dialogueInProgress) return;
        if (Input.GetKeyDown(KeyCode.S)) // Replace with your desired key
        {
            if (isTyping)
            {
                fastTyping = true;
            }
            else if (dialoguePanel.activeSelf)
            {
                EndDialogue();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                fastTyping = true;
            }
            else if (dialoguePanel.activeSelf)
            {
                EndDialogue();
            }
        }
    }

    private void EnableCharacterIcon(GameObject icon)
    {
        ynahIcon.SetActive(false);
        shanIcon.SetActive(false);
        Razor.SetActive(false);
        rformIcon.SetActive(false);
        trixiicon.SetActive(false);

        if (icon != null) icon.SetActive(true);
    }

    private void EndDialogue()
    {
        Debug.Log("[EndDialogue] Dialogue ended.");
        dialoguePanel.SetActive(false); // Hide dialogue panel
        dialogueText.text = ""; // Clear text
        characterNameText.text = ""; // Clear character name
        isTyping = false; // Reset typing state
        playerController?.EnablePlayerInput();

        // Unfreeze player position
        var playerRigidbody = playerController.GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
        {
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation; // Restore default constraints
            Debug.Log("Player position unfrozen after dialogue.");
        }
    }


    public void StartInitialDialogue()
    {
        string dialogueKey = "Stage2_Initial";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

        Debug.Log("StartInitialDialogue called.");
    if (dialogueInProgress)
    {
        Debug.LogWarning("Dialogue already in progress. Ignoring StartInitialDialogue call.");
        return;
    }
        StartCoroutine(InitialDialogue(dialogueKey));
    }
    public IEnumerator InitialDialogue(string dialogueKey)
    {
        dialogueInProgress = true;

        StartDialogue("Shan", "Huh... another nightmare? I thought I was done with this.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Ynah", "Shan, this is the Whimperworld. A reflection of your actions... or inactions.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Ynah", "You ignored your mother's advice. Remember when she told you to brush your teeth? That chocolate feast has consequences.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Shan", "I didn’t think it mattered. But... is this why I'm here?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Ynah", "Every small habit shapes your future. To escape, you must face the monsters here and find five artifacts hidden in the dungeon.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Ynah", "Each artifact holds a lesson. Defeat the dungeon's guardian to move forward. Prove you're ready to change, Shan.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Shan", "So, this is all my fault... Fine. I’ll do what it takes to fix this.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Ynah", "You can do it, Shan. I’ll see you at the end of this stage.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        // Trixi's Dialogue Sequence
        StartDialogue("Trixi", "Hi, Shan! It's me, Trixi. Remember me?", trixiicon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Shan", "Trixi! It’s you! I’m so happy to see you here!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Trixi", "I’ve been watching over you. And guess what? I have a surprise for you!", trixiicon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Trixi", "Here’s a new skill: Black Magic - Poison! This lets you cast a spell that harms enemies with a special poison attack. It’s super strong!", trixiicon);
        playerController.UnlockSkill(3); // Unlock the 3rd skill
        GameDataManager.Instance.UnlockSkill(3);
        GameDataManager.Instance.SaveGame();
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        StartDialogue("Shan", "Wow, thank you so much, Trixi! This will really help!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        yield return new WaitForSeconds(0.2f);

        EndDialogue();
        dialogueInProgress = false;
        dialoguePanel.SetActive(false);

        GameManager2.Instance?.ProceedAfterInitialDialogue();
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();
        Debug.Log("Saved dialogue flags after setting completion.");

    }


    public void StartRazormouth()
    {
        string dialogueKey = "Stage2_Razor";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

        StartCoroutine(StartRazormouthEncounter(dialogueKey));
    }
    private IEnumerator StartRazormouthEncounter(string dialogueKey)
    {
        dialogueInProgress = true;
        StartDialogue("Shan", "Now who are you?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Razor", "I am the terror of your teeth, Shan. You haven't been brushing them, have you?", Razor);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf); 
        StartDialogue("Shan", "I take care of my teeth! What do you know about it?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Razor", "You don't know the horrors of plaque and decay! If you don’t start brushing properly, I'll come back to claim you!", Razor);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "I don't need your scary stories, Razormouth! I'll take better care of my teeth from now on!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Razor", "You better, or you’ll see me again when your breath stinks and your teeth are nothing but rotten bones!", Razor);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "You won’t scare me! I’ll brush, floss, and mouthwash every day!", shanIcon);
        EndDialogue();
        dialogueInProgress = false;
        dialoguePanel.SetActive(false);
        if (RformStatsBar != null)
        {
            RformStatsBar.SetActive(true); // Enable stats bar
        }

        if (DynamoEnemy != null)
        {
            DynamoEnemy.StartBossEncounter(); // Begin Rform behavior
        }
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();

    }
    public void StartRform()
    {
        string dialogueKey = "Stage2_RForm";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

        StartCoroutine(StartRformEncounter(dialogueKey));
    }
    private IEnumerator StartRformEncounter(string dialogueKey)
    {
        dialogueInProgress = true;
        StartDialogue("Shan", "You look so scary!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Ynah", "Don't be scared, Shan. You are much stronger than him. Face him with courage!", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "Ynah? Ynah... where are you?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Razormouth", "HAHAHAHA! You’ll never meet Ynah... face me, Shan. It’s just you and me now!", rformIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "No more running. I’ll face you and take you down, Razormouth. It’s time to fight!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        EndDialogue();
        dialogueInProgress = false;
        dialoguePanel.SetActive(false);
        if (RtrueFormStatsBar != null)
        {
            RtrueFormStatsBar.SetActive(true); // Enable stats bar
        }

        if (DynamoEnemy2 != null)
        {
            DynamoEnemy2.StartBossEncounter(); // Begin Rform behavior
        }
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();

    }
    public void LevelCompletion()
    {
        string dialogueKey = "Stage2_Final";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

        StartCoroutine(FinalDialogue(dialogueKey));
    }
    private IEnumerator FinalDialogue(string dialogueKey)
    {
        dialogueInProgress = true;
        StartDialogue("Shan", "Ugh... these decaying teeth... they're so gross and scary!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Ynah", "Shan, remember what I told you... you must brush your teeth and listen to your mother. This could have been avoided.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "You're right, Ynah... I should have listened... and I will from now on. I’ll take better care of my teeth.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Ynah", "It’s never too late to start, Shan. You’re stronger than you think, and your health matters.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "I’ll do it! I’ll brush my teeth every day and listen to my mom. No more fear from decaying teeth!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        EndDialogue();
        dialogueInProgress = false;
        dialoguePanel.SetActive(false);
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
            GameDataManager.Instance.SaveDialogueFlags();

            GameDataManager.Instance.SaveProgress(
                stage: 2,
                position: playerController?.transform.position ?? Vector3.zero,  // ✅ Null safety
                health: playerController?.playerStats?.health ?? 100,            // ✅ Default value
                mana: playerController?.playerStats?.mana ?? 100,                // ✅ Default value
                level: 2,
                artifacts: BaseGameManager.Instance?.ArtifactsClaimed ?? 0,      // ✅ Null check
                enemies: GameManager2.Instance?.enemiesDefeated ?? 0             // ✅ Null check
            );

            Debug.Log("[GameManager] Cleaning up GameDataManager before loading Cutscene2.");
            GameDataManager.CleanupInstance();
        }
        else
        {
            Debug.LogWarning("[FinalDialogue] GameDataManager.Instance is NULL. Skipping save process.");
        }

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Cutscene2");

    }
    public void ArtifactCompletion()
    {
        string dialogueKey = "Stage2_Artifact";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

        StartCoroutine(ArtifactComplete(dialogueKey));
    }
    private IEnumerator ArtifactComplete(string dialogueKey)
    {
        dialogueInProgress = true;
        StartDialogue("Shan", "I’ve done it! I’ve collected all the artifacts! I can’t wait to show Ynah... I’m ready to change and leave this nightmare behind!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        EndDialogue();
        dialogueInProgress = false;
        dialoguePanel.SetActive(false);
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();

        SceneManager.LoadScene("Endscene");
    }
}
