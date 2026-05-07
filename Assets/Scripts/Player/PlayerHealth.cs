using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public float health = 100;

    public Material dieMaterial;

    GameObject body;
    GameObject tophead;
    GameObject bottomhead;

    Rigidbody rb;

    bool didDie = false;
    public void TakeDamage(float dmg)
    {
        health -= dmg;
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        rb = GetComponent<Rigidbody>();

        body = transform.Find("body").gameObject;
        tophead = transform.Find("head").Find("tophead").gameObject;
        bottomhead = transform.Find("head").Find("bottomhead").gameObject;
    }

    private void Update()
    {
        if (!IsOwner || didDie) return;

        if (health <= 0)
        {
            body.GetComponent<Renderer>().material = dieMaterial;
            tophead.GetComponent<Renderer>().material = dieMaterial;
            bottomhead.GetComponent<Renderer>().material = dieMaterial;

            // ragdoll


            didDie = true;
        }
    }
}
