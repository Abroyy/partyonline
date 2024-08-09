using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioClip soundClip; // Unity'de atayaca��n�z ses dosyas�
    public float maxVolume = 1.0f; // Maksimum ses �iddeti
    public float fadeInDuration = 5.0f; // Sesin maxVolume kadar y�kselmesi i�in ge�en s�re

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = soundClip;
        audioSource.volume = 0; // Ba�lang��ta sesi kapal� olarak ba�lat�yoruz
        audioSource.Play(); // Sesi �almaya ba�lat�yoruz
    }

    void Update()
    {
        if (audioSource.volume < maxVolume)
        {
            // Ses �iddetini zamanla art�rma
            audioSource.volume += Time.deltaTime / fadeInDuration * maxVolume;
        }
        else
        {
            audioSource.volume = maxVolume; // Maksimum ses �iddetini sabitler
        }
    }
}
