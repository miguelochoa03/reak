using Unity.Netcode;
using UnityEngine;

public class PlayerClimb : NetworkBehaviour
{
    Rigidbody rb;

    PlayerInteraction interaction;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        rb = GetComponent<Rigidbody>();

        interaction = GetComponent<PlayerInteraction>();

    }
}
