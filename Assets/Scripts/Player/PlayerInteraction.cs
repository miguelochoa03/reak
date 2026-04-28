using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;


public class PlayerInteraction : NetworkBehaviour
{
    public Transform LookPoint;
    Pickupable heldObject;

    float rayDistance = 3f;
    bool ray;

    public CinemachineCamera cam;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        cam = GetComponent<PlayerMovementCamera>().cam;
    }

    void Update()
    {
        if (!IsOwner) return;

        // camera transform
        Transform TransformCam = cam.transform;

        // start and end pos for raycast
        Vector3 origin = LookPoint.position;
        Vector3 direction = TransformCam.forward;

        Vector3 targetPos = origin + direction * rayDistance;

        // make ray
        ray = Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance);
        Debug.DrawRay(origin, direction * rayDistance, Color.red); // see visually in scene view (not game view)

        // check ray cast hit anything
        if (ray)
        {
            // check if pickupable
            bool IsPickupAble = hit.collider.TryGetComponent<Pickupable>(out var pickup);
            if (IsPickupAble)
            {
                Debug.Log("This can be picked up, name: " + hit.collider.name);

                // pick up object
                if (Input.GetMouseButtonDown(0))
                {
                    heldObject = pickup;

                    // change ownership to client messing with the heldObject
                    var heldObjectNetworkObject = heldObject.GetComponent<NetworkObject>();
                    GetComponent<ServerRpcStuff>().ChangeToClientServerRpc(heldObjectNetworkObject);

                    // avoid stuttering while picked up
                    heldObject.GetComponent<Rigidbody>().isKinematic = true;
                }
            }

            // move pick up object
            if (heldObject != null && Input.GetMouseButton(0))
            {
                // move the object to the end of the raycast
                heldObject.transform.position = origin + direction * rayDistance;
            }

            // move pick up object
            //    if (heldObject != null && Input.GetMouseButton(0))
            //    {
            //        // prevent wall clipping
            //        if (Physics.Raycast(origin, direction, out RaycastHit holdhit, rayDistance))
            //        {
            //            // heldObject is hitting something else
            //            if (holdhit.collider.gameObject != heldObject.gameObject)
            //            {
            //                // drop the object
            //                //heldObject = null;

            //                if (heldObject == null) return;

            //                var netchanger = GetComponent<NetworkOwnershipChanger>();
            //                var netObj = heldObject.GetComponent<NetworkObject>();
            //                bool isPlayer = heldObject.GetComponent<PlayerMovementCamera>() != null;

            //                // tell server it's no longer held
            //                netchanger.SetIsBeingHeldServerRpc(netObj, false);

            //                // restore physics
            //                heldObject.GetComponent<Rigidbody>().isKinematic = false;

            //                // return ownership
            //                if (isPlayer)
            //                    netchanger.ChangeToSpecificClientServerRpc(netObj, originalOwner);
            //                else
            //                    netchanger.ChangeToServerServerRpc(netObj);

            //                heldObject = null;



            //            }
            //            else
            //            {
            //                // move the object to the end of the raycast
            //                heldObject.transform.position = origin + direction * rayDistance;


            //            }
            //        }
            //        else
            //        {
            //            // move the object to the end of the raycast
            //            heldObject.transform.position = origin + direction * rayDistance;
            //    }
            //}

            // drop pick up object
            //if (Input.GetMouseButtonUp(0) && heldObject != null)
            //{
            //    NetworkOwnershipChanger netchanger = GetComponent<NetworkOwnershipChanger>();
            //    var heldObjectNetworkObject = heldObject.GetComponent<NetworkObject>();
            //    bool isPlayer = heldObject.GetComponent<PlayerMovementCamera>() != null;

            //    //heldObject.GetComponent<Pickupable>().isBeingHeld = false;
            //    netchanger.SetIsBeingHeldServerRpc(heldObjectNetworkObject, false);
            //    heldObject.GetComponent<Rigidbody>().isKinematic = false;

            //    if (isPlayer)
            //    {
            //        // give player control back
            //        netchanger.ChangeToSpecificClientServerRpc(heldObjectNetworkObject, originalOwner);
            //    }
            //    else
            //    {
            //        netchanger.ChangeToServerServerRpc(heldObjectNetworkObject);
            //    }

            //    heldObject = null;
            //}

            // throw pick up object
            //if (heldObject != null && Input.GetKeyDown(KeyCode.E))
            //{
            //    NetworkOwnershipChanger netchanger = GetComponent<NetworkOwnershipChanger>();
            //    var heldObjectNetworkObject = heldObject.GetComponent<NetworkObject>();
            //    bool isPlayer = heldObject.GetComponent<PlayerMovementCamera>() != null;

            //    //heldObject.GetComponent<Pickupable>().isBeingHeld = false;
            //    netchanger.SetIsBeingHeldServerRpc(heldObjectNetworkObject, false);

            //    Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            //    rb.isKinematic = false;

            //    Vector3 throwDir = TransformCam.forward;
            //    float throwForce = 50f;
            //    rb.AddForce(throwDir * throwForce, ForceMode.VelocityChange);

            //    if (isPlayer)
            //    {
            //        // give player control back
            //        netchanger.ChangeToSpecificClientServerRpc(heldObjectNetworkObject, originalOwner);
            //    }
            //    else
            //    {
            //        netchanger.ChangeToServerServerRpc(heldObjectNetworkObject);
            //    }

            //    heldObject = null;
        }
    }
}
