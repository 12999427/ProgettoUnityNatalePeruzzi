using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PlayerBehavior : MonoBehaviour
{
    InputSystem_Actions playerInput;
    CharacterController characterController;
    Vector2 currentInputVector;
    bool currentIsRunning = false;
    bool currentIsMoving = false;
    Animator animator;
    [SerializeField] public float moveSpeed = 2f;


    void Awake()
    {
        playerInput = new();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerInput.CharacterControls.Move.performed += MovementInputChanged;
        playerInput.CharacterControls.Move.started += MovementInputChanged;
        playerInput.CharacterControls.Move.canceled += MovementInputChanged;
        playerInput.CharacterControls.Run.started += RunInputChanged;
        playerInput.CharacterControls.Run.canceled += RunInputChanged;
    }
    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    void MovementInputChanged(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<Vector2>());
        currentInputVector = context.ReadValue<Vector2>();
        currentIsMoving = currentInputVector.magnitude > 0;
    }

    void RunInputChanged(InputAction.CallbackContext context)
    {
        currentIsRunning = context.ReadValueAsButton();
    }

    public void OnFootStep()
    {
        // This function can be called from animation events
        Debug.Log("Footstep sound played");
        // Here you can add code to play footstep sounds
    }

    void Update()
    {
        HandleRotation();
        HandleAnimation();
        HandleMovement();
    }

    void HandleMovement()
    {
        bool isGrounded = characterController.isGrounded;

        characterController.attachedRigidbody.useGravity = true;

        Vector3 move = new Vector3(currentInputVector.x, 0, currentInputVector.y);
        move = transform.TransformDirection(move);
        float speed = moveSpeed * (currentIsRunning ? 2f : 1f);
        characterController.Move(move * speed * Time.deltaTime);
    }

    void HandleAnimation()
    {
        animator.SetBool("isWalking", currentIsMoving);
        animator.SetBool("isRunning", currentIsMoving && currentIsRunning);
    }

    void HandleRotation()
    {
    }
}
