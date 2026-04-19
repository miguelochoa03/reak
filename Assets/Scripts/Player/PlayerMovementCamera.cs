using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System.Collections;

public class PlayerMovementCamera : NetworkBehaviour
{
    Rigidbody rb;
    public CinemachineCamera playerCam;
    public Transform head;
    public Transform LookPoint;

    float h;
    float v;

    float movementSpeed = 5f;

    float headangle = 0f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        var cam = Instantiate(playerCam);

        cam.Follow = LookPoint;
        cam.LookAt = LookPoint;

        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;


        StartCoroutine(TryToPreventFlingOnSpawn());
    }

    IEnumerator TryToPreventFlingOnSpawn()
    {
        transform.position += new Vector3(Random.Range(-5f, 5f), Random.Range(0.5f, 3f), Random.Range(-5f, 5f));
        //yield return new WaitForSeconds(Random.Range(2f,6f));
        yield return new WaitForSeconds(0f);
        rb.isKinematic = false;
    }

    void Update()
    {
        if (!IsOwner) return;

        // wasd inputs
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        // camera transform
        Transform cam = Camera.main.transform;

        // rotates body horizontally
        Vector3 bodyEuler = transform.eulerAngles;
        bodyEuler.y = cam.eulerAngles.y;
        transform.eulerAngles = bodyEuler;

        // rotates head vertically
        headangle = cam.eulerAngles.x;
        if (headangle > 180f)
        {
            headangle -= 360f;
        }
        headangle = Mathf.Clamp(headangle, -40f, 40f);
        head.localEulerAngles = new Vector3 (headangle, 0f, 0f);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // max velocity
        Vector3 maxv = rb.linearVelocity;
        maxv.x = Mathf.Clamp(maxv.x, -3f, 3f);
        maxv.y = Mathf.Clamp(maxv.y, -3f, 7f);
        maxv.z = Mathf.Clamp(maxv.z, -3f, 3f);
        rb.linearVelocity = maxv;

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
