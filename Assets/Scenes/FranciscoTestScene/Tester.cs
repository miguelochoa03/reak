using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public float timerSplat = 4.0f;
    public float timerCrack = 5.0f;
    public float timerWalk = 6.0f;
    public float timerJump = 7.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timerSplat -= Time.deltaTime;
        timerCrack -= Time.deltaTime;
        timerWalk -= Time.deltaTime;
        timerJump -= Time.deltaTime;
        if (timerSplat < 0)
        {
            timerSplat = 4.0f;
            SoundManager.play("Splat");
        }
        if (timerCrack < 0)
        {
            timerCrack = 4.0f;
            SoundManager.play("Crack");
        }
        if (timerWalk < 0)
        {
            timerWalk = 4.0f;
            SoundManager.play("Walk");
        }
        if (timerJump < 0)
        {
            timerJump = 4.0f;
            SoundManager.play("Jump");
        }
    }
}
