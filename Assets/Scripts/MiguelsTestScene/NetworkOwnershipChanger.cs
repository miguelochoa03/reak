using Unity.Netcode;
using UnityEngine;

public class NetworkOwnershipChanger : NetworkBehaviour
{
    [ServerRpc]
    public void ChangeToClientServerRpc(NetworkObjectReference objRef, ServerRpcParams rpc = default)
    {
        objRef.TryGet(out NetworkObject obj);
        obj.ChangeOwnership(rpc.Receive.SenderClientId);
    }
    [ServerRpc]
    public void ChangeToServerServerRpc(NetworkObjectReference objRef)
    {
        objRef.TryGet(out NetworkObject obj);
        obj.ChangeOwnership(0);
    }
}
