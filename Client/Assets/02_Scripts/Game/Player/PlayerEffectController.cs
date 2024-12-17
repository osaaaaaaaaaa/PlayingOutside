using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerEffectController : MonoBehaviour
{
    [SerializeField] List<GameObject> effectPrefabs;
    [SerializeField] GameObject effectMudRipples;
    public enum EFFECT_ID
    {
        Run = 0,    // �A�j���[�V��������̌Ăяo��
        Landing,    // �A�j���[�V��������̌Ăяo��
        AreaCleared,
        MudSplash,
        Down,       // �A�j���[�V��������̌Ăяo��
    }

    bool isTouchedMud;

    private void Awake()
    {
        isTouchedMud = false;
    }

    private void OnEnable()
    {
        isTouchedMud = false;
        effectMudRipples.SetActive(false);
    }

    public void SetEffect(EFFECT_ID efectId)
    {
        switch (efectId)
        {
            case EFFECT_ID.Run:
                if (!isTouchedMud)
                    Instantiate(effectPrefabs[(int)efectId], this.transform.position + Vector3.up * 0.1f, Quaternion.identity);
                break;
            case EFFECT_ID.Landing:
                if (!isTouchedMud)
                {
                    Instantiate(effectPrefabs[(int)efectId], this.transform.position, Quaternion.identity);
                }
                    
                break;
            case EFFECT_ID.AreaCleared:
                Instantiate(effectPrefabs[(int)efectId], this.transform.position + Vector3.up, Quaternion.identity);
                break;
            case EFFECT_ID.MudSplash:
                Instantiate(effectPrefabs[(int)efectId], this.transform.position + Vector3.up / 2, Quaternion.identity);
                break;
            case EFFECT_ID.Down:
                Instantiate(effectPrefabs[(int)efectId], this.transform.position + Vector3.up, Quaternion.identity);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // �D�̏ꍇ
        if (other.tag == "Mud")
        {
            isTouchedMud = true;
            effectMudRipples.SetActive(true);
            SetEffect(EFFECT_ID.MudSplash);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // �D�̏ꍇ
        if (other.tag == "Mud")
        {
            isTouchedMud = false;
            effectMudRipples.SetActive(false);
        }
    }
}
