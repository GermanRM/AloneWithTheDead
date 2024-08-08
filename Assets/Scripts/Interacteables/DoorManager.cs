using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{    
    [Header("Door Properties")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider doorCollider;
    public bool isOpened;

    [Header("SFX Properties")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openDoorSound;
    [SerializeField] private AudioClip closeDoorSound;

    public void OpenDoor()
    {
        if (!AnimatorIsPlaying("CloseDoor") && !isOpened)
        {
            Debug.Log("Door opened");
            animator.SetBool("IsOpened", isOpened);
            animator.SetTrigger("OpenDoor");
            audioSource.PlayOneShot(openDoorSound);
            isOpened = true;
        }
    }

    public void CloseDoor()
    {
        if (!AnimatorIsPlaying("OpenDoor") && isOpened)
        {
            Debug.Log("Door closed");
            animator.SetBool("IsOpened", isOpened);
            animator.SetTrigger("CloseDoor");
            audioSource.PlayOneShot(closeDoorSound);
            isOpened = false;
        }           
    }

    private void Update()
    {
        //No pude hacer que esto se ejecutara fuera de update :/, estaría bueno que se hiciera
        //fuera de update para ahorrar rendimiento
        if (AnimatorIsPlaying())
            doorCollider.enabled = false;
        else doorCollider.enabled = true;
    }

    /// <summary>
    /// Check if animator is playing an anim
    /// </summary>
    /// <returns></returns>
    bool AnimatorIsPlaying()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length >
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    /// <summary>
    /// Check if animator is playing an specific anim
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    bool AnimatorIsPlaying(string stateName)
    {
        return AnimatorIsPlaying() && animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
}
