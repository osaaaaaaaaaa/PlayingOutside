using Cinemachine;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    #region �R���|�[�l���g
    PlayerAnimatorController animController;
    PlayerSkillController skillController;
    PlayerItemController itemController;
    PlayerEffectController effectController;
    CinemachineImpulseSource impulseSource;
    CharacterControlUI characterControlUI;
    FloatingJoystick floatingJoystick;
    Rigidbody rb;
    public Rigidbody Rb { get { return rb; } }
    #endregion

    #region �J�����֌W
    const float minShakeVecCamera = 0.5f;
    const float maxShakeVecCamera = 2f;
    #endregion

    #region �v���C���[�̃X�e�[�^�X
    float moveX;
    float moveZ;

    public int hpMax;
    public int hp;

    public float rangeKick;

    #region �m�b�N�o�b�N�����
    public float addKnockBackPower = 5;
    public float correctionKnockBackPowerY = 2f;
    public float defaultKnockBackPower1 = 5;
    public float defaultKnockBackPower2 = 15;
    #endregion

    #region �X�s�[�h�E�W�����v
    public float speed;
    public float Speed { get { return speed; } set { speed = value; } }
    float defaultSpeed = 5f;
    public float DefaultSpeed { get { return defaultSpeed; } set { defaultSpeed = value; } }

    // ���Z����X�s�[�h
    public float addPepperSpeed { get; private set; } = 3f;
    float addMudSpeed = -2f;

    float jumpPower;
    [SerializeField] float defaultJumpPower;

    #endregion
    #endregion

    Transform modelTf;

    // ����������W
    public Vector3 respawnPoint { get; private set; } = Vector3.zero;

    // ���̃v���C���[��Transform
    [SerializeField] List<GameObject> objOtherPlayers = new List<GameObject>();

    // �����オ���Ă��邩�ǂ���
    public bool isStandUp;
    public bool IsStandUp { get { return isStandUp; } set { isStandUp = value; } }

    // ���G��Ԃ��ǂ���
    bool isInvincible;
    public bool IsInvincible { get { return isInvincible; }set { isInvincible = value; } }

    // ���삪�\���ǂ���
    public bool isControlEnabled = true;
    public bool IsControlEnabled { get { return isControlEnabled; }set { isControlEnabled = value; } }

    // �}�b�n�L�b�N�{�^���������Ă��邩�ǂ���
    bool isPressMachKickBtn;
    public bool IsPressMachKickBtn { get { return isPressMachKickBtn; } set { isPressMachKickBtn = value; } }

    bool isMudCollider;

    public bool isDebug;

    private void Awake()
    {
        skillController = GetComponent<PlayerSkillController>();
        animController = GetComponent<PlayerAnimatorController>();
        itemController = GetComponent<PlayerItemController>();
        effectController = GetComponent<PlayerEffectController>();
        rb = GetComponent<Rigidbody>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        var CharacterControls = GameObject.Find("CharacterControls");
        if(CharacterControls) characterControlUI = CharacterControls.GetComponent<CharacterControlUI>();
        var FloatingJoystick = GameObject.Find("Floating Joystick");
        if(FloatingJoystick) floatingJoystick = FloatingJoystick.GetComponent<FloatingJoystick>();
        modelTf = transform.GetChild(0);


        speed = defaultSpeed;
        jumpPower = defaultJumpPower;
        hp = hpMax;
        isStandUp = true;
        isInvincible = false;
        isMudCollider = false;
    }

    private void OnEnable()
    {
        if (characterControlUI != null && this.gameObject.layer == 3) characterControlUI.ToggleUIVisibiliity(true);
    }

    private void OnDisable()
    {
        if (characterControlUI != null && this.gameObject.layer == 3) characterControlUI.ToggleUIVisibiliity(false);
    }

    void Update()
    {
        if (!isStandUp || !isControlEnabled)
        {
            moveX = 0;
            moveZ = 0;
            characterControlUI.ToggleButtonInteractable(false);
            return;
        }

        // �L�[���͂ňړ��������X�V
        if (floatingJoystick && floatingJoystick.Horizontal != 0 && floatingJoystick.Vertical != 0)
        {
            moveX = floatingJoystick.Horizontal;
            moveZ = floatingJoystick.Vertical;
        }
        else
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveZ = Input.GetAxisRaw("Vertical");
        }

        bool isGround = GetComponent<PlayerIsGroundController>().IsGround();
        bool isMachAura = skillController.SkillId == PlayerSkillController.SKILL_ID.Skill3 && skillController.isUsedSkill;
        bool isInteractable = isGround && !skillController.isUsedSkill;
        if (isMachAura)
        {
            characterControlUI.ToggleButtonInteractable(isGround);
        }
        else
        {
            characterControlUI.ToggleButtonInteractable(isInteractable);
        }

        if (isInteractable || isMachAura && isGround)
        {
            if (moveX != 0 || moveZ != 0)
            {
                bool isUsePepper = itemController.itemEffectTimeList.ContainsKey(EnumManager.ITEM_ID.Pepper);
                if (!isUsePepper) animController.SetInt(PlayerAnimatorController.ANIM_ID.Run);
                if (isUsePepper) animController.SetInt(PlayerAnimatorController.ANIM_ID.RunFast);
            }
            else
            {
                // �}�b�n�L�b�N���̓A�C�h���A�j�����Đ����Ȃ�
                if (animController.GetAnimId() != (int)PlayerAnimatorController.ANIM_ID.MachKick)
                {
                    if (isMachAura) animController.SetInt(PlayerAnimatorController.ANIM_ID.IdleA);
                    if (!isMachAura) animController.SetInt(PlayerAnimatorController.ANIM_ID.IdleB);
                }
            }

            // ���Ƃ肠�����c���Ă���####################################################

            // �W�����v����
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (characterControlUI.IsSetupDone)
                    characterControlUI.ClickEvent(CharacterControlUI.ButtonType.jump);
                else OnJumpButton();
            }

            // �X�L����������
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (characterControlUI.IsSetupDone)
                    characterControlUI.ClickEvent(CharacterControlUI.ButtonType.skill);
                else OnSkillButton();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (characterControlUI.IsSetupDone)
                    characterControlUI.ClickEvent(CharacterControlUI.ButtonType.item);
            }
        }

        // ���Ƃ肠�����c���Ă���####################################################
        if (isInteractable || isMachAura)
        {
            // �L�b�N����
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (characterControlUI.IsSetupDone)
                    characterControlUI.ClickEvent(CharacterControlUI.ButtonType.kick);
                else OnKickButton();
            }
        }
    }

    public void OnKickButton()
    {
        bool isMachAura = skillController.SkillId == PlayerSkillController.SKILL_ID.Skill3 && skillController.isUsedSkill;
        var target = SerchNearTarget();
        if (target != null) LookAtPlayer(target);
        if (isMachAura)
        {
            isPressMachKickBtn = true;
            animController.Animator.SetInteger("animation", (int)PlayerAnimatorController.ANIM_ID.MachKick);
        }
        else
        {
            animController.SetInt(PlayerAnimatorController.ANIM_ID.Kick);
        }
    }

    public void OnJumpButton()
    {
        Jump(jumpPower);
    }

    public void OnSkillButton()
    {
        animController.SetInt(animController.GetSkillAnimId());
        characterControlUI.OnSkillButton();
    }

    public void Jump(float _jumpPower)
    {
        transform.position += Vector3.up * GetComponent<PlayerIsGroundController>().rayHeight;
        animController.SetInt(PlayerAnimatorController.ANIM_ID.Jump);
        rb.AddForce(transform.up * _jumpPower);
    }

    private void FixedUpdate()
    {
        if (moveX == 0 && moveZ == 0 || !isStandUp || !isControlEnabled) return;

        // �J�����̌����ƉE�����̑傫�����擾����
        Vector3 cameraRot = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);

        // �ړ��ʂ�ݒ�
        Vector3 setMove = (cameraRight * moveX + cameraRot * moveZ).normalized; // �傫�����P�ɂ���
        rb.velocity = new Vector3(setMove.x * speed, rb.velocity.y, setMove.z * speed); // �������x��������Ȃ��悤�ɂ���

        // ���炩�ɉ�]
        transform.forward = Vector3.Slerp(transform.forward, setMove, Time.deltaTime * 30f);   // ��]���x��������
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<DamageCollider>())
        {
            var damageCollider = other.GetComponent<DamageCollider>();
            Hit(damageCollider.Damage, damageCollider.SpecifiedKnockback, other.transform);
        }
        else if(other.gameObject.tag == "Checkpoint")
        {
            respawnPoint = other.transform.position;
        }
    }

    /// <summary>
    /// �_���[�W���󂯂鏈��
    /// </summary>
    public void Hit(int damage, Vector3 specifiedKnockback, Transform tf)
    {
        if (hp <= 0 || !isStandUp || isInvincible) return;

        // �_���[�W�ɗ�����������
        damage += Random.Range(0, 10);

        hp -= damage;
        LookAtPlayer(tf);
        Vector3 knockBackVec = (this.transform.position - tf.position).normalized;
         rb.velocity = Vector3.zero;

        if (hp <= 0 || specifiedKnockback != Vector3.zero)
        {
            // �m�b�N�o�b�N����x�N�g�����w�肳��Ă��Ȃ��ꍇ
            if(specifiedKnockback == Vector3.zero)
            {
                // �_���[�W�ʂɉ����ăm�b�N�o�b�N����͂��ω�
                float addPower = (float)damage / hpMax * addKnockBackPower;
                float resultPower = defaultKnockBackPower2 + addPower;
                if (resultPower > defaultKnockBackPower2 + addKnockBackPower) resultPower = defaultKnockBackPower2 + addKnockBackPower;
                if (resultPower < defaultKnockBackPower2) resultPower = defaultKnockBackPower2;

                // �傫���m�b�N�o�b�N����
                knockBackVec *= resultPower;
                knockBackVec = new Vector3(knockBackVec.x, resultPower / correctionKnockBackPowerY, knockBackVec.z);
            }
            else
            {
                knockBackVec = specifiedKnockback;
            }

            KnockBackAndDown(knockBackVec);

            // �J�������_���[�W�ʂɉ����ėh�炷
            float shakePower = (float)damage / hpMax * (maxShakeVecCamera - minShakeVecCamera);
            shakePower += minShakeVecCamera;
            if(shakePower > maxShakeVecCamera) shakePower = maxShakeVecCamera;
            if(shakePower < minShakeVecCamera) shakePower = minShakeVecCamera;

            impulseSource.m_DefaultVelocity = Vector3.one * shakePower;
            impulseSource.GenerateImpulse();
        }
        else
        {
            // �_���[�W�ʂɉ����ăm�b�N�o�b�N����͂��ω�
            float addPower = (float)damage / hpMax * addKnockBackPower;
            float resultPower = defaultKnockBackPower1 + addPower;
            if (resultPower > defaultKnockBackPower1 + addKnockBackPower) resultPower = defaultKnockBackPower1 + addKnockBackPower;
            if (resultPower < defaultKnockBackPower1) resultPower = defaultKnockBackPower1;

            // �y���m�b�N�o�b�N����
            knockBackVec *= resultPower;
            knockBackVec = new Vector3(knockBackVec.x, resultPower / correctionKnockBackPowerY, knockBackVec.z);
            rb.AddForce(knockBackVec,ForceMode.Impulse);

            animController.SetInt(PlayerAnimatorController.ANIM_ID.Damage);
        }
    }

    /// <summary>
    /// �^�[�Q�b�g�̕��������� 
    /// </summary>
    /// <param name="target"></param>
    void LookAtPlayer(Transform target)
    {
        // �^�[�Q�b�g�ւ̌����x�N�g���v�Z
        var dir = target.position - transform.position;

        // �^�[�Q�b�g�̕����ւ̉�]
        var lookAtRotation = Quaternion.LookRotation(dir, Vector3.up);
        // ��]�␳
        var angles = Vector3.Scale(lookAtRotation.eulerAngles, Vector3.up); // �v���C���[�̃s�{�b�g�������̂���Y�ȊO��0�ɂ���

        // �^�[�Q�b�g�̕���������
        transform.eulerAngles = angles;
    }

    /// <summary>
    /// �m�b�N�o�b�N���_�E�����鏈��
    /// </summary>
    /// <param name="knockBackVec"></param>
    public async void KnockBackAndDown(Vector3 knockBackVec)
    {
        if (!isStandUp || isInvincible) return;
        hp = hpMax;
        transform.gameObject.layer = 8; // �m�b�N�_�E����Ԃɂ���

        // �m�b�N�o�b�N���o
        transform.position += Vector3.up * GetComponent<PlayerIsGroundController>().rayHeight;
        animController.PlayKnockBackAnim();
        rb.AddForce(knockBackVec, ForceMode.Impulse);

        // �R�C��(�|�C���g)�̃h���b�v����
        if(!isDebug && SceneManager.GetActiveScene().name == "FinalGameScene") await RoomModel.Instance.KnockDownAsynk(transform.position);
    }

    /// <summary>
    /// ��ԋ߂��^�[�Q�b�g���擾
    /// </summary>
    Transform SerchNearTarget()
    {
        if(objOtherPlayers.Count == 0)
        {
            // �܂����̃v���C���[���擾���Ă��Ȃ��ꍇ�͎擾����
            var otherPlayers = GameObject.FindGameObjectsWithTag("Character");
            objOtherPlayers = new List<GameObject>(otherPlayers);
            objOtherPlayers.Remove(this.gameObject);
        }

        Transform target = null;
        float minDis = rangeKick;
        foreach (GameObject player in objOtherPlayers) 
        {
            if(player == null) continue;

            float dis = Mathf.Abs(Vector3.Distance(this.transform.position, player.transform.position));
            if (dis < minDis)
            {
                minDis = dis;
                target = player.transform;
            }
        }

        return target;
    }

    public void OnColliderMudEnter()
    {
        if (isMudCollider) return;
        speed += addMudSpeed;
        jumpPower = defaultJumpPower / 1.5f;
        isMudCollider = true;
    }

    public void OnColliderMudExit()
    {
        if (!isMudCollider) return;
        speed -= addMudSpeed;
        jumpPower = defaultJumpPower;
        isMudCollider = false;
    }

    void InitParam()
    {
        // �A�C�e�����ʁE�p�[�e�B�N�����Z�b�g
        itemController.ClearAllItemEffects();
        effectController.ClearAllParticles();

        rb.drag = 0;
        IsStandUp = true;
        IsControlEnabled = true;
        IsInvincible = false;
        isMudCollider = false;
        this.gameObject.layer = 3;
        defaultSpeed = 5;
        speed = defaultSpeed;
        jumpPower = defaultJumpPower;
        GetComponent<PlayerAudioController>().ResetRunningSourse(PlayerAudioController.AudioClipName.running_default);
    }

    public void Respawn()
    {
        transform.position = respawnPoint;
        InitParam();
    }

    /// <summary>
    /// �����ݒ�ȂǂɌĂяo��
    /// </summary>
    /// <param name="startPointTf"></param>
    public void InitPlayer(Transform startPointTf)
    {
        respawnPoint = startPointTf.position;
        transform.position = startPointTf.position;
        transform.eulerAngles += startPointTf.eulerAngles;
        modelTf.localPosition = Vector3.zero;

        InitParam();
    }

    /// <summary>
    /// �A�j���[�V�����I�����ȂǂɌĂяo��
    /// </summary>
    public void InitPlayer()
    {
        modelTf.localPosition = Vector3.zero;
        this.gameObject.layer = 3;
        isControlEnabled = true;
        rb.drag = 0;

        speed = defaultSpeed;
        jumpPower = defaultJumpPower;
        if (itemController.itemEffectTimeList.ContainsKey(EnumManager.ITEM_ID.Pepper)) speed += addPepperSpeed;
        if (isMudCollider)
        {
            speed += addMudSpeed;
            jumpPower = defaultJumpPower / 1.5f;
        }
    }
}
