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
        // カーソルを非表示にしてロック
        Cursor.lockState = CursorLockMode.Locked;

        // カメラの初期Y位置を保存
        initialY = transform.position.y;

        // 初期角度を保存
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        // WASDキーによるカメラの移動（Y軸は固定）
        float moveForward = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float moveRight = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;

        Vector3 move = transform.right * moveRight + transform.forward * moveForward;
        move.y = 0; // Y軸の移動を固定

        transform.position += move;

        // マウスによるカメラの回転
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f); // ピッチの回転角度を制限

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}
