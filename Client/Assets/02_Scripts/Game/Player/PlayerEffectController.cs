//*********************************************************
// キャラクターのアエフェクト(パーティクル)を管理するスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerEffectController : MonoBehaviour
{
    [SerializeField] List<GameObject> effectPrefabs;
    PlayerController playerController;

    public enum EFFECT_ID
    {
        Run = 0,        // [アニメーションからの呼び出し] 走る
        Landing,        // [アニメーションからの呼び出し] 着地
        AreaCleared,    // ゴールしたとき
        MudSplash,      // 泥に着水したとき
        MudRipples,     // 泥の中にいるとき
        Down,           // [アニメーションからの呼び出し] ノックダウンするダメージを受けたとき
        KnockBackSmoke, // [アニメーションからの呼び出し] ノックバック中
        PepperFire,     // [アイテム] 唐辛子
        AuraBuff,       // スピードアップ
        AuraDebuff,     // スピードダウン
    }

    public List<GameObject> particleSystems = new List<GameObject>();
    bool isTouchedMud;
    Coroutine debuffCoroutine;

    private void Awake()
    {
        isTouchedMud = false;
        playerController = GetComponent<PlayerController>();
        particleSystems = new List<GameObject>();
    }

    private void OnEnable()
    {
        isTouchedMud = false;
        ClearAllParticles();
    }

    public void SetEffect(EFFECT_ID efectId)
    {
        GameObject effect = null;
        GameObject effectTmp;
        switch (efectId)
        {
            case EFFECT_ID.Run:
                if (!isTouchedMud)
                    Instantiate(effectPrefabs[(int)efectId], this.transform.position + Vector3.up * 0.1f, Quaternion.identity);
                break;
            case EFFECT_ID.Landing:
                if (!isTouchedMud)
                    Instantiate(effectPrefabs[(int)efectId], this.transform.position, Quaternion.identity);
                break;
            case EFFECT_ID.AreaCleared:
                Instantiate(effectPrefabs[(int)efectId], this.transform.position + Vector3.up, Quaternion.identity);
                break;
            case EFFECT_ID.MudSplash:
                Instantiate(effectPrefabs[(int)efectId], this.transform.position + Vector3.up / 2, Quaternion.identity);
                break;
            case EFFECT_ID.MudRipples:
                if (!SerchSameNameParticle(EFFECT_ID.MudRipples.ToString()))
                {
                    effect = Instantiate(effectPrefabs[(int)efectId], this.transform);
                    effect.name = EFFECT_ID.MudRipples.ToString();
                }
                break;
            case EFFECT_ID.Down:
                Instantiate(effectPrefabs[(int)efectId], this.transform.position + Vector3.up, Quaternion.identity);
                break;
            case EFFECT_ID.KnockBackSmoke:
                effectTmp = Instantiate(effectPrefabs[(int)efectId]);
                effectTmp.transform.position += this.transform.position;
                break;
            case EFFECT_ID.PepperFire:
                if (!SerchSameNameParticle(EFFECT_ID.PepperFire.ToString()))
                {
                    effect = Instantiate(effectPrefabs[(int)efectId], this.transform);
                    effect.name = EFFECT_ID.PepperFire.ToString();
                }
                break;
            case EFFECT_ID.AuraBuff:
                if (!SerchSameNameParticle(EFFECT_ID.AuraBuff.ToString()))
                {
                    effect = Instantiate(effectPrefabs[(int)efectId], this.transform);
                    effect.name = EFFECT_ID.AuraBuff.ToString();
                }
                break;
            case EFFECT_ID.AuraDebuff:
                if (!SerchSameNameParticle(EFFECT_ID.AuraDebuff.ToString()))
                {
                    effect = Instantiate(effectPrefabs[(int)efectId], this.transform);
                    effect.name = EFFECT_ID.AuraDebuff.ToString();
                }
                break;
        }

        if (effect != null) particleSystems.Add(effect);
    }

    bool SerchSameNameParticle(string name)
    {
        foreach (GameObject particleSystem in particleSystems)
        {
            if (particleSystem != null && particleSystem.name == name) return true;
        }
        return false;
    }

    public void ClearAllParticles()
    {
        foreach (GameObject particleSystem in particleSystems)
        {
            if (particleSystem) Destroy(particleSystem);
        }
        particleSystems.Clear();

        if (debuffCoroutine != null)
        {
            StopCoroutine(debuffCoroutine);
            debuffCoroutine = null;
        }
    }

    public void ClearParticle(EFFECT_ID efectId)
    {
        string particleName = efectId.ToString();
        GameObject particleToRemove = null;
        foreach (GameObject particleSystem in particleSystems)
        {
            if (particleSystem && particleSystem.name == particleName)
            {
                particleToRemove = particleSystem;
                particleSystem.GetComponent<ParticleSystem>().Stop();
            }
        }

        if (particleToRemove != null)
        {
            particleSystems.Remove(particleToRemove);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 泥の場合
        if (other.tag == "Mud")
        {
            isTouchedMud = true;
            SetEffect(EFFECT_ID.MudSplash);
            SetEffect(EFFECT_ID.MudRipples);
        }
        // デバフ
        if (other.GetComponent<DebuffCollider>() != null)
        {
            if (other.GetComponent<DebuffCollider>().IsSpeedDown)
            {
                SetEffect(EFFECT_ID.AuraDebuff);
                if (debuffCoroutine != null)
                {
                    StopCoroutine(debuffCoroutine);
                }
                debuffCoroutine = StartCoroutine(AuraDebufCoroutine(other.GetComponent<DebuffCollider>().DebuffTime));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 泥の場合
        if (other.tag == "Mud")
        {
            isTouchedMud = false;
            ClearParticle(EFFECT_ID.MudRipples);
        }
    }

    IEnumerator AuraDebufCoroutine(float effectTime)
    {
        if (playerController.enabled)
        {
            playerController.Speed -= playerController.DefaultSpeed;
            playerController.DefaultSpeed = 3f;
            playerController.Speed += playerController.DefaultSpeed;
        }

        while (effectTime > 0)
        {
            effectTime--;
            yield return new WaitForSeconds(1f);
        }

        if (playerController.enabled)
        {
            playerController.Speed -= playerController.DefaultSpeed;
            playerController.DefaultSpeed = 5f;
            playerController.Speed += playerController.DefaultSpeed;
        }
        ClearParticle(EFFECT_ID.AuraDebuff);
        debuffCoroutine = null;
    }
}
