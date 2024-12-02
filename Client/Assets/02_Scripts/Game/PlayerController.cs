using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEditor.Experimental.GraphView.GraphView;

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
    float speed;
    public float Speed { get { return speed; } set { speed = value; } }
    public float defaultSpeed { get; private set; } = 5;
    public float jumpPower;
    #endregion

    private void Start()
    {
        speed = defaultSpeed;
        animController = GetComponent<PlayerAnimatorController>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // �L�[���͂ňړ��������X�V
        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");

        if (IsGround())
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
        }
    }

    private void FixedUpdate()
    {
        // �J�����̌����ƉE�����̑傫�����擾����
        Vector3 cameraRot = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);

        // �ړ��ʂ�ݒ�
        Vector3 setMove = (cameraRight * moveX + cameraRot * moveZ).normalized; // �傫�����P�ɂ���
        rb.velocity = new Vector3(setMove.x * speed, rb.velocity.y, setMove.z * speed); // �������x��������Ȃ��悤�ɂ���

        // ���炩�ɉ�]
        transform.forward = Vector3.Slerp(transform.forward, setMove, Time.deltaTime * 30f);   // ��]���x��������
    }

    bool IsGround()
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

    public void InitPlayer(Vector3 startPosition)
    {
        transform.position = startPosition;
    }
}
