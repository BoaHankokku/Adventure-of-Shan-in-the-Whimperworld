using ClearSky;
using TMPro;
using UnityEngine;
using System.Collections;
using ClearSky.Controller;
using Unity.VisualScripting;
using UnityEngine.Device;
using ClearSky.Enemy;
using UnityEngine.SceneManagement;

public class Stage1Dialogue : MonoBehaviour
{
    [Header("Dialogue Elements")]
    public TextMeshProUGUI dialogueText, characterNameText;
    public GameObject dialoguePanel;
    public GameObject ynahIcon, shanIcon, trixiIcon, tfIcon;
    public GameObject trixiObject;
    public GameObject ynahObject;

    [Header("Dynamo")]
    public DynamoEnemy DynamoEnemy;
    public GameObject dynamoStatsBar;
    public GameObject obtainedMagicScrollPanel;
    [Header("Dynamo")]
    public TrueFormEnemy TrueFormEnemy;
    public GameObject TFormStatsBar;
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
        if (GameDataManager.Instance.IsTrixiDestroyed() && trixiObject != null)
        {
            Destroy(trixiObject);
            Debug.Log("Trixi object destroyed on load.");
        }
        dialoguePanel.SetActive(false); // Ensure dialogue panel is hidden at the start
        dialogueText.text = "";
        characterNameText.text = "";

