using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class Player : NetworkBehaviour
{
    Rigidbody rb;
    Vector3 move;
    public CinemachineCamera playerCam;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
    }

    // inputs
    void Update()
    {
        if (!IsOwner) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float speed = 5f;
        move = new Vector3(h, 0, v) * speed;

    }

    // movement
    void FixedUpdate()
    {
        if (!IsOwner) return;

        // movement
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}
