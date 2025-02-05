//*********************************************************
// キャラクターのスキルを管理するスクリプト
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerSkillController : MonoBehaviour
{
    PlayerController playerController;
    PlayerAnimatorController playerAnimatorController;
    PlayerAudioController playerAudioController;
    Rigidbody rb;
    [SerializeField] PlayerAnimEventTrigger eventTrigger;

    [SerializeField] GameObject skillObj;       // 固有スキルのパーティクル

    public bool isUsedSkill { get; private set; }

    public enum SKILL_ID
    {
        Skill1,
        Skill2,
        Skill3,
        Skill4,
        Skill5
    }

    [SerializeField] SKILL_ID skillID = SKILL_ID.Skill1;
    public SKILL_ID SkillId { get { return skillID; } }

    [SerializeField] float coolTime;
    public float CoolTime { get { return coolTime; } }
    float forsePower;
    bool isUseForse;

    private void Start()
    {
        skillObj.SetActive(false);
        isUseForse = false;
        playerController = GetComponent<PlayerController>();
        playerAnimatorController = GetComponent<PlayerAnimatorController>();
        playerAudioController = GetComponent<PlayerAudioController>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // スキル5再生中で落下して着地するとき
        if (eventTrigger.IsPlayStampAnimFall && skillID == SKILL_ID.Skill5 && isUsedSkill && GetComponent<PlayerIsGroundController>().IsGround())
        {
            eventTrigger.IsPlayStampAnimFall = false;
            skillObj.SetActive(true);
            skillObj.GetComponent<ParticleSystem>().Play();
            skillObj.GetComponent<BoxCollider>().enabled = true;
            transform.GetChild(0).transform.localPosition = Vector3.zero;   // モデルを初期化
            transform.gameObject.layer = 3;
            playerAnimatorController.PlayAnimationFromFrame(148, "Skill5");
            playerController.IsControlEnabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (!isUseForse) return;

        Vector3 forse = transform.forward.normalized * forsePower;
        rb.velocity = new Vector3(forse.x, rb.velocity.y,forse.z);
    }

    public void OnStartSkillAnim()
    {
        if (isUsedSkill) return;
        isUsedSkill = true;
        bool isShowParticle = true;

        switch (skillID)
        {
            case SKILL_ID.Skill1:
                playerController.IsInvincible = true;
                playerController.Speed = 3f;
                skillObj.GetComponent<SphereCollider>().enabled = true;
                break;
            case SKILL_ID.Skill2:
                transform.gameObject.layer = 8; // 他のプレイヤーとの接触判定を無くす
                playerController.IsInvincible = true;
                playerController.IsControlEnabled = false;
                skillObj.GetComponent<BoxCollider>().enabled = true;

                isUseForse = true;
                forsePower = 8;
                break;
            case SKILL_ID.Skill3:
                playerController.IsInvincible = true;
                playerController.IsControlEnabled = false;
                Invoke("OnEndSkillAnim", 12f);
                break;
            case SKILL_ID.Skill4:
                playerController.IsInvincible = true;
                playerController.IsControlEnabled = false;
                break;
            case SKILL_ID.Skill5:
                isShowParticle = false;
                transform.gameObject.layer = 8; // 他のプレイヤーとの接触判定を無くす
                playerController.IsInvincible = true;
                playerController.Speed = 3f;
                playerController.IsControlEnabled = false;
                break;
        }

        if (isShowParticle)
        {
            skillObj.SetActive(true);
            skillObj.GetComponent<ParticleSystem>().Play();
        }
    }

    public void OnEndSkillAnim()
    {
        CancelInvoke("OnEndSkillAnim");
        isUseForse = false;
        isUsedSkill = false;
        skillObj.SetActive(false);
        playerController.InitPlayer();
        playerAudioController.StopLoopSkillSourse();

        switch (skillID)
        {
            case SKILL_ID.Skill1:
                skillObj.GetComponent<SphereCollider>().enabled = false;
                break;
            case SKILL_ID.Skill2:
                skillObj.GetComponent<BoxCollider>().enabled = false;
                break;
            case SKILL_ID.Skill3:
                playerAnimatorController.SetInt(PlayerAnimatorController.ANIM_ID.IdleB);
                break;
            case SKILL_ID.Skill4:
                skillObj.GetComponent<SphereCollider>().enabled = false;
                break;
            case SKILL_ID.Skill5:
                skillObj.GetComponent<BoxCollider>().enabled = false;
                eventTrigger.IsPlayStampAnimFall = false;
                break;
        }
    }
}
