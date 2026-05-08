using Unity.Netcode;
using UnityEngine;

// Spawned when a player throws an apple. Server-authoritative collision.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class AppleProjectile : NetworkBehaviour
{
    public float stunDuration = 2f;
    public float lifetime = 6f;
    [Tooltip("Force applied to the hit player on impact, opposite to the apple's velocity (knockback feel).")]
    public float knockbackForce = 8f;

    Rigidbody rb;
    bool consumed;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        if (IsServer) Invoke(nameof(SelfDespawn), lifetime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (!IsServer || consumed) return;

        var no = col.collider.GetComponentInParent<NetworkObject>();
        if (no == null) { SelfDespawn(); return; }

        // Don't ragdoll the thrower (avoid self-hits)
        if (no.OwnerClientId == OwnerClientId) return;

        var stun = no.GetComponent<PlayerStun>();
        if (stun != null)
        {
            stun.StunForRpc(stunDuration);
            // Optional: knockback impulse (server-side; ClientNetworkTransform will sync it)
            var hitRb = no.GetComponent<Rigidbody>();
            if (hitRb != null && rb != null)
            {
                Vector3 dir = rb.linearVelocity.normalized;
                if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
                hitRb.AddForce(dir * knockbackForce, ForceMode.VelocityChange);
            }
        }

        consumed = true;
        SelfDespawn();
    }

    void SelfDespawn()
    {
        if (!IsServer) return;
        if (NetworkObject != null && NetworkObject.IsSpawned) NetworkObject.Despawn(true);
    }
}
