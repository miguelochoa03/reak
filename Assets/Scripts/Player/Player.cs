using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class Player : NetworkBehaviour
{
    Rigidbody rb;
    Vector3 move;
    public CinemachineCamera playerCam;
    public Transform head;

    float h;
    float v;

    float movementSpeed = 5f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        var cam = Instantiate(playerCam);

        cam.Follow = head;
        cam.LookAt = head;

        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // inputs
    void Update()
    {
        if (!IsOwner) return;

        // wasd
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        //float speed = 5f;
        //move = new Vector3(h, 0, v) * speed;

        Transform cam = Camera.main.transform;

        //
        Vector3 bodyEuler = transform.eulerAngles;
        bodyEuler.y = cam.eulerAngles.y;
        transform.eulerAngles = bodyEuler;

        //
        Vector3 headEuler = head.localEulerAngles;
        headEuler.x = cam.eulerAngles.x;
        head.localEulerAngles = headEuler;
    }

    // movement
    void FixedUpdate()
    {
        if (!IsOwner) return;

        // camera 
        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward;
        camForward.y = 0;
        camForward.Normalize();
        
        Vector3 camRight = cam.right;
        camRight.y = 0;
        camRight.Normalize();

        // move based on direction
        Vector3 moveDir = camRight * h + camForward * v;

        // wasd movement
        rb.linearVelocity = new Vector3(moveDir.x, rb.linearVelocity.y, moveDir.z) * movementSpeed;
        //rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}
