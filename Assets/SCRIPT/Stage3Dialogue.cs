using ClearSky;
using TMPro;
using UnityEngine;
using System.Collections;
using ClearSky.Controller;
using ClearSky.Enemy;

public class Stage3Dialogue : MonoBehaviour
{
    [Header("Dialogue Elements")]
    public TextMeshProUGUI dialogueText, characterNameText;
    public GameObject dialoguePanel;
    public GameObject ynahIcon, shanIcon, heartbossIcon, unknownGirlIcon, trixiIcon, hform;
    public GameObject unknownGirlObject;

    [Header("Rform and Rtrueform")]
    public Boss2 RformEnemy;
    public Boss2 RtrueFormEnemy;
    public GameObject RformStatsBar;
    public GameObject RtrueFormStatsBar;

    [Header("Dialogue Process")]
    private bool isTyping = false;
    private bool fastTyping = false;
    private Coroutine typingCoroutine;
    private string fullText;
    public float typingSpeed = 0.05f;
    public float fastTypingSpeed = 0.01f;

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
    private void Update()
    {
        if (!dialogueInProgress) return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse button clicked during dialogue.");
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

    private void EnableCharacterIcon(GameObject icon)
    {
        ynahIcon.SetActive(false);
        shanIcon.SetActive(false);
        heartbossIcon.SetActive(false);
        unknownGirlIcon.SetActive(false);
        trixiIcon.SetActive(false);
        hform.SetActive(false);

        if (icon != null) icon.SetActive(true);
    }

    private void EndDialogue()
    {
        Debug.Log("[EndDialogue] Dialogue ended.");
        dialoguePanel.SetActive(false);
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
        string dialogueKey = "Stage3_Initial";

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
    private IEnumerator InitialDialogue(string dialogueKey)
    {
        dialogueInProgress = true;

        StartDialogue("???", "Help me...", null);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "Who are you? What's your name?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "Why aren’t you answering? What is happening?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Wait a second... this looks like my school... but abandoned.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Ynah! Ynah! Where are you?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Ynah", "Shan, you need to be brave. This stage is different.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Ynah", "You must save the girl. There are fewer enemies, but the monster guarding her is very powerful.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);


        StartDialogue("Trixi", "Shan, this is my final appearance. I wanted to see you one last time.", trixiIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Trixi, why are you leaving? I’ll miss you!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Trixi", "Don’t be sad, Shan. I’ve given you all I can. This last skill is my gift to you.", trixiIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Trixi", "This new skill is called ‘Dark Devastation.’ It deals massive damage, but it costs a lot of mana and has a long cooldown. Use it wisely.", trixiIcon);
        playerController.UnlockSkill(4);
        GameDataManager.Instance.UnlockSkill(4);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Thank you, Trixi. I’ll use it carefully.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Trixi", "Good luck, Shan. Be careful—the enemies ahead are few, but they hit hard.", trixiIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        EndDialogue();
        dialogueInProgress = false;
        dialoguePanel.SetActive(false);
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();

        GameManager3.Instance?.ProceedAfterInitialDialogue();
    }

    public void StartHeartboss()
    {
        string dialogueKey = "Stage3_Heart";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

        StartCoroutine(StartHeartbossEncounter(dialogueKey));
    }

    private IEnumerator StartHeartbossEncounter(string dialogueKey)
    {
        dialogueInProgress = true;
        StartDialogue("Shan", "Who are you? And where is the girl?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Heart", "The girl? I don’t know any girl! I am the guardian of this place.", heartbossIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "I need to save her. She needs me!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Heart", "Then you’ll have to go through me! Prepare yourself!", heartbossIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        EndDialogue();
        dialogueInProgress = false;
        dialoguePanel.SetActive(false);
        if (RformStatsBar != null) RformStatsBar.SetActive(true);
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();

        if (RformEnemy != null) RformEnemy.StartBossEncounter();

    }

    public void StartRform()
    {
        string dialogueKey = "Stage3_Hform";

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
        // Initial dialogue with the girl
        StartDialogue("Girl", "Please... help me...", unknownGirlIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Wait... I know you. You’re my classmate... the one who’s always bullied!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Don’t worry. I’ll save you.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        // Destroy the girl's object after the dialogue
        if (unknownGirlObject != null)
        {
            Destroy(unknownGirlObject);
            Debug.Log($"Unknown girl game object ({unknownGirlObject.name}) has been destroyed.");
        }

        // Dialogue introducing the boss's true form
        StartDialogue("Devil", "You dare to challenge me? Witness my true power!", hform);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "I won’t back down! I’ll defeat you and save her!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        EndDialogue();
        dialogueInProgress = false;
        dialoguePanel.SetActive(false);

        // Activate the true form's stats bar and begin the encounter
        if (RtrueFormStatsBar != null) RtrueFormStatsBar.SetActive(true);
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();

        if (RtrueFormEnemy != null) RtrueFormEnemy.StartBossEncounter();
    }

    public void LevelCompletion()
    {
        string dialogueKey = "Stage3_Final";

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
        StartDialogue("Ynah", "Shan, you’ve done well. Remember, standing up for others makes you stronger.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "I’ll always protect those who need me. I’ll never let anyone feel abandoned again.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Ynah", "You’re learning, Shan. Keep going, and you’ll overcome anything.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        EndDialogue();
        dialogueInProgress = false;
        dialoguePanel.SetActive(false);
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();

    }
}
