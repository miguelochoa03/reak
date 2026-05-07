using Unity.Netcode;
using UnityEngine;

public class PoisonApple : NetworkBehaviour
{
    [SerializeField] float healAmount = -10f;

    bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || used) return;

        if (other.TryGetComponent<PlayerHealth>(out var health))
        {
            health.health += healAmount;

            // Cap Health at 100
            if (health.health > 100)
            {
                health.health = 100;
            }

            used = true;

            NetworkObject.Despawn();
        }
    }
}
