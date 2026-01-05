using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerBehavior_RB : MonoBehaviour
{
    public InputSystem_Actions playerInput;
    Rigidbody rb;
    Vector2 currentInputVector;
    Vector2 currentCameraInput;
    bool currentIsRunning = false;
    bool currentIsMoving = false;
    Animator animator;
    Camera playerCamera;
    CapsuleCollider capsuleCollider;
    Quaternion camera3Drotation;

    private float yaw;
    private float pitch;

    public Vector2 cameraSens = new Vector2(5f, 5f);
    public float pitchMin = -70f;
    public float pitchMax = 80f;

    [SerializeField] public float walkSpeed = 0.1f;
    [SerializeField] public float runSpeed = 0.1f;


    void Awake()
    {
        playerInput = new();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, GetComponent<CapsuleCollider>().height / -2f * 0.99f, 0);

        animator = GetComponent<Animator>();
        playerCamera = gameObject.GetComponentInChildren<Camera>();

        capsuleCollider = GetComponent<CapsuleCollider>();

        playerInput.CharacterControls.Move.performed += MovementInputChanged;
        playerInput.CharacterControls.Move.started += MovementInputChanged;
        playerInput.CharacterControls.Move.canceled += MovementInputChanged;
        playerInput.CharacterControls.Run.started += RunInputChanged;
        playerInput.CharacterControls.Run.canceled += RunInputChanged;

        playerInput.CharacterControls.Look.started += CameraInputChanged;
        playerInput.CharacterControls.Look.performed += CameraInputChanged;
        playerInput.CharacterControls.Look.canceled += CameraInputChanged;
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
        //Debug.Log(context.ReadValue<Vector2>());
        currentInputVector = context.ReadValue<Vector2>();
        currentIsMoving = currentInputVector.magnitude > 0;
    }

    void CameraInputChanged(InputAction.CallbackContext context)
    {
        currentCameraInput = context.ReadValue<Vector2>();
    }

    void RunInputChanged(InputAction.CallbackContext context)
    {
        currentIsRunning = context.ReadValueAsButton();
    }

    public void OnFootstep()
    {
        // This function can be called from animation events
        Debug.Log("Footstep sound played");
        // Here you can add code to play footstep sounds
    }

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }
    void Update()
    {
        HandleAnimation();
        HandleCamera();
    }

void HandleCamera()
{
    float radius = 2f;    // distanza camera dal player
    float height = 0.8f;  // altezza camera rispetto al player

    // 1. Aggiorna yaw e pitch dai controlli
    yaw += currentCameraInput.x * cameraSens.x;
    pitch -= currentCameraInput.y * cameraSens.y;
    pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

    // Trova la direzione (Quaternion) della camera usando yaw e pitch
    Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

    //Posiziona la camera rispetto al player
    Vector3 offset = rotation * new Vector3(0f, 0f, -radius); //Spostamento di raggio, nella corretta direzione
    Vector3 targetPosition = transform.position + Vector3.up * height + offset; //Spostamento di altezza + offset
    playerCamera.transform.position = targetPosition;

    //Rotazione
    playerCamera.transform.LookAt(transform.position + Vector3.up * height);


    camera3Drotation = playerCamera.transform.rotation;
}

    void HandleMovement()
    {
        int falling = getFalling();

        if (falling == 0)
            rb.linearDamping = 1f;
        else
            rb.linearDamping = 0f;

        float speed = currentIsRunning ? runSpeed : walkSpeed;
        Vector3 MoveVector = playerCamera.transform.TransformDirection(new Vector3(currentInputVector.x, 0f, currentInputVector.y)) * speed;
        rb.linearVelocity = new Vector3 (MoveVector.x, rb.linearVelocity.y, MoveVector.z);

    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleAnimation()
    {
        animator.SetBool("isWalking", currentIsMoving);
        animator.SetBool("isRunning", currentIsMoving && currentIsRunning);
        animator.SetBool("isInAir", getFalling() != 0);
    }

    void HandleRotation()
    {
        if (!currentIsMoving)
            return;

        // Direzione di movimento camera-relative (stessa usata per muoversi)
        // Vettore che rappresenta la direzione di movimento del giocatore rispetto alla rotazione della telecamera
        Vector3 moveDir = playerCamera.transform.TransformDirection(
            new Vector3(currentInputVector.x, 0f, currentInputVector.y)
        );

        //Qusto perchè la telecamera ha una rotazione y (pitch) che non vogliamo influenzi la direzione di movimento orizzontale del player
        // Il giocatore non deve inclinarsi in base alla rotazione della camera
        moveDir.y = 0f;

        // Errori di calcolo se magnitude = 0
        if (moveDir.magnitude < 0.0001f)
            return;

        // Rotazione verso la direzione di movimento
        Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            0.2f
        );
    }

    int getFalling()
    {
        Vector3 center = transform.position + capsuleCollider.center;
        float rayLength = capsuleCollider.height / 2f + 0.1f;

        //Ray verso il basso
        bool hasGround = Physics.Raycast(center, Vector3.down, rayLength, Physics.DefaultRaycastLayers);

        if (hasGround)
            return 0; // Non cade

        if (rb.linearVelocity.y < -0.1f)
            return -2; // Sta cadendo
        else if (rb.linearVelocity.y > 0.1f)
            return 1; // Sta salendo
        else
            return -1; // In aria ma non cade, Y costante
    }
}
