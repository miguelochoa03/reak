using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public float speed = 5f;

    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;

    Rigidbody rb;
    bool ragdolled;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (ragdolled) return;

        float x = (Input.GetKey(right) ? 1f : 0f) - (Input.GetKey(left) ? 1f : 0f);
        float z = (Input.GetKey(up)    ? 1f : 0f) - (Input.GetKey(down) ? 1f : 0f);

        Vector3 dir = new Vector3(x, 0f, z).normalized;
        rb.linearVelocity = new Vector3(dir.x * speed, rb.linearVelocity.y, dir.z * speed);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<BallController>() != null)
            ragdolled = true;
    }
}
