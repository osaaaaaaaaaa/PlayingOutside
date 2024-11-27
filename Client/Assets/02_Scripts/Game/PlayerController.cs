using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    PlayerAnimatorController animController;
    Rigidbody rb;
    float moveX;
    float moveZ;
    const float speed = 5;
    public float jump;

    private void Start()
    {
        animController = GetComponent<PlayerAnimatorController>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // �L�[���͂ňړ��������X�V
        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");

        if(moveX != 0 || moveZ != 0)
        {
            animController.SetInt(PlayerAnimatorController.ANIM_ID.Run);
        }
        else
        {
            animController.SetInt(PlayerAnimatorController.ANIM_ID.IdleB);
        }

        // �W�����v����
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    animController.SetInt(PlayerAnimatorController.ANIM_ID.Jump);
        //    rb.AddForce(transform.up * jump);
        //}
    }

    private void FixedUpdate()
    {
        // �J�����̌����ƉE�����̑傫�����擾����
        Vector3 cameraRot = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);

        // �ړ��ʂ�ݒ�
        Vector3 setMove = (cameraRight * moveX + cameraRot * moveZ).normalized; // �傫�����P�ɂ���
        rb.velocity = setMove * speed;

        // ���炩�ɉ�]
        transform.forward = Vector3.Slerp(transform.forward, setMove, Time.deltaTime * 30f);   // ��]���x��������
    }

    public void InitPlayer(Vector3 startPosition)
    {
        transform.position = startPosition;
    }
}
