using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    PlayerController playerController;
    PlayerAnimatorController playerAnimatorController;
    Rigidbody rb;

    // 固有スキルのパーティクル
    [SerializeField] GameObject skillObj;

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
    [SerializeField] float forsePower;
    bool isUseForse;
    bool isEndSkill5;

    private void Start()
    {
        skillObj.SetActive(false);
        isUseForse = false;
        playerController = GetComponent<PlayerController>();
        playerAnimatorController = GetComponent<PlayerAnimatorController>();
        rb = GetComponent<Rigidbody>();
        isEndSkill5 = true;
    }

    private void Update()
    {
        if (!isEndSkill5 && skillID == SKILL_ID.Skill5 && isUsedSkill && GetComponent<PlayerIsGroundController>().IsGround())
        {
            // スキル5再生中で落下している場合
            if (playerController.Rb.drag > 0)
            {
                isEndSkill5 = true;
                transform.GetChild(0).transform.localPosition = Vector3.zero;
                playerAnimatorController.PlayAnimationFromFrame(148, "Skill5");
            }
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
        isUsedSkill = true;
        skillObj.SetActive(true);
        skillObj.GetComponent<ParticleSystem>().Play();

        switch (skillID)
        {
            case SKILL_ID.Skill1:
                playerController.IsInvincible = true;
                playerController.Speed = 3f;
                skillObj.GetComponent<SphereCollider>().enabled = true;
                break;
            case SKILL_ID.Skill2:
                playerController.IsInvincible = true;
                playerController.IsControlEnabled = false;
                skillObj.GetComponent<BoxCollider>().enabled = true;

                isUseForse = true;
                forsePower = 8;
                break;
            case SKILL_ID.Skill3:
                Invoke("OnEndSkillAnim", 8f);
                break;
            case SKILL_ID.Skill4:
                playerController.IsInvincible = true;
                playerController.IsControlEnabled = false;
                break;
            case SKILL_ID.Skill5:
                playerController.IsInvincible = true;
                playerController.Speed = 3f;
                isEndSkill5 = false;
                break;
        }
    }

    public void OnEndSkillAnim()
    {
        isUseForse = false;
        skillObj.SetActive(false);
        playerController.InitPlayer();
        isUsedSkill = false;

        switch (skillID)
        {
            case SKILL_ID.Skill1:
                skillObj.GetComponent<SphereCollider>().enabled = false;
                break;
            case SKILL_ID.Skill2:
                skillObj.GetComponent<BoxCollider>().enabled = false;
                break;
            case SKILL_ID.Skill3:
                break;
            case SKILL_ID.Skill4:
                skillObj.GetComponent<SphereCollider>().enabled = false;
                break;
            case SKILL_ID.Skill5:
                isEndSkill5 = true;
                break;
        }
    }
}
