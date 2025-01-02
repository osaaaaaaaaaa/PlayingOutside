using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float mouseSensitivity = 100.0f;

    private float pitch = 0.0f;
    private float yaw = 0.0f;
    private float initialY;

    void Start()
    {
        // �J�[�\�����\���ɂ��ă��b�N
        Cursor.lockState = CursorLockMode.Locked;

        // �J�����̏���Y�ʒu��ۑ�
        initialY = transform.position.y;

        // �����p�x��ۑ�
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        // WASD�L�[�ɂ��J�����̈ړ��iY���͌Œ�j
        float moveForward = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float moveRight = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;

        Vector3 move = transform.right * moveRight + transform.forward * moveForward;
        move.y = 0; // Y���̈ړ����Œ�

        transform.position += move;

        // �}�E�X�ɂ��J�����̉�]
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f); // �s�b�`�̉�]�p�x�𐧌�

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}
