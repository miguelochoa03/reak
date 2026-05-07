using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Cinemachine;
using System.Collections;
using static UnityEngine.UI.Image;

public class PlayerMovementCamera : NetworkBehaviour
{
    PlayerAnimation anim;

    public Transform head;
    public Transform LookPoint;

    public Vector3 spawnPosition = new Vector3(0f, 3f, 0f);

    [Header("Movement")]
    public float walkSpeed = 9f;
    public float sprintSpeed = 14f;
    public float jumpForce = 22f;
    [Tooltip("Cap on downward velocity. Lower = floatier, higher = faster falls.")]
    public float maxFallSpeed = 80f;
    [Tooltip("Multiplier for gravity while ASCENDING. <1 = lighter (longer jump), >1 = heavier.")]
    [Range(0.2f, 4f)] public float gravityScale = 1.5f;
    [Tooltip("Extra multiplier applied while FALLING. Adds on top of gravityScale to make fall snappier than ascent.")]
    [Range(1f, 6f)] public float fallGravityMultiplier = 3f;

    [Header("Camera")]
    [Tooltip("FOV applied to the FP Camera virtual camera at spawn.")]
    public float fov = 80f;

    [Header("Perspective Toggle")]
    public KeyCode togglePerspectiveKey = KeyCode.P;
    [Tooltip("If on: 3rd person camera is in front of player (selfie). Off: behind player (standard).")]
    public bool tpFrontView = false;
    [Tooltip("Distance from player in 3rd person view.")]
    public float tpDistance = 3.5f;
    [Tooltip("Vertical offset above the look-point in 3rd person.")]
    public float tpHeight = 0.3f;
    public float tpMouseSensitivity = 3f;
    public float tpPitchMin = -30f;
    public float tpPitchMax = 60f;

    CinemachineCamera tpFrontCam;
    bool isThirdPerson;
    float tpYaw, tpPitch;

    Rigidbody rb;
    float movementSpeed;

    public CinemachineCamera cam;
    Transform transCam;

    float h, v;

    bool canJump = false;
    bool isGrounded = false;

    void Animations()
    {
        bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        if (isMoving)
        {
            anim.PlayWalk();

            return;
        }

        //anim.PlayIdle();
        anim.PlayNothing();
    }
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

        transform.position = spawnPosition;

        rb = GetComponent<Rigidbody>();

        anim = GetComponent<PlayerAnimation>();

        cam = Instantiate(cam);
        DontDestroyOnLoad(cam.gameObject);

        cam.Follow = LookPoint;
        cam.LookAt = LookPoint;
        var lens = cam.Lens;
        lens.FieldOfView = fov;
        cam.Lens = lens;
        cam.Priority = 20;

        SetupThirdPersonCamera();

        transCam = Camera.main != null ? Camera.main.transform : null;


        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(TryToPreventFlingOnSpawn());
    }

    void SetupThirdPersonCamera()
    {
        var go = new GameObject("TP_Front_Cam");
        DontDestroyOnLoad(go);

        // No Body / no Aim components — we drive the transform manually in LateUpdate.
        tpFrontCam = go.AddComponent<CinemachineCamera>();
        tpFrontCam.Follow = null;
        tpFrontCam.LookAt = null;

        var lens = tpFrontCam.Lens;
        lens.FieldOfView = fov;
        tpFrontCam.Lens = lens;
        tpFrontCam.Priority = 10; // start lower than FP cam

        ApplyInstantBrainBlend();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // Each new scene has its own CinemachineBrain with default 2s blend — kill it.
        ApplyInstantBrainBlend();
        // Also refresh transCam since the old Main Camera is gone.
        transCam = Camera.main != null ? Camera.main.transform : null;
    }

    void ApplyInstantBrainBlend()
    {
        var brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;
        if (brain != null)
        {
            var blend = brain.DefaultBlend;
            blend.Time = 0f;
            brain.DefaultBlend = blend;
        }
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [Rpc(SendTo.Owner)]
    public void TeleportToSpawnRpc()
    {
        transform.position = spawnPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void TogglePerspective()
    {
        isThirdPerson = !isThirdPerson;
        if (isThirdPerson)
        {
            tpYaw = transform.eulerAngles.y;
            tpPitch = 0f;
        }
        if (cam != null) cam.Priority = isThirdPerson ? 10 : 20;
        if (tpFrontCam != null) tpFrontCam.Priority = isThirdPerson ? 20 : 10;
    }

    void LateUpdate()
    {
        if (!IsOwner || !isThirdPerson || tpFrontCam == null || LookPoint == null) return;

        tpYaw   += Input.GetAxis("Mouse X") * tpMouseSensitivity;
        tpPitch -= Input.GetAxis("Mouse Y") * tpMouseSensitivity;
        tpPitch  = Mathf.Clamp(tpPitch, tpPitchMin, tpPitchMax);

        // Position camera relative to player at chosen yaw/pitch
        Quaternion camRot = Quaternion.Euler(tpPitch, tpYaw, 0f);
        Vector3 dirFromPlayer = camRot * (tpFrontView ? Vector3.forward : Vector3.back);
        Vector3 camPos = LookPoint.position + dirFromPlayer * tpDistance + Vector3.up * tpHeight;
        tpFrontCam.transform.position = camPos;
        tpFrontCam.transform.rotation = Quaternion.LookRotation(LookPoint.position - camPos);
    }

    void Update()
    {
        if (!IsOwner) return;

        // Re-grab Main Camera if the previous one was destroyed (scene change).
        if (transCam == null) transCam = Camera.main != null ? Camera.main.transform : null;
        if (transCam == null) return;

        // wasd inputs
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        // rotates body horizontally — driven by FP cam in first person, by tpYaw in third person
        Vector3 bodyEuler = transform.eulerAngles;
        bodyEuler.y = isThirdPerson ? tpYaw : transCam.eulerAngles.y;
        transform.eulerAngles = bodyEuler;

        // sprint input
        movementSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        // jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            canJump = true;
        }

        // perspective toggle
        if (Input.GetKeyDown(togglePerspectiveKey)) TogglePerspective();

        Animations();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (transCam == null) transCam = Camera.main != null ? Camera.main.transform : null;
        if (transCam == null) return;

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

        // Asymmetric gravity: lighter on ascent (snappy peak), heavier on descent (no float).
        bool falling = rb.linearVelocity.y < 0f;
        float effectiveScale = falling ? gravityScale * fallGravityMultiplier : gravityScale;
        float extraG = (effectiveScale - 1f) * Physics.gravity.y;
        rb.AddForce(Vector3.up * extraG, ForceMode.Acceleration);

        // Cap fall speed so terminal velocity feels reasonable
        float vy = Mathf.Max(rb.linearVelocity.y, -maxFallSpeed);

        // wasd movement
        rb.linearVelocity = new Vector3(moveDir.x * movementSpeed, vy, moveDir.z * movementSpeed);
    }
}
