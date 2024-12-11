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
    #region �n�ʔ���p
    [SerializeField] LayerMask groundRayer;
    const float rayHeight = 0.2f;
    const float rayWeight = 0.4f;
    #endregion

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
        if (!animController.isStandUp) return;

        // �L�[���͂ňړ��������X�V
        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");

        if (IsGround() && !animController.IsAnimRunning())
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
                transform.position += Vector3.up * rayHeight;
                animController.SetInt(PlayerAnimatorController.ANIM_ID.Jump);
                rb.AddForce(transform.up * jumpPower);
            }

            // �L�b�N����
            if (Input.GetMouseButtonDown(0)) 
            {
                animController.SetInt(PlayerAnimatorController.ANIM_ID.Kick);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!animController.isStandUp || animController.IsAnimRunning()) return;

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
        if (other.tag == "Attack")
        {
            Debug.Log("a");
            Hit(other.GetComponent<AttackCollider>().damage,other.transform);
        }
    }

    /// <summary>
    /// �_���[�W���󂯂鏈��
    /// </summary>
    public void Hit(int damage, Transform tf)
    {
        if (hp <= 0 || !animController.isStandUp
            || animController.isInvincible) return;

        hp -= damage;
        Vector3 knockBackVec = (this.transform.position - tf.position).normalized;

        if (hp <= 0)
        {
            knockBackVec *= testA;
            knockBackVec = new Vector3(knockBackVec.x * testA / 2, testA, knockBackVec.z * testA / 2);
            Debug.Log(knockBackVec.ToString());
            KnockBackAndDown(knockBackVec);
        }
        else
        {
            // �A�����ă_���[�W���󂯂Ȃ��悤�ɁA���������m�b�N�o�b�N����
            transform.position += new Vector3(knockBackVec.x * rayHeight, 0, knockBackVec.z * rayHeight);

            knockBackVec = new Vector3(knockBackVec.x * testB, testB, knockBackVec.z * testB);
            rb.AddForce(knockBackVec, ForceMode.VelocityChange);

            animController.SetInt(PlayerAnimatorController.ANIM_ID.Damage);
        }
    }

    /// <summary>
    /// �n�ʔ���p
    /// </summary>
    /// <returns></returns>
    public bool IsGround()
    {
        Vector3 basePos = transform.position;    // �����X�^�[�̃s�{�b�g�����S�ɂ��邽�ߒ�������
        Vector3 leftStartPos= basePos - Vector3.right * rayWeight;      // �����̎n�_
        Vector3 rightStartPos = basePos + Vector3.right * rayWeight;    // �E���̎n�_
        Vector3 forwardStartPos = basePos - Vector3.back * rayWeight;   // �O�̎n�_
        Vector3 backStartPos = basePos + Vector3.back * rayWeight;      // ���̎n�_
        Vector3 endPosition = basePos - Vector3.up * rayHeight;     // �I�_(��)

        Debug.DrawLine(leftStartPos, endPosition, Color.red);
        Debug.DrawLine(rightStartPos, endPosition, Color.red);
        Debug.DrawLine(forwardStartPos, endPosition, Color.blue);
        Debug.DrawLine(backStartPos, endPosition, Color.blue);

        return Physics.Linecast(leftStartPos, endPosition, groundRayer)
            || Physics.Linecast(rightStartPos, endPosition, groundRayer)
            || Physics.Linecast(forwardStartPos, endPosition, groundRayer)
            || Physics.Linecast(backStartPos, endPosition, groundRayer);
    }

    /// <summary>
    /// �m�b�N�o�b�N���_�E�����鏈��
    /// </summary>
    /// <param name="knockBackVec"></param>
    public void KnockBackAndDown(Vector3 knockBackVec)
    {
        if (animController.isInvincible) return;

        if (animController.isStandUp && hp <= 0)
        {
            hp = hpMax;
            transform.position += Vector3.up * rayHeight;
            animController.PlayKnockBackAnim();
            rb.AddForce(knockBackVec, ForceMode.VelocityChange);
        }
    }

    public void InitPlayer(Transform startPointTf)
    {
        transform.position = startPointTf.position;
        transform.eulerAngles += startPointTf.eulerAngles;
        speed = defaultSpeed;
    }
}
