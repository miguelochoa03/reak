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

    float mouseX;
    float mouseY;
    float sens = 200f;
    float xRotation = 0f;

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
        float speed = 5f;
        move = new Vector3(h, 0, v) * speed;

        // mouse
        mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;

        // vertical rotate
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        //head.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // horizontal rotate
        transform.Rotate(Vector3.up * mouseX);

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
