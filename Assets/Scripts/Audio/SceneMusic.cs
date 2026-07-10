using System;
using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip music;

    private void Start()
    {
        AudioManager.Instance?.PlayMusic(music);
    }
}
