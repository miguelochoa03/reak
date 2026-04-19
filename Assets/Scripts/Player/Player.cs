using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System.Collections;

public class Player : NetworkBehaviour
{
    Rigidbody rb;
    public CinemachineCamera playerCam;
    public Transform head;
    public Transform LookPoint;

    float h;
    float v;

    float movementSpeed = 5f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        var cam = Instantiate(playerCam);

        cam.Follow = LookPoint;
        cam.LookAt = LookPoint;

        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Update()
    {
        if (!IsOwner) return;

        // wasd inputs
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        // camera transform
        Transform cam = Camera.main.transform;

        // angle values
        float cameraAngle = cam.eulerAngles.y;
        float bodyAngle = transform.eulerAngles.y;

        // how far the head is turned relative to the body
        float angleDiff = Mathf.DeltaAngle(bodyAngle, cameraAngle);

        // head rotation
        Vector3 headEuler = head.localEulerAngles;
        headEuler.x = cam.eulerAngles.x;
        headEuler.y = angleDiff;
        head.localEulerAngles = headEuler;

        // rotate body cases
        float limit = 60f;
        bool isMoving = (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f);

        if (isMoving || Mathf.Abs(angleDiff) > limit)
        {
            float newAngle = Mathf.LerpAngle(bodyAngle, cameraAngle, Time.deltaTime * 5f);
            transform.eulerAngles = new Vector3(0f, newAngle, 0f);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // camera transform
        Transform cam = Camera.main.transform;

        // ignore vertical vector relative to camera (forward and backward movement)
        Vector3 camForward = cam.forward;
        camForward.y = 0;
        camForward.Normalize();

        // ignore vertical vector relative to camera (right and left movement)
        Vector3 camRight = cam.right;
        camRight.y = 0;
        camRight.Normalize();

        // move based on direction
        Vector3 moveDir = camRight * h + camForward * v;

        // wasd movement
        rb.linearVelocity = new Vector3(moveDir.x, rb.linearVelocity.y, moveDir.z) * movementSpeed;
    }

}
