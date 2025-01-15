using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mud : MonoBehaviour
{
    [SerializeField] AudioClip mudLandingSE;
    [SerializeField] AudioClip mudRunningSE;
    PlayerAudioController.AudioClipName tmpRunningName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            other.GetComponent<PlayerAudioController>().PlayOneShot(mudLandingSE);
            other.GetComponent<PlayerController>().OnColliderMudEnter();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var compornent = other.GetComponent<PlayerAudioController>();
        if (compornent)
        {
            compornent.ResetRunningSourse(PlayerAudioController.AudioClipName.running_mud);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            other.GetComponent<PlayerController>().OnColliderMudExit();

            var audioCompornent = other.GetComponent<PlayerAudioController>();
            audioCompornent.ResetRunningSourse(audioCompornent.runningClipName);
        }
    }
}
