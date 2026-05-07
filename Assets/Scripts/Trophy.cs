//using Unity.Netcode;
//using UnityEngine;

//[RequireComponent(typeof(Collider))]
//public class Trophy : NetworkBehaviour
//{
//    [Tooltip("Optional: positions to relocate the trophy to after a player grabs it. Random pick. If empty, trophy disappears for `respawnDelay` seconds in place.")]
//    public Transform[] respawnPoints;
//    public float respawnDelay = 1.5f;

//    [Tooltip("Optional one-shot sound played for everyone when grabbed.")]
//    public AudioSource collectSound;

//    bool collected;
//    Renderer[] renderers;
//    Collider trophyCollider;

//    void Awake()
//    {
//        renderers = GetComponentsInChildren<Renderer>();
//        trophyCollider = GetComponent<Collider>();
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (!IsServer || collected) return;

//        var no = other.GetComponentInParent<NetworkObject>();
//        if (no == null) return;
//        var score = no.GetComponent<PlayerScore>();
//        if (score == null) return;

//        collected = true;

//        // 1) Award point
//        score.AddPointServer();

//        // 2) Teleport every player to their own spawn position
//        foreach (var c in NetworkManager.Singleton.ConnectedClientsList)
//        {
//            if (c.PlayerObject == null) continue;
//            var pmc = c.PlayerObject.GetComponent<PlayerMovementCamera>();
//            if (pmc != null) pmc.TeleportToSpawnRpc();
//        }

//        // 3) Play sound + hide trophy + relocate
//        PlayCollectSoundRpc();
//        SetVisibleRpc(false);

//        if (respawnPoints != null && respawnPoints.Length > 0)
//        {
//            var p = respawnPoints[Random.Range(0, respawnPoints.Length)];
//            transform.position = p.position;
//            transform.rotation = p.rotation;
//        }

//        Invoke(nameof(ReadyAgain), respawnDelay);
//    }

//    void ReadyAgain()
//    {
//        if (!IsServer) return;
//        SetVisibleRpc(true);
//        collected = false;
//    }

//    [Rpc(SendTo.ClientsAndHost)]
//    void PlayCollectSoundRpc()
//    {
//        if (collectSound != null) collectSound.Play();
//    }

//    [Rpc(SendTo.ClientsAndHost)]
//    void SetVisibleRpc(bool visible)
//    {
//        if (renderers != null)
//            foreach (var r in renderers) r.enabled = visible;
//        if (trophyCollider != null) trophyCollider.enabled = visible;
//    }
//}
