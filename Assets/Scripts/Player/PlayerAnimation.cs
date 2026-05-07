using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator anim;
    string current;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayIdle() => Play("Idle");
    public void PlayJump() => Play("Jump");
    public void PlayFall() => Play("Fall");
    public void PlayWalk() => Play("Walk");
    public void PlayClimb() => Play("Climb");
    public void PlayJack() => Play("Jack");

    void Play(string state)
    {
        if (current == state) return;
        current = state;
        anim.CrossFade(state, 0.1f);
    }
}
