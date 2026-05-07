using Unity.Netcode;
using UnityEngine;

public class Grenade : NetworkBehaviour
{
    [Header("Explosion")]
    [SerializeField] float fuseTime = 3f;
    [SerializeField] float explosionRadius = 5f;
    [SerializeField] float explosionForce = 15f;
    [SerializeField] float damage = 25f;

    bool exploded = false;

    void Start()
    {
        if (IsServer)
        {
            Invoke(nameof(Explode), fuseTime);
        }
    }

    void Explode()
    {
        if (exploded) return;

        exploded = true;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius
        );

        foreach (Collider hit in hits)
        {
            // Damage
            if (hit.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(damage);
            }

            // Push Physics
            if (hit.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddExplosionForce(
                    explosionForce,
                    transform.position,
                    explosionRadius,
                    1f,
                    ForceMode.VelocityChange
                );
            }
        }

        Debug.Log("BOOOOOM");

        NetworkObject.Despawn();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}