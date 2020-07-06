using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundLibrary : MonoBehaviour
{
    public static SoundLibrary instance;
    public AudioClip coinPickUp, keyPickUp, attackHit, walk, doorLocked, doorOpen;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}
