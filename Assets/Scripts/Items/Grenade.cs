using Unity.Netcode;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Grenade : NetworkBehaviour
{
    [Header("Explosion")]
    [SerializeField] float fuseTime = 5f;
    [SerializeField] float explosionRadius = 5f;
    [SerializeField] float explosionForce = 15f;
    [SerializeField] float damage = 25f;

    bool exploded = false;

    public UnityEvent explosion;

    void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    void Explode()
    {
        if (exploded) return;

        exploded = true;

        Debug.Log("BOOOOOM");

        explosion.Invoke();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius
        );

        foreach (Collider hit in hits)
        {
            // DAMAGE
            if (hit.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(damage);
            }

            // PUSH
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

        //NetworkObject.Despawn();

        StartCoroutine(Disappear());
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }

    public IEnumerator Disappear()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}