using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [SerializeField] AudioSource oneShotSourse;
    [SerializeField] AudioSource runningSourse;
    [SerializeField] AudioSource loopSkillSourse;
    [SerializeField] List<AudioClip> audioClips;
    [SerializeField] float customPitch;
    Dictionary<AudioClipName, AudioClip> audioClipList;
    public AudioClipName runningClipName { get; private set; } = AudioClipName.running_default;

    public enum AudioClipName
    {
        piyo = 0,
        running_default,
        running_grass,
        running_mud,
        kick,
        damage01,
        damage02,
        damage03,
        knockdown,
        skill1_hurricane,
        skill2_screw,
        skill3_machaura,
        skill4_rollkick,
        skill5_rising,
        skill5_stamp,
        item_get,
        item_pepper,
    }

    private void Awake()
    {
        audioClipList = new Dictionary<AudioClipName, AudioClip>();
        foreach (var clip in audioClips)
        {
            AudioClipName clipName = (AudioClipName)Enum.Parse(typeof(AudioClipName),clip.name);
            audioClipList.Add(clipName, clip);
        }
        runningSourse.clip = audioClipList[AudioClipName.running_default];
    }

    private void OnEnable()
    {
        StopRunningSourse();
        StopLoopSkillSourse();
    }

    void SetupAudioVolume()
    {
        oneShotSourse.volume = AudioVolume.SeVolume;
        runningSourse.volume = AudioVolume.SeVolume;
        loopSkillSourse.volume = AudioVolume.SeVolume;
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (clip == audioClipList[AudioClipName.piyo]) oneShotSourse.pitch = customPitch;
        else oneShotSourse.pitch = 1;
        oneShotSourse.PlayOneShot(clip);
    }

    public void PlayOneShot(AudioClipName name)
    {
        PlayOneShot(audioClipList[name]);
    }

    public void PlayDamageClip()
    {
        int id = UnityEngine.Random.Range((int)AudioClipName.damage01, (int)AudioClipName.damage03 + 1);
        switch (id)
        {
            case (int)AudioClipName.damage01:
                PlayOneShot(AudioClipName.damage01);
                break;
            case (int)AudioClipName.damage02:
                PlayOneShot(AudioClipName.damage02);
                break;
            case (int)AudioClipName.damage03:
                PlayOneShot(AudioClipName.damage03);
                break;
        }
    }

    public void ResetRunningSourse(AudioClipName name)
    {
        if (audioClipList[name] != runningSourse.clip)
        {
            runningSourse.Stop();
            runningSourse.clip = audioClipList[name];
            if (name != AudioClipName.running_mud) runningClipName = name;
        }
    }

    public void PlayRunningSourse()
    {
        if (runningSourse.isPlaying) return;
        runningSourse.loop = true;
        runningSourse.Play();
    }

    public void StopRunningSourse()
    {
        runningSourse.Stop();
    }

    public void ResetLoopSkillSourse(AudioClipName clipName)
    {
        loopSkillSourse.Stop();
        if (clipName == AudioClipName.kick) loopSkillSourse.pitch = 2f;
        else loopSkillSourse.pitch = 1f;
        loopSkillSourse.clip = audioClipList[clipName];
        loopSkillSourse.Play();
    }

    public void StopLoopSkillSourse()
    {
        loopSkillSourse.Stop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 6 && runningSourse.clip != audioClipList[AudioClipName.running_mud])
        {
            switch (collision.gameObject.tag)
            {
                case "Grass":
                    ResetRunningSourse(AudioClipName.running_grass);
                    break;
                default:
                    ResetRunningSourse(AudioClipName.running_default);
                    break;
            }
        }
    }
}
