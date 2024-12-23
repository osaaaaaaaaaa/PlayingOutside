using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    PlayerController playerController;

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

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void OnStartSkillAnim()
    {
        skillObj.SetActive(true);
        skillObj.GetComponent<ParticleSystem>().Play();

        switch (skillID)
        {
            case SKILL_ID.Skill1:
                playerController.IsInvincible = true;
                playerController.Speed = 3f;
                skillObj.GetComponent<SphereCollider>().enabled = true;
                isUsedSkill = true;
                break;
        }
    }

    public void OnEndSkillAnim()
    {
        skillObj.SetActive(false);
        playerController.InitPlayer();
        isUsedSkill = false;

        switch (skillID)
        {
            case SKILL_ID.Skill1:
                skillObj.GetComponent<SphereCollider>().enabled = false;
                break;
        }
    }
}
