using Unity.Netcode;
using UnityEngine;

public class PlayerClimb : NetworkBehaviour
{
    Rigidbody rb;
    Climbable climbableObject;

    PlayerInteraction interaction;
    bool interactionray;
    RaycastHit interactionhit;

    bool isClimbing = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        rb = GetComponent<Rigidbody>();

        interaction = GetComponent<PlayerInteraction>();
        
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector3 origin = transform.position + transform.up * 2f;
        Vector3 direction = transform.forward;
        float rayDistance = 1.7f;

        // will be used to attach yourself to the wall or whatever
        bool climbray = Physics.Raycast(origin, direction, out RaycastHit climbhit, rayDistance);
        Debug.DrawRay(origin, direction * rayDistance, Color.red); // see visually in scene view (not game view)

        // read where you're facing
        interactionray = interaction.ray;
        interactionhit = interaction.hit;

        if (interactionray)
        {
            // check if climbable object
            bool IsClimbable = interactionhit.collider.TryGetComponent<Climbable>(out var climb);
            if (IsClimbable)
            {
                Debug.Log("This can be climbed, name: " + interactionhit.collider.name);

                // climbable object
                if (Input.GetMouseButtonDown(0))
                {
                    climbableObject = climb;
                }
            }
        }

        // hold click to stay climbing
        if (Input.GetMouseButton(0) && climbableObject != null)
        {

        }

        // let go of click to stop climbing
        if (Input.GetMouseButtonUp(0) && climbableObject != null)
        {

        }
    }
}
