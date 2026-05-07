using Unity.Netcode;
using UnityEngine;

public class Apple : NetworkBehaviour
{
    [SerializeField] int healAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<PlayerHealth>(out var health)) //PlayerHealth is a placeholder script call for wherever health will be handled
        {
            health.Heal(healAmount);

            NetworkObject.Despawn();
        }
    }
}