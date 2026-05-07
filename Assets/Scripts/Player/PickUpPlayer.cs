using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PickUpPlayer : NetworkBehaviour
{
    private ulong clientId;

    PlayerInteraction interaction;
    bool interactionray;
    RaycastHit interactionhit;

    PlayerAnimation anim;

    Rigidbody heldplayerrb;
    Pickupable heldPlayer;

    Transform transCam;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        interaction = GetComponent<PlayerInteraction>();
        anim = GetComponent<PlayerAnimation>();
        transCam = GetComponent<PlayerInteraction>().transCam;

    }

    void Update()
    {
        if (!IsOwner) return;

        // check ray cast hit anything
        if (interactionray)
        {
            // check if pickupable
            bool IsPickupAble = interactionhit.collider.TryGetComponent<Pickupable>(out var pickup);
            if (IsPickupAble)
            {
                Debug.Log("This can be picked up, name: " + interactionhit.collider.name);

                // pick up object
                if (Input.GetMouseButtonDown(0))
                {
                    heldPlayer = pickup;



                    // change ownership to client messing with the poor player
                    var heldPlayerNetworkObject = heldPlayer.GetComponent<NetworkObject>();
                    clientId = heldPlayerNetworkObject.OwnerClientId;
                    GetComponent<ServerRpcStuff>().ChangeToClientServerRpc(heldPlayerNetworkObject);
                }
            }
        }



        // move pick up object
        if (Input.GetMouseButton(0) && heldPlayer != null)
        {
            // move the object to the end of the raycast

            heldplayerrb = heldPlayer.GetComponent<Rigidbody>();

            // avoid gravity building up
            heldplayerrb.useGravity = false;

            heldplayerrb.linearDamping = 16f;
            heldplayerrb.angularDamping = 1f;

            Vector3 targetPos = GetComponent<PlayerInteraction>().targetPos;
            Vector3 toTarget = targetPos - heldPlayer.transform.position;
            heldplayerrb.AddForce(toTarget * 0.05f);
        }
        // drop pick up object
        if (Input.GetMouseButtonUp(0) && heldPlayer != null)
        {
            heldplayerrb = heldPlayer.GetComponent<Rigidbody>();

            // give gravity back
            heldplayerrb.useGravity = true;

            heldplayerrb.linearDamping = 0.2f;
            heldplayerrb.angularDamping = 0.4f;

            // change ownership back to server
            var heldPlayerNetworkObject = heldPlayer.GetComponent<NetworkObject>();
            GetComponent<ServerRpcStuff>().ChangeToSpecificClientServerRpc(heldPlayerNetworkObject, clientId);

            heldPlayer = null;
        }
        // throw pick up object
        if (Input.GetKeyDown(KeyCode.E) && heldPlayer != null)
        {
            heldplayerrb = heldPlayer.GetComponent<Rigidbody>();

            // give gravity back
            heldplayerrb.useGravity = true;

            heldplayerrb.linearDamping = 0.2f;
            heldplayerrb.angularDamping = 0.4f;

            Vector3 throwDir = transCam.forward;
            float throwForce = 15f;
            heldplayerrb.AddForce(throwDir * throwForce, ForceMode.VelocityChange);

            // change ownership back to server
            var heldPlayerNetworkObject = heldPlayer.GetComponent<NetworkObject>();
            GetComponent<ServerRpcStuff>().ChangeToSpecificClientServerRpc(heldPlayerNetworkObject, clientId);

            heldPlayer = null;
        }
    }
}
