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
    public void PlayWalk() => Play("Walk");
    public void PlayClimb() => Play("Climb");
    public void PlayJack() => Play("Jack");
    public void PlayNothing() => Play("Nothing");

    void Play(string state)
    {
        if (current == state) return;
        current = state;
        anim.CrossFade(state, 0.5f);
    }
}
