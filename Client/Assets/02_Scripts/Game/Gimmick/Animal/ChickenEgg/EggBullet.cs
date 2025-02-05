//*********************************************************
// 鶏が発射する卵のスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EggBullet : MonoBehaviour
{
    [SerializeField] GameObject explosionParticle;

    public void DoExplosion()
    {
        Instantiate(explosionParticle, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    void PlayAudio()
    {
        GetComponent<AudioSource>().Play();
    }

    private void OnEnable()
    {
        Invoke("PlayAudio", 1.5f);
    }
}
