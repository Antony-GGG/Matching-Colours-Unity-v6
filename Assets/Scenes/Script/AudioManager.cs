using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound sound in sounds)
        {
            sound.audioSource = gameObject.AddComponent<AudioSource>();

            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
        }
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("VolumeOn") == 1)
        {
            foreach (Sound sound in sounds)
            {
                sound.audioSource.mute = false;
            }
        }
        else if (PlayerPrefs.GetInt("VolumeOn") == 0)
        {
            foreach (Sound sound in sounds)
            {
                sound.audioSource.mute = true;
            }
        }

        FindFirstObjectByType<AudioManager>().Play("BG");
    }

    public void Play(String name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound: "+ name +" not found!");
            return;
        }
        s.audioSource.Play();
    }

    public void Stop(String name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound: " + name + " not found!");
            return;
        }
        s.audioSource.Stop();
    }
}