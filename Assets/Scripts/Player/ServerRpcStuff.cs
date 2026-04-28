using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class ServerRpcStuff : NetworkBehaviour
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
    [ServerRpc]
    public void ChangeToSpecificClientServerRpc(NetworkObjectReference objRef, ulong clientId)
    {
        objRef.TryGet(out NetworkObject obj);
        obj.ChangeOwnership(clientId);
    }
}
