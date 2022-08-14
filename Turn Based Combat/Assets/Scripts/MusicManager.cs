using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource introMusicSource;
    [SerializeField] AudioSource mainMusicSource;
    bool mainMusicPlaying;

    private void Update()
    {
        if (!introMusicSource.isPlaying && !mainMusicPlaying)
        {
            mainMusicSource.Play();
            mainMusicPlaying = true;
        }
    }
}
