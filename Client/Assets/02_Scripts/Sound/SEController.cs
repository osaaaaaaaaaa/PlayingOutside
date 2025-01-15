using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEController : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    AudioSource sourse;

    // Start is called before the first frame update
    void Start()
    {
        sourse = GetComponent<AudioSource>();
        if (sourse == null) sourse = this.gameObject.AddComponent<AudioSource>();
    }

    public void PlayAudio()
    {
        sourse.PlayOneShot(clip);
    }
}
