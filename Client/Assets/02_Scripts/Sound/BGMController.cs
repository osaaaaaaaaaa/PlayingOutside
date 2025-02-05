//*********************************************************
// BGMを管理するスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class BGMController : MonoBehaviour
{
    AudioSource bgmSource;
    [SerializeField] bool isStartPlayBGM = true;

    private void Awake()
    {
        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;
        SetupAudioVolume();

        if (isStartPlayBGM)
        {
            if (FadeAudioController.Instance != null) FadeAudioController.Instance.PlayFadeAudio(this.gameObject, true);
            else bgmSource.Play();
        }

    }

    public void SetupAudioVolume()
    {
        if(bgmSource) bgmSource.volume = AudioVolume.BgmVolume;
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
