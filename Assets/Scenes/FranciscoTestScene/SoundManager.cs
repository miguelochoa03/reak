using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    private static SoundManager Instance;
    private static AudioSource source;
    private static SoundEffects sounds;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            sounds = GetComponent<SoundEffects>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void play(string name)
    {
        AudioClip audio = sounds.returnRandom(name);
        if (audio != null)
        {
            source.PlayOneShot(audio);
        }
    }
}
