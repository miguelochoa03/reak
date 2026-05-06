using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{

    [SerializeField] private SoundEffectBunch[] soundEffects;
    private Dictionary<string, List<AudioClip>> sounds;
    private void Awake()
    {
        sounds = new Dictionary<string, List<AudioClip>>();
        foreach (SoundEffectBunch bunch in soundEffects)
        {
            sounds[bunch.name] = bunch.audio;
        }
    }

    public AudioClip returnRandom(string name)
    {
        if (sounds.ContainsKey(name))
        {
            List<AudioClip> audio = sounds[name];
            if (audio.Count > 0)
            {
                return audio[UnityEngine.Random.Range(0, audio.Count)];
            }
        }
        return null;
    }
}

[System.Serializable]
public struct SoundEffectBunch
{
    public string name;
    public List<AudioClip> audio;
}
