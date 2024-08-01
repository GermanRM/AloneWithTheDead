using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSounds : MonoBehaviour

{
    private float seconds;
    private bool isPlaying=false;
    public AudioSource audioSource;
    public AudioClip audioGroan;
    [SerializeField] private ZombieCombat combat;

    private void Update() {
    if (!isPlaying && combat.isAlive)
    {
        StartCoroutine(SoundDelay());
    }
    
}

private IEnumerator SoundDelay() {
    isPlaying=true;
    seconds = Random.Range(3.5f,6.0f);
    yield return new WaitForSeconds(seconds);
    audioSource.PlayOneShot(audioGroan);
    isPlaying=false;
}
}
