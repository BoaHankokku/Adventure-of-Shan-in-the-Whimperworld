using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickScript : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;

    public BoxCollider2D playerCollider;
    private Vector2 inputVector;
    public float handleLimit = 1.0f;

    public float movePower = 10f;
    public Rigidbody2D playerRigidbody;
    public Animator playerAnimator;
    

    private void Start()
    {
        // Add listener to the jump button


        // Ensure the necessary components are assigned
        if (joystickBackground == null || joystickHandle == null)
        {
            Debug.LogError("Joystick components not assigned.");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / joystickBackground.sizeDelta.x) * 2;
            inputVector = new Vector2(pos.x, 0);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            joystickHandle.anchoredPosition = new Vector2(inputVector.x * (joystickBackground.sizeDelta.x / 2) * handleLimit, 0);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;
        playerAnimator.SetBool("isRun", false);
    }

    public float Horizontal() { return inputVector.x; }

    // Control the player movement (left-right)
    private void Update()
    {
        // Continuously check if the player is grounded

        // Move the player based on joystick input
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 moveVelocity = Vector3.zero;

        if (Horizontal() < 0)
        {
            moveVelocity = Vector3.left;
            playerRigidbody.transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f);
            playerAnimator.SetBool("isRun", true);
        }
        else if (Horizontal() > 0)
        {
            moveVelocity = Vector3.right;
            playerRigidbody.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            playerAnimator.SetBool("isRun", true);
        }
        else
        {
            playerAnimator.SetBool("isRun", false);
        }

        playerRigidbody.transform.position += moveVelocity * movePower * Time.deltaTime;
    }

}