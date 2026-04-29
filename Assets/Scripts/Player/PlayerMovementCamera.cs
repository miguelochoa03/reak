using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System.Collections;

public class PlayerMovementCamera : NetworkBehaviour
{
    public Transform head;
    public Transform LookPoint;

    Rigidbody rb;
    float movementSpeed = 5f;

    public CinemachineCamera cam;
    Transform transCam;

    float h, v;

    float headAngle = 0f;

    IEnumerator TryToPreventFlingOnSpawn()
    {
        rb.isKinematic = true;
        transform.position += new Vector3(Random.Range(-5f, 5f), Random.Range(0.5f, 3f), Random.Range(-5f, 5f));
        yield return new WaitForSeconds(Random.Range(2f,6f));
        //yield return new WaitForSeconds(1f);
        rb.isKinematic = false;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        rb = GetComponent<Rigidbody>();

        cam = Instantiate(cam);

        cam.Follow = LookPoint;
        cam.LookAt = LookPoint;

        transCam = cam.transform;

        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(TryToPreventFlingOnSpawn());
    }

    void Update()
    {
        if (!IsOwner) return;

        // wasd inputs
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        // rotates body horizontally
        Vector3 bodyEuler = transform.eulerAngles;
        bodyEuler.y = transCam.eulerAngles.y;
        transform.eulerAngles = bodyEuler;

        // rotates head vertically
        headAngle = transCam.eulerAngles.x;
        if (headAngle > 180f)
        {
            headAngle -= 360f;
        }
        headAngle = Mathf.Clamp(headAngle, -40f, 40f);
        head.localEulerAngles = new Vector3(headAngle, 0f, 0f);
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // max velocity
        //Vector3 maxv = rb.linearVelocity;
        //maxv.x = Mathf.Clamp(maxv.x, -3f, 3f);
        //maxv.y = Mathf.Clamp(maxv.y, -3f, 7f);
        //maxv.z = Mathf.Clamp(maxv.z, -3f, 3f);
        //rb.linearVelocity = maxv;

        // control velocity (prevent flings) //
        Vector3 controlledVelocity = rb.linearVelocity;

        // prevent flings but allows room for jump
        if (controlledVelocity.y > 7f || controlledVelocity.x > 7f || controlledVelocity.z > 7f)
        {
            controlledVelocity.y = 0f;
            controlledVelocity.x = 0f;
            controlledVelocity.z = 0f;
        }

        // max fall velocity
        if (controlledVelocity.y < -3f)
        {
            controlledVelocity.y = -3f;
        }

        // max side velocity
        controlledVelocity.x = Mathf.Clamp(controlledVelocity.x, -5f, 5f);
        controlledVelocity.z = Mathf.Clamp(controlledVelocity.z, -5f, 5f);

        // set that velocity //
        rb.linearVelocity = controlledVelocity;

        // camera transform
        //Transform cam = Camera.main.transform;
        Transform TransformCam = cam.transform;

        // ignore vertical vector relative to camera (forward and backward movement)
        Vector3 camForward = TransformCam.forward;
        camForward.y = 0;
        camForward.Normalize();

        // ignore vertical vector relative to camera (right and left movement)
        Vector3 camRight = TransformCam.right;
        camRight.y = 0;
        camRight.Normalize();

        // move based on direction
        Vector3 moveDir = camRight * h + camForward * v;

        // wasd movement
        rb.linearVelocity = new Vector3(moveDir.x, rb.linearVelocity.y, moveDir.z) * movementSpeed;
    }
}
