//*********************************************************
// 効果音を管理するスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SEController : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    AudioSource sourse;

    private void Awake()
    {
        sourse = GetComponent<AudioSource>();
        if (sourse == null) sourse = this.gameObject.AddComponent<AudioSource>();
        SetupAudioVolume();
    }


    public void SetupAudioVolume()
    {
        if(sourse) sourse.volume = AudioVolume.SeVolume;
    }

    public void PlayAudio()
    {
        if (clip != null)
        {
            if(sourse == null)
            {
                sourse = GetComponent<AudioSource>();
                if (sourse == null) sourse = this.gameObject.AddComponent<AudioSource>();
                SetupAudioVolume();
            }
            sourse.PlayOneShot(clip);
        }
    }

    public void PlayAudio(AudioClip audioClip)
    {
        sourse.PlayOneShot(audioClip);
    }

    public void PlayAudio(AudioClip audioClip, bool isLoop)
    {
        sourse.loop = isLoop;
        sourse.PlayOneShot(audioClip);
    }

    public void StopAudio()
    {
        sourse.loop = false;
        sourse.Stop();
    }
}
