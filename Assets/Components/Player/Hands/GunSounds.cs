using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This component is isolated because we want gun sounds both in player hands
// and in player characters
public class GunSounds : MonoBehaviour {
    [SerializeField]
    private float gunshotVolume = 0.5f;

    [SerializeField]
    private AudioClip gunshotClip;

    [SerializeField]
    private float reloadVolume = 0.4f;

    [SerializeField]
    private AudioClip reloadClip;

    private AudioSource audioSource;

    public void Awake() {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null) {
            throw new System.Exception("Missing AudioSource component.");
        }
    }

    public void Shoot() {
        audioSource.PlayOneShot(gunshotClip, gunshotVolume);
    }

    public void Reload() {
        audioSource.PlayOneShot(reloadClip, reloadVolume);
    }
}
