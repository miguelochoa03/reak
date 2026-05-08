using Unity.Netcode;
using UnityEngine;

// Hold E for 3 seconds while looking at an Apple to pick it up.
// Press F (default) to throw, applying forward velocity.
// Server is authoritative for both pickup and throw.
public class ApplePickupAndThrow : NetworkBehaviour
{
    [Header("Pickup")]
    public KeyCode pickupKey = KeyCode.E;
    public float pickupHoldTime = 3f;
    [Tooltip("Radius around the player to search for apples to pick up.")]
    public float pickupRadius = 3f;

    [Header("Throw")]
    [Tooltip("Mouse button to throw. 0=Left, 1=Right, 2=Middle.")]
    public int throwMouseButton = 0;
    public AppleProjectile appleProjectilePrefab;
    public float throwForce = 18f;
    public float spawnDistance = 1.2f;
    [Tooltip("Seconds you must wait between throws.")]
    public float throwCooldown = 1f;

    float nextThrowTime;

    [Header("References")]
    public Transform LookPoint;

    [Tooltip("Optional UI text that shows pickup progress / apple count. Hooked up if assigned.")]
    public TMPro.TMP_Text statusLabel;

    public NetworkVariable<int> AppleCount = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    Transform camTransform;
    Apple holdingTarget;
    float holdProgress;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        if (LookPoint == null)
        {
            var pmc = GetComponent<PlayerMovementCamera>();
            if (pmc != null) LookPoint = pmc.LookPoint;
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        if (camTransform == null) camTransform = Camera.main != null ? Camera.main.transform : null;
        if (camTransform == null || LookPoint == null) return;

        HandlePickup();
        HandleThrow();
    }

    void HandlePickup()
    {
        bool holding = Input.GetKey(pickupKey);

        // Proximity pickup: find the nearest available Apple within pickupRadius.
        Apple nearest = FindNearestApple();

        if (holding)
        {
            int total = Object.FindObjectsByType<Apple>(FindObjectsSortMode.None).Length;
            Debug.Log($"[Pickup] Holding E. Apples in scene: {total}. Nearest in radius: {(nearest != null ? nearest.name : "none")}");
        }

        if (holding && nearest != null)
        {
            // Started or continued holding on the same apple
            if (holdingTarget != nearest)
            {
                holdingTarget = nearest;
                holdProgress = 0f;
            }
            holdProgress += Time.deltaTime;
            UpdateStatus($"Picking up... {holdProgress:0.0}/{pickupHoldTime:0.0}s");

            if (holdProgress >= pickupHoldTime)
            {
                RequestPickupRpc(holdingTarget.NetworkObject);
                holdingTarget = null;
                holdProgress = 0f;
            }
        }
        else
        {
            // Released, or no apple nearby
            if (holdingTarget != null)
            {
                holdingTarget = null;
                holdProgress = 0f;
                UpdateStatus("");
            }
        }
    }

    Apple FindNearestApple()
    {
        Apple[] all = Object.FindObjectsByType<Apple>(FindObjectsSortMode.None);
        Apple nearest = null;
        float bestDistSq = pickupRadius * pickupRadius;
        Vector3 me = transform.position;
        float closestSeenSq = float.MaxValue;
        foreach (var a in all)
        {
            if (a == null || !a.Available.Value) continue;
            float d = (a.transform.position - me).sqrMagnitude;
            if (d < closestSeenSq) closestSeenSq = d;
            if (d <= bestDistSq) { nearest = a; bestDistSq = d; }
        }
        if (nearest == null && Input.GetKey(pickupKey) && all.Length > 0)
            Debug.Log($"[Pickup] Closest apple is {Mathf.Sqrt(closestSeenSq):0.0}m away — pickupRadius={pickupRadius}m. Bump pickupRadius higher.");
        return nearest;
    }

    void HandleThrow()
    {
        if (!Input.GetMouseButtonDown(throwMouseButton)) return;
        if (Time.time < nextThrowTime)
        {
            UpdateStatus($"Cooldown {nextThrowTime - Time.time:0.0}s");
            return;
        }
        if (AppleCount.Value <= 0) { UpdateStatus("No apples"); return; }

        nextThrowTime = Time.time + throwCooldown;
        Vector3 origin = LookPoint.position + camTransform.forward * spawnDistance;
        Vector3 dir = camTransform.forward;
        RequestThrowRpc(origin, dir);
    }

    [Rpc(SendTo.Server)]
    void RequestPickupRpc(NetworkObjectReference appleRef)
    {
        if (!appleRef.TryGet(out NetworkObject no)) return;
        var apple = no.GetComponent<Apple>();
        if (apple == null) return;

        if (apple.TryConsume())
        {
            AppleCount.Value++;
            // Notify spawner to schedule a respawn
            var spawner = Object.FindFirstObjectByType<AppleSpawner>();
            if (spawner != null) spawner.NotifyEaten(apple);
        }
    }

    [Rpc(SendTo.Server)]
    void RequestThrowRpc(Vector3 origin, Vector3 dir)
    {
        if (AppleCount.Value <= 0) return;
        if (appleProjectilePrefab == null) return;

        AppleCount.Value--;

        var inst = Instantiate(appleProjectilePrefab, origin, Quaternion.LookRotation(dir));
        var no = inst.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(OwnerClientId);

        var rb = inst.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = dir.normalized * throwForce;
    }

    void UpdateStatus(string s)
    {
        if (statusLabel != null) statusLabel.text = s;
    }
}
