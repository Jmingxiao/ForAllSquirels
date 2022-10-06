using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float drag = 0.0f;
    Vector3 directionalInput;
    Vector3 targetDirection;

    Quaternion targetRotation;

    readonly float gravity = Physics.gravity.y;
    float movementSpeed = 4;
    bool grounded = false;


    bool jumping = false;

    RaycastHit groundInfo;

    CameraRig mainCam;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = GameObject.FindGameObjectWithTag("cameraRig").GetComponent<CameraRig>();
    }

    private void Update()
    {
        directionalInput.z = Input.GetAxis("Vertical");
        directionalInput.x = Input.GetAxis("Horizontal");

        if (directionalInput.magnitude > 1)
            directionalInput.Normalize();

        if (!jumping)
            jumping = Input.GetKeyDown("space");
    }

    private void FixedUpdate()
    {
        GroundCheck();
        rb.AddForce(0, gravity, 0, ForceMode.Acceleration);
        CalculateMoveDirection();
        Move();
        Rotate();
        jumping = false;
    }

    void GroundCheck()
    {
        int layerMask = 1 << 9;
        layerMask = ~layerMask;

        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, 0.5f, layerMask))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
    }

    void CalculateMoveDirection()
    {
        Vector3 cameraForward = mainCam.transform.forward;
        Vector3 cameraRight = mainCam.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        targetDirection = directionalInput.z * cameraForward + directionalInput.x * cameraRight;

        if (groundInfo.normal != Vector3.zero)
            targetDirection = Vector3.Cross(Vector3.Cross(-targetDirection, Vector3.up), groundInfo.normal);
    }

    void Move()
    {
        if (grounded && jumping)
        {
            rb.AddForce(0, 5, 0, ForceMode.Impulse);
        }
        movementSpeed = 3;
        if (Input.GetKey(KeyCode.LeftShift) && grounded)
        {
            movementSpeed = 5;
        }
        Gliding();
        rb.MovePosition(transform.position + targetDirection * movementSpeed * Time.deltaTime);
    }
    void Rotate()
    {
        float angle;
        angle = Mathf.Atan2(directionalInput.x, directionalInput.z) * Mathf.Rad2Deg;
        angle += mainCam.transform.rotation.eulerAngles.y;

        if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
            targetRotation = Quaternion.Euler(0, angle, 0);

        if (transform.rotation != targetRotation)
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.2f);
    }
    void Gliding()
    {
        if (Input.GetKey(KeyCode.E))
        {
            drag = 10.0f;
        }
        else { drag = 0.0f; }
        var vel = rb.velocity;
        vel.y *= 1.0f - Time.deltaTime * drag;
        rb.velocity = vel;
    }
}
