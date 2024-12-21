using Cinemachine;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    #region �R���|�[�l���g
    PlayerAnimatorController animController;
    PlayerSkillController skillController;
    Rigidbody rb;
    CinemachineImpulseSource impulseSource;
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
    public float defaultSpeed { get; private set; } = 5;
    float jumpPower;
    #endregion
    #endregion

    // ���̃v���C���[��Transform
    [SerializeField] List<GameObject> objOtherPlayers = new List<GameObject>();

    // ���G��Ԃ��ǂ���
    bool isInvincible;
    public bool IsInvincible { get { return isInvincible; }set { isInvincible = value; } }

    // ���삪�\���ǂ���
    bool isControlEnabled = true;
    public bool IsControlEnabled { get { return isControlEnabled; }set { isControlEnabled = value; } }

    private void Awake()
    {
        skillController = GetComponent<PlayerSkillController>();
        animController = GetComponent<PlayerAnimatorController>();
        rb = GetComponent<Rigidbody>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        speed = defaultSpeed;
        jumpPower = 500;
        hp = hpMax;
    }

    void Update()
    {
        if (!animController.isStandUp || !isControlEnabled)
        {
            moveX = 0;
            moveZ = 0;
            return;
        }

        // �L�[���͂ňړ��������X�V
        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");

        if (GetComponent<PlayerIsGroundController>().IsGround() && !skillController.isUsedSkill)
        {
            if (moveX != 0 || moveZ != 0)
            {
                animController.SetInt(PlayerAnimatorController.ANIM_ID.Run);
            }
            else
            {
                animController.SetInt(PlayerAnimatorController.ANIM_ID.IdleB);
            }

            // �W�����v����
            if (Input.GetKeyDown(KeyCode.Space))
            {
                transform.position += Vector3.up * GetComponent<PlayerIsGroundController>().rayHeight;
                animController.SetInt(PlayerAnimatorController.ANIM_ID.Jump);
                rb.AddForce(transform.up * jumpPower);
            }

            // �L�b�N����
            if (Input.GetMouseButtonDown(0))
            {
                var target = SerchNearTarget();
                if(target != null) LookAtPlayer(target);
                animController.SetInt(PlayerAnimatorController.ANIM_ID.Kick);
            }

            // �X�L����������
            if (Input.GetKeyDown(KeyCode.E))
            {
                animController.SetInt(PlayerAnimatorController.ANIM_ID.Skill);
            }
        }
    }

    private void FixedUpdate()
    {
        if (moveX == 0 && moveZ == 0 || !animController.isStandUp || !isControlEnabled) return;

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
    }

    /// <summary>
    /// �_���[�W���󂯂鏈��
    /// </summary>
    public void Hit(int damage, Vector3 specifiedKnockback, Transform tf)
    {
        if (hp <= 0 || !animController.isStandUp || isInvincible) return;

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
    public void KnockBackAndDown(Vector3 knockBackVec)
    {
        if (!animController.isStandUp || isInvincible) return;
        hp = hpMax;
        transform.position += Vector3.up * GetComponent<PlayerIsGroundController>().rayHeight;
        animController.PlayKnockBackAnim();
        rb.AddForce(knockBackVec, ForceMode.Impulse);
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

    public void InitPlayer(Transform startPointTf)
    {
        transform.position = startPointTf.position;
        transform.eulerAngles += startPointTf.eulerAngles;
        speed = defaultSpeed;
    }

    public void InitPlayer()
    {
        this.gameObject.layer = 3;
        speed = defaultSpeed;
        isControlEnabled = true;
    }
}
