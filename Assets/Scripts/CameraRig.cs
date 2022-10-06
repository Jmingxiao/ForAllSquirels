using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    Vector2 rotationalInput;
    Quaternion targetRotation;

    public enum Mode
    {
        Free,
        Follow
    }
    public enum Dir
    {
       Up,Down,Left,Right,Front,Back
    }

    public Mode cameraMode = Mode.Free;

    public Quaternion GetTargetRotation() { return targetRotation; }

    [Range(0, 1)]
    [SerializeField] float followSmoothing = 0.05f;

    [Range(0, 1)]
    [SerializeField] float rotationSmoothing = 0.4f;

    float rotationSpeed = 40;
    float zoomSpeed = 100;

    float minDistance = 1.0f;
    float maxDistance = 3.0f;

    public LayerMask layer;
    public Transform target;
    private Vector3 cameraPosition;
    private Transform TheCamera;
    Vector3 prevPos;
    Vector3 currentVelocity;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Application.targetFrameRate = 60;
        transform.position = target.position;
        cameraPosition = new Vector3(0, 0.3f, -maxDistance);
        prevPos = cameraPosition;
        TheCamera = Camera.main.transform;
        TheCamera.localPosition = cameraPosition;
        
    }

    private void Update()
    {
        rotationalInput.x += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        rotationalInput.y -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        rotationalInput.y = Mathf.Clamp(rotationalInput.y, -60, 60);

        if (cameraMode == Mode.Follow)
        {
            rotationalInput.x = target.rotation.eulerAngles.y;
            rotationalInput.y = 15;
        }

        targetRotation = Quaternion.Euler(rotationalInput.y, rotationalInput.x, 0);

        cameraPosition.z += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
        cameraPosition.z = Mathf.Clamp(cameraPosition.z, -maxDistance, -minDistance);
    }

    private void FixedUpdate()
    {
        if (Physics.CheckSphere(TheCamera.position, 1.5f, layer))
        {
            for(int i =1; i<6; i++)
            {
                if (detect((Dir)i))
                {
                    return;
                }
            }
        }
            // rig position
            if (transform.position != target.position)
                transform.position = Vector3.SmoothDamp(transform.position, target.position, ref currentVelocity, followSmoothing);

            // rig rotation
            if (transform.rotation != targetRotation)
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothing);



            // camera position
            if (Camera.main.transform.localPosition != cameraPosition)
                Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, cameraPosition, 0.25f);
      
    }
    bool detect(Dir dir)
    {
        switch (dir)
        {
            case Dir.Down:
                if (Physics.Raycast(TheCamera.position, Vector3.down, 0.2f, layer))
                {
                    Debug.Log(dir);
                    if (cameraPosition.y < TheCamera.localPosition.y)
                        return true;
                }
                break;
            case Dir.Left:
                if (Physics.Raycast(TheCamera.position, Vector3.left, 0.2f, layer))
                {
                    Debug.Log(dir);
                    if (cameraPosition.x < TheCamera.localPosition.x)
                        return true;
                }
                break;
            case Dir.Right:
                if (Physics.Raycast(TheCamera.position, Vector3.right, 0.2f, layer))
                {
                    Debug.Log(dir);
                    if (cameraPosition.x > TheCamera.localPosition.x)
                        return true;
                }
                break;
            case Dir.Front:
               
                if (Physics.Raycast(TheCamera.position, Vector3.forward, 0.2f, layer))
                {
                    Debug.Log(dir);
                    if (cameraPosition.z > TheCamera.localPosition.z)
                        return true;
                }
                break;
            case Dir.Back:
                if (Physics.Raycast(TheCamera.position, Vector3.back, 0.2f, layer))
                {
                    Debug.Log(dir);
                    if (cameraPosition.z < TheCamera.localPosition.z)
                        return true;
                }
                break;
        }

        return false;
    }
    public void SetMode(Mode mode)
    {
        if (mode == Mode.Free)
        {
            cameraMode = Mode.Free;
            rotationSmoothing = 0.4f;
        }
        else if (mode == Mode.Follow)
        {
            cameraMode = Mode.Follow;
            rotationSmoothing = 0.1f;
        }
    }
}
