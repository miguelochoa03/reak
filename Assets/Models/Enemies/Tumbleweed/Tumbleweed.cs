using UnityEngine;

public class Tumbleweed : MonoBehaviour
{
    public Rigidbody rigidBody;
    public GameObject[] playerList;
    public GameObject targetPlayer; // Targeted player
    public enum gameState : int
    { // Game states
        waiting,
        force,
        finished,
    }
    public gameState currentState; // Current game state
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = gameState.waiting;
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        switch(currentState)
        {
            case gameState.waiting:
                playerList = GameObject.FindGameObjectsWithTag("Player");
                if (playerList.Length > 0)
                {
                    for (int i = 0; i < playerList.Length; i++)
                    {
                        if (Vector3.Distance(transform.position, playerList[i].transform.position) < 25.0f)
                        {
                            targetPlayer = playerList[i];
                            currentState = gameState.force;
                        }
                    }
                }
                break;
            case gameState.force:
                Vector3 distance = new Vector3((targetPlayer.transform.position.x - transform.position.x), 0, (targetPlayer.transform.position.z - transform.position.z));
                Vector3 localVelocity = Vector3.ClampMagnitude(distance, 1) * 8;
                rigidBody.linearVelocity = transform.TransformDirection(localVelocity);
                currentState = gameState.finished;
                break;
            case gameState.finished:
                break;
        }
    }
}
