using UnityEngine;
using System.Collections;

public class FallDamageDetector : MonoBehaviour
{
    public float fallDamageTime = 1f;
    public float airTime = 0f;
    public enum fallState : int
    { // Game states
        grounded,
        airborne,
        landing,
    }
    public fallState currentFallState; // Current game state
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (currentFallState)
        {
            case fallState.grounded:
                if (!gameObject.GetComponent<PlayerMovementCamera>().isGrounded)
                {
                    currentFallState = fallState.airborne;
                }
                break;
            case fallState.airborne:
                airTime += Time.deltaTime;
                if (gameObject.GetComponent<PlayerMovementCamera>().isGrounded)
                {
                    currentFallState = fallState.landing;
                }
                break;
            case fallState.landing:
                if (airTime > 3f)
                {
                    gameObject.GetComponent<PlayerHealth>().TakeDamage(Mathf.Round(airTime) * 2f);
                }
                currentFallState = fallState.grounded;
                airTime = 0f;
                break;
        }
    }
}