        playerController = FindObjectOfType<SimplePlayerController>();
        StartInitialDialogue();
    }

    private void StartDialogue(string characterName, string dialogueLine, GameObject characterIcon)
    {
        if (dialogueInProgress && isTyping)
        {
            Debug.LogWarning("Dialogue is already in progress. Ignoring StartDialogue call.");
            return;
        }

        fullText = dialogueLine;
        characterNameText.text = characterName;

        EnableCharacterIcon(characterIcon);

        // Stop any ongoing typing coroutine
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(fullText));

        // Show the dialogue panel and disable player input
        dialoguePanel.SetActive(true);
        playerController?.DisablePlayerInput();

        dialogueInProgress = true;

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

            // Break the loop if fastTyping is enabled and the coroutine is interrupted
            if (fastTyping && i < text.Length - 1)
            {
                dialogueText.text = text; // Display the full text immediately
                break;
            }
        }

        isTyping = false; // Typing is complete
    }

    private void Update()
    {
        if (!dialogueInProgress) return;

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
        trixiIcon.SetActive(false);
        tfIcon.SetActive(false);

        if (icon != null) icon.SetActive(true);
    }

    private void EndDialogue()
    {
        Debug.Log("[EndDialogue] Dialogue ended.");
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // Stop any ongoing typing coroutine
            typingCoroutine = null;
        }
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        characterNameText.text = "";
        isTyping = false;
        playerController?.EnablePlayerInput();

        // Unfreeze player position
        var playerRigidbody = playerController.GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
        {
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation; // Restore default constraints
            Debug.Log("Player position unfrozen after dialogue.");
        }

        dialogueInProgress = false; // Reset the dialogue progress state
    }

    public void StartInitialDialogue()
    {
        string dialogueKey = "Stage1_Initial";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }
        StartDialogue("Ynah", "Take your Blue Scepter and use the default attack to clear obstacles on your way. Be careful of their attacks.", ynahIcon);

        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();
    }

    public void StartCompletionDialogue()
    {
        string dialogueKey = "Stage1_Completion";
        Debug.Log($"Checking if dialogue '{dialogueKey}' is completed: {GameDataManager.Instance.IsDialogueCompleted(dialogueKey)}");
        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

        StartCoroutine(PlayCompletionDialogue(dialogueKey));
    }

    private IEnumerator PlayCompletionDialogue(string dialogueKey)
    {
    
        StartDialogue("Ynah", "Well done! You have mastered the default attack. Now, find the magic scroll located in this haunted house. You can also use some items granted by the rewards.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Ynah", "I will see you again at the end of this stage. Good luck on your journey, Shan. Remember, you have the power within you to overcome any obstacle.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "Did that really just happen? I... I need to find that magic scroll. Ynah said it would guide me. I can do this.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();
  
    }
    private bool shanTrixDialogueStarted = false;
    public void StartShanTrixiEncounterDialogue(GameObject trixiObject, GameObject zoneObject)
    {
        if (shanTrixDialogueStarted) return; // Prevent repeated triggers
        shanTrixDialogueStarted = true;

        string dialogueKey = "Stage1_ShanTrix";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

      
        StartCoroutine(ShanTrixiDialogueSequence(trixiObject, zoneObject, dialogueKey));
    }

    private IEnumerator ShanTrixiDialogueSequence(GameObject trixiObject, GameObject zoneObject, string dialogueKey)
    {

        StartDialogue("Shan", "Who are you?...", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
  

        StartDialogue("Trixi", "I'm Trixi, the Merchant of Wonders. I heard from Ynah that you will face the monster guarding this haunted house.", trixiIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
       

        StartDialogue("Trixi", "Ynah sent me here to help you defeat that donut dynamo and other future enemies.", trixiIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
     

        // Unlock Ice Frost skill
        StartDialogue("Trixi", "As a gift, I will teach you a new skill: Ice Frost. It summons sharp ice spikes to deal powerful damage to your enemies!", trixiIcon);
        playerController.UnlockSkill(2);
        GameDataManager.Instance.UnlockSkill(2);
        GameDataManager.Instance.SaveGame();
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Thank you, Trixi. This will be a great help!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
     
        if (trixiObject != null)
        {
            Destroy(trixiObject);
            GameDataManager.Instance.SetTrixiDestroyed(true);
        }
        

        if (zoneObject != null)
        {
            Debug.Log($"Destroying Zone Object: {zoneObject.name}");
            Destroy(zoneObject);
        }
        else
        {
            Debug.LogWarning("Zone Object is null, cannot destroy.");
        }
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();

    }
    private bool bossDialogueStarted = false;
    public void StartBossDialogue()
    {
        if (bossDialogueStarted) return; // Prevent repeated triggers
        bossDialogueStarted = true;

        string dialogueKey = "Stage1_lvl5Boss";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

        dialogueInProgress = true;
        StartCoroutine(ShanBossEncounterCompletionDialogue(dialogueKey));
    }

    private IEnumerator ShanBossEncounterCompletionDialogue(string dialogueKey)
    {
        StartDialogue("Shan", "Who are you?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Boss", "I see what you've been doing to your health! Enjoying your sweets, are you?.", tfIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Boss", "Be careful, Shan, too much sugar could be your downfall, leading to more than just a bellyache—think long-term, think suffering.", tfIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        if (TFormStatsBar != null)
        {
            TFormStatsBar.SetActive(true);
        }
        if (TrueFormEnemy != null)
        {
            TrueFormEnemy.StartTrueFormEncounter();
        }

        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();
        dialogueInProgress = false;
    }
    private bool donutDynamoDialogueStarted = false;
    public void StartDonutDynamoEncounter()
    {
        if (donutDynamoDialogueStarted) return; // Prevent repeated triggers
        donutDynamoDialogueStarted = true;

        string dialogueKey = "Stage1_Donut";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }

        dialogueInProgress = true;
        StartCoroutine(DonutDynamoDialogueSequence(dialogueKey));
    }

    private IEnumerator DonutDynamoDialogueSequence(string dialogueKey)
    {
        StartDialogue("Shan", "You must be Donut Dynamo that the merchant Trixi mentioned.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "Why aren't you talking? I bet you have the magic scroll.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        EndDialogue();
        if (dynamoStatsBar != null)
        {
            dynamoStatsBar.SetActive(true);
        }
        if (DynamoEnemy != null)
        {
            DynamoEnemy.StartDonutDynamoEncounter();
        }

        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();
        dialogueInProgress = false;
    }
    private bool afterDonutDialogueStarted = false;
    public void AftertDonutDynamoEncounter()
    {
        if (afterDonutDialogueStarted) return; // Prevent repeated triggers
        afterDonutDialogueStarted = true;

        string dialogueKey = "Stage1_afterDonut";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }
        dialogueInProgress = true;
        StartCoroutine(DefeatDynamoDialogueSequence(dialogueKey));
    }

    private IEnumerator DefeatDynamoDialogueSequence(string dialogueKey)
    {
        obtainedMagicScrollPanel.SetActive(true);
        StartDialogue("Shan", "Now that I have the magic scroll, I'm closer to reaching Ynah.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        StartDialogue("Shan", "I need to give it back to her.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);
        EndDialogue();
        obtainedMagicScrollPanel.SetActive(false);
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();
        dialogueInProgress = false;
    }
    public void AfterTFormEncounter()
    {
        string dialogueKey = "Stage1_ynah";

        if (GameDataManager.Instance.IsDialogueCompleted(dialogueKey))
        {
            Debug.Log("Completion dialogue already completed. Skipping...");
            return;
        }
        dialogueInProgress = true;
        StartCoroutine(DefeatTFormDialogueSequence(dialogueKey));
    }

    private IEnumerator DefeatTFormDialogueSequence(string dialogueKey)
    {
        StartDialogue("Ynah", "Great job, Shan! You've successfully cleared this stage.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Ynah", "By the way, do you have the magic scroll you were tasked to find?", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Yes, I have it right here! But can you finally tell me why I’m here?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Ynah", "You’re here to face this challenge because of something I’ve noticed about your habits in the real world.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "My habits? What do you mean?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Ynah", "Let me ask you this—you're not a fan of eating vegetables, are you?", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Ugh, no! They taste terrible. Is that why I’m here?", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Ynah", "Exactly! This is your challenge. If you keep avoiding healthy food and only eat junk and sweets...", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Ynah", "...you’ll end up unhealthy. The monster you fought today symbolizes the harm those bad habits can bring.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Wow, I didn’t think of it that way... I guess I need to start eating better.", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Ynah", "That’s the spirit! Now wake up and check what your mother prepared for you for breakfast.", ynahIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        StartDialogue("Shan", "Yesss! I'm going to eat healthy foods starting today!", shanIcon);
        yield return new WaitUntil(() => !dialoguePanel.activeSelf);

        EndDialogue();
        GameDataManager.Instance.SetDialogueCompleted(dialogueKey, true);
        GameDataManager.Instance.SaveDialogueFlags();
       
        GameDataManager.Instance.SaveProgress(
      stage: 1, // Current stage
      position: playerController.transform.position,
      health: playerController.playerStats.health,
      mana: playerController.playerStats.mana,
      level: 1, // Flag stage 1 completed
      artifacts: 0, // No artifacts here
      enemies: 0 // All enemies cleared
  );
        if (ynahObject != null) Destroy(ynahObject);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Cutscene2");

    }



}
