using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject wingsGO = null;
    [SerializeField] private Animator anim = null;
    [SerializeField] private GameObject poop = null;
    [SerializeField] private GameObject poop2 = null;

    Vector3 directionalInput;
    Vector3 targetDirection;

    Quaternion targetRotation;

    bool movementInput = false;

    float gravity = Physics.gravity.y * 2;

    float movementSpeed;
    float walkSpeed = 4f;
    float glideSpeed = 6.5f;
    float glideSteeringSensitivity = 0.2f;

    float terminalVelocity = 6f;
    float lift = 0;

    float initialJumpForce = 7;
    float additionalJumpForce = 35;
    float currentJumpForce;

    bool jumpQueued = false;
    bool jumpHold = false;

    bool isGrounded = false;
    bool isJumping = false;
    bool isGliding = false;

    float jumpTimer = 0;
    float timeBeforeJumpExtend = 0.075f;

    RaycastHit groundInfo;

    public LayerMask collisionMask;

    public CameraRig cameraRig;

    public Checkpoints checkpoints;
    int currentCheckpoint;

    Rigidbody rb;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private AudioSource windAudioSource = null;
    [SerializeField] private AudioClip runClip, jumpClip, glideClip;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        currentCheckpoint = 0;

        movementSpeed = walkSpeed;
        currentJumpForce = additionalJumpForce;
    }

    private void Update()
    {
        directionalInput.z = Input.GetAxisRaw("Vertical");
        directionalInput.x = Input.GetAxisRaw("Horizontal");

        movementInput = directionalInput.z != 0 || directionalInput.x != 0;

        if (directionalInput.magnitude > 1)
            directionalInput.Normalize();

        if (Input.GetKeyDown("space"))
            jumpQueued = true;

        jumpHold = Input.GetKey("space");
    }

    private void FixedUpdate()
    {
        GroundCheck();
        ApplyForces();
        CalculateMoveDirection();
        Move();
        Jump();
        Rotate();
        CheckpointCheck();
        WindAmbianceVolume();
    }

    void CheckpointCheck()
    {
        if (currentCheckpoint != checkpoints.checkpoints.Length - 1)
        {
            if (transform.position.z > checkpoints.checkpoints[currentCheckpoint + 1].position.z)
            {
                currentCheckpoint += 1;
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water Level"))
        {
            transform.position = checkpoints.checkpoints[currentCheckpoint].position;
            DisableGlide();
        }
        if (other.CompareTag("snakeArea"))
        {
            transform.position = checkpoints.checkpoints[currentCheckpoint].position;
            DisableGlide();
        }

        if (other.gameObject == poop)
        {
            poop2.SetActive(true);
        }
    }

    void GroundCheck()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out groundInfo, 0.25f, collisionMask))
        {
            isGrounded = true;

            if (transform.position.z > checkpoints.deathPoint.position.z && groundInfo.collider.CompareTag("Ground"))
                transform.position = checkpoints.checkpoints[currentCheckpoint].position;

            DisableGlide();
        }
        else
        {
            isGrounded = false;
        }
    }

    void ApplyForces()
    {
       rb.AddForce(0, gravity, 0, ForceMode.Acceleration);

        if (isGliding && rb.velocity.y < 0)
        {
            lift = -gravity - (terminalVelocity + rb.velocity.y);
            rb.AddForce(0, lift, 0, ForceMode.Acceleration);
        }
    }

    void CalculateMoveDirection()
    {
        if (!isGliding)
        {
            Vector3 cameraForward = cameraRig.transform.forward;
            Vector3 cameraRight = cameraRig.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            targetDirection = directionalInput.z * cameraForward + directionalInput.x * cameraRight;
        }
        else
        {
            targetDirection = 1 * transform.forward + directionalInput.x * glideSteeringSensitivity * transform.right;
        }

        if (groundInfo.normal != Vector3.zero)
            targetDirection = Vector3.Cross(Vector3.Cross(-targetDirection, Vector3.up), groundInfo.normal);
    }

    void Move()
    {
        var v= targetDirection.normalized * movementSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + v);

        if (movementInput && isGrounded){
            
            
            anim.SetBool("Running", true);
        }
        else
            anim.SetBool("Running", false);
    }

    void Jump()
    {
        if (!isJumping)     // if not performing a jump
        {
            if (jumpQueued)    // if space was pressed in the last Update() tick
            {
                if (isGrounded)    // jump
                {
                    rb.AddForce(0, initialJumpForce, 0, ForceMode.Impulse);
                    isJumping = true;

                    audioSource.volume = Random.Range(0.23f, 0.27f);
                    audioSource.pitch = Random.Range(0.75f, 1.25f);
                    audioSource.PlayOneShot(jumpClip);
                }
                else        // toggle glide
                {
                    ToggleGlide();
                }
                jumpQueued = false;
            }
        }
        else     // if a jump was executed
        {
            if (jumpHold)    // if  space is still being held
            {
                jumpTimer += Time.fixedDeltaTime;

                if (jumpTimer > timeBeforeJumpExtend)
                {
                    rb.AddForce(0, currentJumpForce, 0, ForceMode.Acceleration);
                    currentJumpForce = Mathf.Lerp(currentJumpForce, 0, 0.1f);
                }
            }
            else   // reset jump variables
            {
                jumpTimer = 0;
                isJumping = false;
                currentJumpForce = additionalJumpForce;
            }
        }
    }

    void ToggleGlide()
    {
        if (!isGliding)
            EnableGlide();
        else
            DisableGlide();
    }

    void EnableGlide()
    {
        isGliding = true;
        movementSpeed = glideSpeed;
        anim.SetBool("Flying", true);

        audioSource.clip = glideClip;
        audioSource.volume = .25f;
        audioSource.Play();

        //cameraRig.SetMode(CameraRig.Mode.Follow);
    }

    void DisableGlide()
    {
        isGliding = false;
        movementSpeed = walkSpeed;
        anim.SetBool("Flying", false);

        //audioSource.Stop();

        //cameraRig.SetMode(CameraRig.Mode.Free);
    }

    void Rotate()
    {
        float angle;
        float tilt;
        Vector3 planeAngle = new Vector3();
        if (!isGliding)
        {
            angle = Mathf.Atan2(directionalInput.x, directionalInput.z) * Mathf.Rad2Deg;
            angle += cameraRig.transform.rotation.eulerAngles.y;
            if(isGrounded){
                Vector3 norm = groundInfo.normal.normalized;
                Vector3 normalz = new Vector3(0, 1, norm.z);
                Vector3 normalx = new Vector3(norm.x, 1, 0);
                planeAngle.x = Mathf.Acos(Vector3.Dot(Vector3.up, normalz.normalized)) * Mathf.Rad2Deg;
                planeAngle.z = Mathf.Acos(Vector3.Dot(Vector3.up, normalx.normalized)) * Mathf.Rad2Deg;
                if (norm.z < 0 )
                    planeAngle *= -1;
                if (norm.x < 0&& norm.z>=0||norm.x>0&& norm.z<0)
                    planeAngle.z *= -1;
                planeAngle = Quaternion.Euler(0, angle, 0) * planeAngle;
            }
        if (movementInput)
           targetRotation = Quaternion.Euler(planeAngle.x, angle, -planeAngle.z);
        }
        else
        {
            angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            tilt = -directionalInput.x * 30;

            targetRotation = Quaternion.Euler(0, angle, tilt);
        }
       
        if (transform.rotation != targetRotation)
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.2f);
    }

    private void WindAmbianceVolume()
    {
        windAudioSource.volume = (transform.position.y / 100) * 0.2f;
    }

    public void RunSound()
    {
        audioSource.volume = Random.Range(0.14f, 0.18f);
        audioSource.pitch = Random.Range(0.80f, 1.20f);
        audioSource.PlayOneShot(runClip);
    }
}
