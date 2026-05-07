using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestingExplosion : MonoBehaviour
{
    [SerializeField] private Material bodyColor;
    [SerializeField] private Material brightRed;
    public Renderer[] renderers = new Renderer[3];
    public float deathTimer = 3.0f;
    public bool alive = true;

    void Start()
    {
        renderers[0].material = bodyColor;
        renderers[1].material = bodyColor;
        renderers[2].material = bodyColor;
    }

    void Update()
    {
        deathTimer -= Time.deltaTime;
        if (deathTimer < 0 && alive)
        {
            alive = false;
            renderers[0].material = brightRed;
            renderers[1].material = brightRed;
            renderers[2].material = brightRed;
        }
    }
}
