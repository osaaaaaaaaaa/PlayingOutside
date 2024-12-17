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
    Rigidbody rb;
    #endregion

    #region �v���C���[�̃X�e�[�^�X
    float moveX;
    float moveZ;

    public int hpMax;
    public int hp;
    public float testA;
    public float testB;

    #region �X�s�[�h�E�W�����v
    float speed;
    public float Speed { get { return speed; } set { speed = value; } }
    public float defaultSpeed { get; private set; } = 5;
    float jumpPower;
    public float JumpPower { get { return speed; } set { speed = value; } }
    public float defaultJumpPower { get; private set; } = 500;
    #endregion
    #endregion

    private void Start()
    {
        speed = defaultSpeed;
        jumpPower = defaultJumpPower;
        animController = GetComponent<PlayerAnimatorController>();
        rb = GetComponent<Rigidbody>();

        hp = hpMax;
    }

    void Update()
    {
        // �L�b�N����
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("��Ԃ悤��");
            rb.AddForce(new Vector3(0, testB, testB), ForceMode.Impulse);
        }

        if (!animController.isStandUp) return;

        // �L�[���͂ňړ��������X�V
        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");

        if (!GetComponent<PlayerIsGroundController>().IsGround() || !animController.isStandUp || !animController.isControlEnabled) return;

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

        //// �L�b�N����
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Debug.Log("��Ԃ悤��");
        //    //transform.position += Vector3.up * GetComponent<PlayerIsGroundController>().rayHeight;
        //    rb.AddForce(new Vector3(0,testB,testB), ForceMode.Impulse);
        //    //animController.SetInt(PlayerAnimatorController.ANIM_ID.Kick);
        //}
    }

    private void FixedUpdate()
    {
        if (!animController.isStandUp || !animController.isControlEnabled) return;

        // �J�����̌����ƉE�����̑傫�����擾����
        Vector3 cameraRot = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);

        // �ړ��ʂ�ݒ�
        Vector3 setMove = (cameraRight * moveX + cameraRot * moveZ).normalized; // �傫�����P�ɂ���
        rb.velocity = new Vector3(setMove.x * speed, rb.velocity.y, setMove.z * speed); // �������x��������Ȃ��悤�ɂ���

        // ���炩�ɉ�]
        transform.forward = Vector3.Slerp(transform.forward, setMove, Time.deltaTime * 30f);   // ��]���x��������
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            Hit(other.GetComponent<DamageCollider>().damage,other.transform);
        }
    }

    /// <summary>
    /// �_���[�W���󂯂鏈��
    /// </summary>
    public void Hit(int damage, Transform tf)
    {
        if (hp <= 0 || !animController.isStandUp || animController.isInvincible) return;

        hp -= damage;
        LookAtPlayer(tf);
        Vector3 knockBackVec = (this.transform.position - tf.position).normalized;
        rb.velocity = Vector3.zero;

        if (hp <= 0)
        {
            knockBackVec *= testA;
            knockBackVec = new Vector3(knockBackVec.x * testA, testA, knockBackVec.z * testA);
            KnockBackAndDown(knockBackVec);
        }
        else
        {
            // �����m�b�N�o�b�N����
            knockBackVec = new Vector3(knockBackVec.x * testB, 1f, knockBackVec.z * testB);
            rb.AddForce(knockBackVec,ForceMode.Impulse);

            animController.SetInt(PlayerAnimatorController.ANIM_ID.Damage);
        }
    }

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
        if (!animController.isStandUp || animController.isInvincible) return;
        hp = hpMax;
        transform.position += Vector3.up * GetComponent<PlayerIsGroundController>().rayHeight;
        animController.PlayKnockBackAnim();
        rb.AddForce(knockBackVec, ForceMode.Impulse);
    }

    public void InitPlayer(Transform startPointTf)
    {
        transform.position = startPointTf.position;
        transform.eulerAngles += startPointTf.eulerAngles;
        speed = defaultSpeed;
    }
}
