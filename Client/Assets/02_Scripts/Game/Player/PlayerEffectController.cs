using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerEffectController : MonoBehaviour
{
    [SerializeField] List<GameObject> effectPrefabs;

    public enum EFFECT_ID
    {
        Run = 0,        // [�A�j���[�V��������̌Ăяo��] ����
        Landing,        // [�A�j���[�V��������̌Ăяo��] ���n
        AreaCleared,    // �S�[�������Ƃ�
        MudSplash,      // �D�ɒ��������Ƃ�
        MudRipples,     // �D�̒��ɂ���Ƃ�
        Down,           // [�A�j���[�V��������̌Ăяo��] �m�b�N�_�E������_���[�W���󂯂��Ƃ�
        KnockBackSmoke, // [�A�j���[�V��������̌Ăяo��] �m�b�N�o�b�N��
        PepperFire,   // [�A�C�e��] ���h�q��H�ׂ��Ƃ�
    }

    List<GameObject> particleSystems;
    bool isTouchedMud;

    private void Awake()
    {
        isTouchedMud = false;
        particleSystems = new List<GameObject>();
    }

    private void OnEnable()
    {
        isTouchedMud = false;
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
                if(!SerchSameNameParticle(EFFECT_ID.PepperFire.ToString()))
                effect = Instantiate(effectPrefabs[(int)efectId], this.transform);
                effect.name = EFFECT_ID.PepperFire.ToString();
                break;
        }

        if (effect != null) particleSystems.Add(effect);
    }

    bool SerchSameNameParticle(string name)
    {
        foreach (GameObject particleSystem in particleSystems)
        {
            if(particleSystem != null && particleSystem.name == name) return true;
        }
        return false;
    }

    public void StopAllParticles()
    {
        foreach(GameObject particleSystem in particleSystems)
        {
            if(particleSystem != null) particleSystem.GetComponent<ParticleSystem>().Stop();
        }

        particleSystems.Clear();
    }

    public void StopParticle(string name)
    {
        foreach (GameObject particleSystem in particleSystems)
        {
            if (particleSystem != null && particleSystem.name == name) particleSystem.GetComponent<ParticleSystem>().Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // �D�̏ꍇ
        if (other.tag == "Mud")
        {
            isTouchedMud = true;
            SetEffect(EFFECT_ID.MudSplash);
            SetEffect(EFFECT_ID.MudRipples);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // �D�̏ꍇ
        if (other.tag == "Mud")
        {
            isTouchedMud = false;
            StopParticle(EFFECT_ID.MudRipples.ToString());
        }
    }
}
