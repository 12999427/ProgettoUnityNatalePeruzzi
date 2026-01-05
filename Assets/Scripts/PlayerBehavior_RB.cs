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

    protected AudioSource footstepAudioSource;
    [SerializeField] public AudioClip[] footstepClip;

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

        footstepAudioSource = GetComponent<AudioSource>();

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
        footstepAudioSource.PlayOneShot(footstepClip[Random.Range(0, footstepClip.Length)]);
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
    float height = 0f;  // altezza camera rispetto al player

    // 1. Aggiorna yaw e pitch dai controlli
    yaw += currentCameraInput.x * cameraSens.x;
    pitch -= currentCameraInput.y * cameraSens.y;
    pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

    // Trova la direzione (Quaternion) della camera usando yaw e pitch
    Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

    //Posiziona la camera rispetto al player (desiderata)
    Vector3 offset = rotation * new Vector3(0f, 0f, -radius); //Spostamento di raggio, nella corretta direzione
    Vector3 targetPosition = transform.position + Vector3.up * height + offset; //Spostamento di altezza + offset

    //Controllo collisioni (spring arm)
    Vector3 playerCenter = transform.position + Vector3.up * (height); // punto di partenza del ray
    Vector3 direction = targetPosition - playerCenter;
    float distance = direction.magnitude;

    if (Physics.Raycast(playerCenter, direction.normalized, out RaycastHit hit, distance))
    {
        // sposta la camera leggermente verso il giocatore rispetto al punto di contatto
        targetPosition = hit.point - direction.normalized * 0.1f; // 0.1f offset per evitare clipping
    }

    playerCamera.transform.position = targetPosition;

    //Rotazione
    playerCamera.transform.LookAt(transform.position + Vector3.up * height);


    camera3Drotation = playerCamera.transform.rotation;
}

    void HandleMovement()
    {
        int falling = getFalling();
        float speed = currentIsRunning ? runSpeed : walkSpeed;
        Vector3 MoveDir= playerCamera.transform.TransformDirection(new Vector3(currentInputVector.x, 0f, currentInputVector.y));
        MoveDir.y = 0f;
        MoveDir.Normalize();
        Vector3 MoveVector = MoveDir * speed;

        if (falling == 0) //a terra
        {
            rb.linearDamping = 1f;
            rb.linearVelocity = new Vector3 (MoveVector.x, rb.linearVelocity.y, MoveVector.z);
        }
        else
        {
            rb.linearDamping = 0f;

            if (currentIsMoving)
            {
                // raycast per controllare se c'è il muro nella direzione di movimento. se si applicasse
                // indipendentemente il movimento, il rigidbody potrebbe "attaccarsi" al muro
                
                float rayDistance = MoveVector.magnitude * Time.fixedDeltaTime + 0.03f; // un piccolo margine
                Vector3 rayOrigin = transform.position + MoveDir * capsuleCollider.radius;
                if (!Physics.Raycast(rayOrigin, MoveDir, rayDistance))
                {
                    rb.linearVelocity = new Vector3 (MoveVector.x, rb.linearVelocity.y, MoveVector.z);
                }
                else
                {
                    rb.linearVelocity = new Vector3 (MoveVector.x*-1, rb.linearVelocity.y, MoveVector.z*-1);
                }
            }
        }
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
        RaycastHit hitInfo;
        // 0.35f è più o meno il punto della semisfera del capsule collider in cui inizia a cadere
        bool hasGround = Physics.SphereCast(center, GetComponent<CapsuleCollider>().radius * 0.35f, Vector3.down, out hitInfo, rayLength, Physics.DefaultRaycastLayers);

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
