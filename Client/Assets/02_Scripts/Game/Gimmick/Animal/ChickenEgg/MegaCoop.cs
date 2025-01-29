using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class MegaCoop : MonoBehaviour
{
    [SerializeField] SEController seController;
    [SerializeField] MegaChickenAnim megaChicken;
    Vector3 startPos;
    bool isPlayAnim;

    private void Start()
    {
        startPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlayAnim) return;

        // �_���[�W�R���C�_�[�̎擾�`�F�b�N
        var damageCompornent = other.GetComponent<DamageCollider>();
        if (damageCompornent == null) return;

        var playerController = damageCompornent.root.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // NPC�ł͂Ȃ��ꍇ
            if (playerController.gameObject.layer == 3)
            {
                if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
                {
                    PlayCoopGimmickAsynk();
                }
                else
                {
                    PlayDamageAnim();
                }
            }
        }
    }

    /// <summary>
    /// �_���[�W���󂯂�A�j���[�V�������Đ�����
    /// </summary>
    public void PlayDamageAnim()
    {
        if (isPlayAnim) return;
        isPlayAnim = true;
        seController.PlayAudio();
        DOTween.Kill(this.gameObject);
        transform.position = startPos;
        transform.DOPunchPosition(new Vector3(1f, 0, 0), 1f, 10, 1.5f).OnComplete(megaChicken.PlayShowAnim);
    }

    /// <summary>
    /// �M�~�b�N�������N�G�X�g
    /// </summary>
    async void PlayCoopGimmickAsynk()
    {
        // Tween�������N�G�X�g
        await RoomModel.Instance.TriggerMegaCoopAsynk();
    }

    /// <summary>
    /// ���K�{���B��鏈�������������Ƃ�
    /// </summary>
    public async void OnHideMegaChickenTween()
    {
        if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
        {
            await RoomModel.Instance.TriggerMegaCoopEndAsynk();
        }
        else
        {
            isPlayAnim = false;
        }   
    }

    /// <summary>
    /// �{�����̃M�~�b�N�I���ʒm����M�����Ƃ�
    /// </summary>
    public void OnTriggerEnd()
    {
        isPlayAnim = false;
    }
}
