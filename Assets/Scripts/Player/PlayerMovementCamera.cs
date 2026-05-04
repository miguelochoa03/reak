using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System.Collections;
using static UnityEngine.UI.Image;

public class PlayerMovementCamera : NetworkBehaviour
{
    public Transform head;
    public Transform LookPoint;

    Rigidbody rb;
    float movementSpeed = 5f;
    const float origMovementSpeed = 5f;

    public CinemachineCamera cam;
    Transform transCam;

    float h, v;

    float headAngle = 0f;

    bool canJump = false;
    bool isGrounded = false;
    float jumpForce = 14f;

    IEnumerator TryToPreventFlingOnSpawn()
    {
        rb.isKinematic = true;
        //transform.position += new Vector3(Random.Range(-5f, 5f), Random.Range(0.5f, 3f), Random.Range(-5f, 5f));
        //yield return new WaitForSeconds(Random.Range(2f,6f));
        yield return new WaitForSeconds(1f);
        rb.isKinematic = false;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        rb = GetComponent<Rigidbody>();

        cam = Instantiate(cam);

        cam.Follow = LookPoint;
        cam.LookAt = LookPoint;

        //transCam = cam.transform;
        transCam = Camera.main.transform;


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
        //headAngle = transCam.eulerAngles.x;
        //if (headAngle > 180f)
        //{
        //    headAngle -= 360f;
        //}
        //headAngle = Mathf.Clamp(headAngle, -40f, 40f);
        //head.localEulerAngles = new Vector3(headAngle, 0f, 0f);

        // sprint input
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = 8f;
        }
        else
        {
            movementSpeed = origMovementSpeed;
        }

        // jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            canJump = true;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);
        Debug.DrawRay(transform.position, Vector3.down * 0.1f, Color.red); // see visually in scene view (not game view)

        if (canJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            canJump = false;
        }

        // ignore vertical vector relative to camera (forward and backward movement)
        Vector3 camForward = transCam.forward;
        camForward.y = 0;
        camForward.Normalize();

        // ignore vertical vector relative to camera (right and left movement)
        Vector3 camRight = transCam.right;
        camRight.y = 0;
        camRight.Normalize();

        // move based on direction
        Vector3 moveDir = camRight * h + camForward * v;

        // wasd movement
        rb.linearVelocity = new Vector3(moveDir.x * movementSpeed, rb.linearVelocity.y, moveDir.z * movementSpeed);
    }
}
