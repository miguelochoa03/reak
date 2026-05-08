using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Tooltip("Which clip name to play when throwing. Defaults to 'Jack' — change to whichever clip looks like a throw.")]
    public string throwClipName = "Jack";
    [Tooltip("How long the throw animation locks out movement-driven animation changes.")]
    public float throwLockDuration = 0.45f;

    Animator anim;
    string current;
    float lockUntil;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayIdle() => Play("Idle");
    public void PlayJump() => Play("Jump");
    public void PlayFall() => Play("Fall");
    public void PlayWalk() => Play("Walk");
    public void PlayClimb() => Play("Climb");
    public void PlayTuck() => Play("Tuck");
    public void PlayJack() => Play("Jack");
    public void PlayNothing() { /* stay on whatever's playing */ }

    public void PlayThrow()
    {
        current = throwClipName;
        anim.CrossFade(throwClipName, 0.1f);
        lockUntil = Time.time + throwLockDuration;
    }

    void Play(string state)
    {
        if (Time.time < lockUntil) return;       // throw / one-shot is still playing
        if (current == state) return;
        current = state;
        anim.CrossFade(state, 0.1f);
    }
}
