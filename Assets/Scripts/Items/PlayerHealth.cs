using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100);

    int maxHealth = 100;

    public void Heal(int amount)
    {
        if (!IsServer) return;

        currentHealth.Value += amount;

        if (currentHealth.Value > maxHealth)
        {
            currentHealth.Value = maxHealth;
        }

        Debug.Log($"Player healed. Health: {currentHealth.Value}");
    }

    public void Damage(int amount)
    {
        if (!IsServer) return;

        currentHealth.Value -= amount;

        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;

            Debug.Log("Player died");
        }
    }
}