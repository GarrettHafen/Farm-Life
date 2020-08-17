using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sounds[] sounds;
    private AudioSource[] audioSources;

    void Awake()
    {
        foreach(Sounds s in sounds)
        {
           s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            
        }
        audioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
    }

    private void Start()
    {
        instance = this;
    }

    public void PlaySound(string name)
    {
        Sounds s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    public void Mute()
    {
        foreach(AudioSource s in audioSources)
        {
            s.mute = !s.mute;
        }
    }
}
