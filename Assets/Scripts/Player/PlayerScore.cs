//using Unity.Collections;
//using Unity.Netcode;
//using UnityEngine;

//public class PlayerScore : NetworkBehaviour
//{
//    public NetworkVariable<int> Score = new NetworkVariable<int>(
//        0,
//        NetworkVariableReadPermission.Everyone,
//        NetworkVariableWritePermission.Server);

//    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(
//        new FixedString32Bytes("Player"),
//        NetworkVariableReadPermission.Everyone,
//        NetworkVariableWritePermission.Owner);

//    public override void OnNetworkSpawn()
//    {
//        if (!IsOwner) return;
//        if (LobbyManager.Instance != null && !string.IsNullOrEmpty(LobbyManager.Instance.PlayerName))
//        {
//            var n = LobbyManager.Instance.PlayerName;
//            if (n.Length > 31) n = n.Substring(0, 31);
//            PlayerName.Value = new FixedString32Bytes(n);
//        }
//    }

//    public void AddPointServer()
//    {
//        if (!IsServer) return;
//        Score.Value++;
//    }
//}
