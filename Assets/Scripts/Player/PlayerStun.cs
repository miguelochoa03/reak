using System.Collections;
using Unity.Netcode;
using UnityEngine;

// Disables movement/input on a player for a set duration.
// Triggered by an external system (apple projectile hit, etc.)
public class PlayerStun : NetworkBehaviour
{
    PlayerMovementCamera movement;
    PlayerInteraction interaction;
    Coroutine routine;

    public override void OnNetworkSpawn()
    {
        movement = GetComponent<PlayerMovementCamera>();
        interaction = GetComponent<PlayerInteraction>();
    }

    // Server tells the owner to stun themselves for N seconds.
    // Owner gates their own input — visually & functionally feels right.
    [Rpc(SendTo.Owner)]
    public void StunForRpc(float seconds)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(StunRoutine(seconds));
    }

    IEnumerator StunRoutine(float seconds)
    {
        SetStunned(true);
        yield return new WaitForSeconds(seconds);
        SetStunned(false);
        routine = null;
    }

    void SetStunned(bool stunned)
    {
        if (movement != null) movement.enabled = !stunned;
        if (interaction != null) interaction.enabled = !stunned;

        // Try to trigger visual ragdoll if HandleRagdoll exists on this prefab.
        var ragdoll = GetComponent<HandleRagdoll>();
        if (ragdoll != null)
        {
            if (stunned) ragdoll.TurnOn();
            else ragdoll.TurnOff();
        }
    }
}
