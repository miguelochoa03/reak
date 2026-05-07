using Unity.Netcode;
using UnityEngine;

public class HandleRagdoll : NetworkBehaviour
{
    [SerializeField] private GameObject _ragdoll;
    [SerializeField] private GameObject _animatedModel;

    private bool _dead;

    Rigidbody rb;

    public void TurnOn()
    {
        _dead = true;
        ToggleDead();
    }

    public void TurnOff()
    {
        _dead = false;
        ToggleDead();
    }

    public override void OnNetworkSpawn()
    {
        // Visual setup must run on every client (owner and non-owners), not just the owner.
        // Otherwise remote clients see the wrong visual state (or nothing at all).
        _ragdoll.gameObject.SetActive(false);
        _animatedModel.gameObject.SetActive(true);

        var animated = transform.Find("animatedreakcharacterrigged");
        if (animated != null) rb = animated.GetComponent<Rigidbody>();
    }

    //private void Awake()
    //{
    //    _ragdoll.gameObject.SetActive(false);
    //    rb = transform.Find("animatedreakcharacterrigged").GetComponent<Rigidbody>();
    //}

    // test
    //private void Update()
    //{
    //    if (Input.GetButtonDown("Fire1"))
    //    {
    //        TurnOn();
    //        //ToggleDead();
    //    }
    //    //ToggleDead();
    //}
    private void ToggleDead()
    {
        if (!IsOwner) return;

        //_dead = !_dead;

        if (_dead)
        {
            CopyTransformData(_animatedModel.transform, _ragdoll.transform, rb.linearVelocity);
            _ragdoll.gameObject.SetActive(true);
            _animatedModel.gameObject.SetActive(false);
        }
        else
        {
            // switch back to model and disable ragdoll
            _ragdoll.gameObject.SetActive(false);
            _animatedModel.gameObject.SetActive(true);
        }
    }

    private void CopyTransformData(Transform sourceTransform, Transform destinationTransform, Vector3 velocity)
    {
        if (!IsOwner) return;

        if (sourceTransform.childCount != destinationTransform.childCount)
        {
            Debug.LogWarning("Invalid transform copy, they need to match transform hierarchies");
            return;
        }

        for (int i = 0; i < sourceTransform.childCount; i++)
        {
            var source = sourceTransform.GetChild(i);
            var destination = destinationTransform.GetChild(i);
            destination.position = source.position;
            destination.rotation = source.rotation;
            var rb = destination.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = velocity;

            CopyTransformData(source, destination, velocity);
        }
    }
}
