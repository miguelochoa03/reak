using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerInteraction : NetworkBehaviour
{
    public Transform LookPoint;
    public float rayDistance = 7f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // 

    }

    void Update()
    {
        if (!IsOwner) return;

        // camera transform
        Transform cam = Camera.main.transform;

        // start and end pos for raycast
        Vector3 origin = LookPoint.position;
        Vector3 direction = cam.forward;

        // check if ray cast hit anything
        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance))
        {
            Debug.DrawRay(origin, direction * rayDistance, Color.red); // see visually in scene view (not game view)

            bool IsPickupAble = hit.collider.TryGetComponent<Pickupable>(out var pickup);

            if (IsPickupAble)
            {
                Debug.Log("This can be picked up, name: " + hit.collider.name);
            } else
            {
                Debug.Log("NOT pickupable.");
            }
        }
        else
        {
            Debug.DrawRay(origin, direction * rayDistance, Color.yellow); // see visually in scene view (not game view)
        }
    }
}
