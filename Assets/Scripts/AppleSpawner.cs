using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Spawns Apple prefabs randomly inside this GameObject's BoxCollider.
// The BoxCollider should be Is Trigger (non-physical) and can have its renderer disabled
// (invisible) — it's just used to define the spawn volume.
[RequireComponent(typeof(BoxCollider))]
public class AppleSpawner : NetworkBehaviour
{
    public Apple applePrefab;

    [Tooltip("How many apples can exist at once.")]
    public int maxApples = 3;

    BoxCollider area;

    [Header("Timing")]
    [Tooltip("Seconds between spawn attempts. Each tick, if fewer than Max Apples exist, a new one spawns.")]
    public float spawnInterval = 3f;
    [Tooltip("Optional: vertical offset added to chosen spawn position so apples don't bury in the ground.")]
    public float spawnYOffset = 0.5f;

    readonly List<NetworkObject> _live = new List<NetworkObject>();
    float nextSpawnTime;

    void Awake()
    {
        area = GetComponent<BoxCollider>();
        if (area != null) area.isTrigger = true; // ensure it doesn't physically collide
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        if (applePrefab == null) return;
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (!IsServer || applePrefab == null) return;
        if (Time.time < nextSpawnTime) return;
        nextSpawnTime = Time.time + spawnInterval;

        // Prune destroyed/despawned entries from our live list
        _live.RemoveAll(n => n == null || !n.IsSpawned);

        if (_live.Count < maxApples) SpawnOne();
    }

    void SpawnOne()
    {
        if (area == null) area = GetComponent<BoxCollider>();
        if (area == null) return;

        // Random point inside the BoxCollider's local bounds, transformed to world
        Vector3 local = area.center + new Vector3(
            Random.Range(-area.size.x * 0.5f, area.size.x * 0.5f),
            Random.Range(-area.size.y * 0.5f, area.size.y * 0.5f),
            Random.Range(-area.size.z * 0.5f, area.size.z * 0.5f));
        Vector3 pos = transform.TransformPoint(local);
        pos.y += spawnYOffset;

        var inst = Instantiate(applePrefab, pos, Quaternion.identity);
        var no = inst.GetComponent<NetworkObject>();
        no.Spawn(true);
        _live.Add(no);
    }

    public void NotifyEaten(Apple eaten) { /* spawning is now Update-driven; no-op */ }
}
