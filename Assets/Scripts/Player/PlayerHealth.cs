using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public float health = 100;

    public Material dieMaterial;

    public Renderer body;
    public Renderer tophead;
    public Renderer bottomhead;

    Rigidbody rb;

    HandleRagdoll ragdoll;

    bool didDie = false;
    public void TakeDamage(float dmg)
    {
        health -= dmg;
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        rb = GetComponent<Rigidbody>();

        ragdoll = GetComponent<HandleRagdoll>();

        body = transform.Find("body").GetComponent<Renderer>();
        tophead = transform.Find("head").Find("tophead").GetComponent<Renderer>();
        bottomhead = transform.Find("head").Find("bottomhead").GetComponent<Renderer>();
    }

    private void Update()
    {
        if (!IsOwner || didDie) return;

        if (health <= 0)
        {
            body.material = dieMaterial;
            tophead.material = dieMaterial;
            bottomhead.material = dieMaterial;

            // ragdoll

            ragdoll.TurnOn();

            didDie = true;
        }
    }
}
