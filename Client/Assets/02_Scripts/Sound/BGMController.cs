using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    AudioSource bgmSource;
    [SerializeField] bool isStartPlayBGM = true;

    void Start()
    {
        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;

        if (isStartPlayBGM)
        {
            if (FadeAudioController.Instance != null) FadeAudioController.Instance.PlayFadeAudio(this.gameObject, true);
            else bgmSource.Play();
        }
    }

    public void PlayAudio()
    {
        if (bgmSource.isPlaying) return;

        if (FadeAudioController.Instance != null) FadeAudioController.Instance.PlayFadeAudio(this.gameObject, true);
        else bgmSource.Play();
    }

    public void StopAudio()
    {
        if (FadeAudioController.Instance != null && bgmSource.isPlaying) FadeAudioController.Instance.PlayFadeAudio(this.gameObject, false);
    }
}
