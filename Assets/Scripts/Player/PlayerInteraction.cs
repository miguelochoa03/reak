using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;


public class PlayerInteraction : NetworkBehaviour
{
    public Transform LookPoint;
    public Transform Target;

    Rigidbody heldobjectrb;
    Pickupable heldObject;

    float rayDistance = 3f;
    public RaycastHit hit;
    public bool ray;

    public CinemachineCamera cam;
    Transform transCam;

    float throwForce = 15f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        cam = GetComponent<PlayerMovementCamera>().cam;

        transCam = Camera.main.transform;

    }

    void Update()
    {
        if (!IsOwner) return;
        if (transCam == null) transCam = Camera.main != null ? Camera.main.transform : null;
        if (transCam == null) return;

        // start and end pos for raycast
        Vector3 origin = LookPoint.position;
        Vector3 direction = transCam.forward;
        Vector3 targetPos = origin + direction * rayDistance;

        // make ray
        //ray = Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance);
        ray = Physics.Raycast(origin, direction, out hit, rayDistance);
        if (Target != null) Target.position = targetPos;
        Debug.DrawRay(origin, direction * rayDistance, Color.red); // see visually in scene view (not game view)

        // Debug.Log(heldObject);

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
                }
            }
        }

        // move pick up object
        if (Input.GetMouseButton(0) && heldObject != null)
        {
            // move the object to the end of the raycast

            heldobjectrb = heldObject.GetComponent<Rigidbody>();

            // avoid gravity building up
            heldobjectrb.useGravity = false;

            heldobjectrb.linearDamping = 16f;
            heldobjectrb.angularDamping = 1f;

            Vector3 toTarget = targetPos - heldObject.transform.position;
            heldobjectrb.AddForce(toTarget * 0.05f);
        }

        // drop pick up object
        if (Input.GetMouseButtonUp(0) && heldObject != null)
        {
            heldobjectrb = heldObject.GetComponent<Rigidbody>();

            // give gravity back
            heldobjectrb.useGravity = true;

            heldobjectrb.linearDamping = 0.2f;
            heldobjectrb.angularDamping = 0.4f;

            // change ownership back to server
            var heldObjectNetworkObject = heldObject.GetComponent<NetworkObject>();
            GetComponent<ServerRpcStuff>().ChangeToServerServerRpc(heldObjectNetworkObject);

            heldObject = null;
        }

        // throw pick up object
        if (Input.GetKeyDown(KeyCode.E) && heldObject != null)
        {
            heldobjectrb = heldObject.GetComponent<Rigidbody>();

            // give gravity back
            heldobjectrb.useGravity = true;

            heldobjectrb.linearDamping = 0.2f;
            heldobjectrb.angularDamping = 0.4f;

            Vector3 throwDir = transCam.forward;
            heldobjectrb.AddForce(throwDir * throwForce, ForceMode.VelocityChange);

            // change ownership back to server
            var heldObjectNetworkObject = heldObject.GetComponent<NetworkObject>();
            GetComponent<ServerRpcStuff>().ChangeToServerServerRpc(heldObjectNetworkObject);

            heldObject = null;
        }
    }
}
