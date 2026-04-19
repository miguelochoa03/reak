using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerInteraction : NetworkBehaviour
{
    public Transform LookPoint;
    Pickupable heldObject;

    public float rayDistance = 7f;

    void Update()
    {
        if (!IsOwner) return;

        // camera transform
        Transform cam = Camera.main.transform;

        // start and end pos for raycast
        Vector3 origin = LookPoint.position;
        Vector3 direction = cam.forward;

        // check ray cast hit anything
        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance))
        {
            Debug.DrawRay(origin, direction * rayDistance, Color.red); // see visually in scene view (not game view)

            // check if pickupable
            bool IsPickupAble = hit.collider.TryGetComponent<Pickupable>(out var pickup);
            if (IsPickupAble)
            {
                Debug.Log("This can be picked up, name: " + hit.collider.name);

                // pick up object
                if (Input.GetMouseButtonDown(0))
                {
                    heldObject = pickup;
                }
            }
            else
            {
                Debug.Log("NOT pickupable.");
            }
        }
        else
        {
            Debug.DrawRay(origin, direction * rayDistance, Color.yellow); // see visually in scene view (not game view)
        }

        // move pick up object
        if (heldObject != null && Input.GetMouseButton(0))
        {
            // prevent wall clipping
            if (Physics.Raycast(origin, direction, out RaycastHit holdhit, rayDistance))
            {
                // heldObject is hitting something else
                if (holdhit.collider.gameObject != heldObject.gameObject)
                {
                    // stop at the wall
                    //heldObject.transform.position = holdhit.point;

                    // drop the object
                    heldObject = null;
                }
                else
                {
                    heldObject.transform.position = origin + direction * rayDistance;
                }
            }
            else
            {
                heldObject.transform.position = origin + direction * rayDistance;
            }
        }

        // drop pick up object
        if (Input.GetMouseButtonUp(0))
        {
            heldObject = null;
        }
    }
}
