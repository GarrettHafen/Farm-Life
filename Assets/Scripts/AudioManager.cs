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
        audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        instance = this;
    }

    public void PlaySound(string name)
    {
        Sounds s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound not found: " + name);
            return;
        }
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
