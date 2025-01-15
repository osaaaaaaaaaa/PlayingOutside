using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    AudioSource bgmSource;

    void Start()
    {
        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;
        if(FadeAudioController.Instance != null) FadeAudioController.Instance.PlayFadeAudio(this.gameObject, true);
        else bgmSource.Play();
    }

    public void StopAudio()
    {
        if (FadeAudioController.Instance != null) FadeAudioController.Instance.PlayFadeAudio(this.gameObject, false);
    }
}
