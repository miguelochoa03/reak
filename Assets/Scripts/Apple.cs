using Unity.Netcode;
using UnityEngine;

// World apple — sits in the scene waiting to be picked up.
// AppleSpawner spawns instances of this at configured points.
[RequireComponent(typeof(Collider))]
public class Apple : NetworkBehaviour
{
    public NetworkVariable<bool> Available = new NetworkVariable<bool>(true);

    Renderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        Available.OnValueChanged += (_, val) => SetVisible(val);
    }

    public override void OnNetworkSpawn()
    {
        SetVisible(Available.Value);
    }

    void SetVisible(bool visible)
    {
        if (renderers != null)
            foreach (var r in renderers) r.enabled = visible;
    }

    // Called server-side when a player completes a 3-second pickup.
    public bool TryConsume()
    {
        if (!IsServer || !Available.Value) return false;
        Available.Value = false;
        // Despawn after a short delay so any client visual finishes
        Invoke(nameof(Despawn), 0.1f);
        return true;
    }

    void Despawn()
    {
        if (NetworkObject.IsSpawned) NetworkObject.Despawn(true);
    }
}
