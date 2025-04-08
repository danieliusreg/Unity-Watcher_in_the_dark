using UnityEngine;
using System.Collections;

public class Key : MonoBehaviour
{
    private Vector3 keyPosition;

    public AudioClip pickupSound; // <- Garso failas
    private AudioSource audioSource;

    private bool isPickedUp = false;

    void Start()
    {
        keyPosition = transform.position;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        if (isPickedUp) return; // Kad nesuveiktų kelis kartus
        isPickedUp = true;

        Debug.Log("🔑 Raktų dalis paimta: " + gameObject.name);

        // Paleidžiam garsą
        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);

        // Pranešam GraveDigging
        GraveDigging graveDigging = Object.FindFirstObjectByType<GraveDigging>();
        if (graveDigging != null)
            graveDigging.CollectKey(keyPosition);

        // Paleidžiam patrol'ą
        RandomPatrol patrol = Object.FindFirstObjectByType<RandomPatrol>();
        if (patrol != null)
            patrol.StartTimedChase();

        // Palaukiam, kol garsas nuskambės, tada sunaikinam objektą
        StartCoroutine(DestroyAfterSound());
    }

    IEnumerator DestroyAfterSound()
    {
        if (pickupSound != null)
            yield return new WaitForSeconds(pickupSound.length);
        Destroy(gameObject);
    }
}
