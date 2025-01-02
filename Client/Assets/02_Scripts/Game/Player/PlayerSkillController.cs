using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    PlayerController playerController;
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
        SKill5
    }

    [SerializeField] SKILL_ID skillID = SKILL_ID.Skill1;
    public SKILL_ID SkillId { get { return skillID; } }

    [SerializeField] float coolTime;
    public float CoolTime { get { return coolTime; } }
    [SerializeField] float forsePower;
    bool isUseForse;

    private void Start()
    {
        skillObj.SetActive(false);
        isUseForse = false;
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
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
        }
    }
}
