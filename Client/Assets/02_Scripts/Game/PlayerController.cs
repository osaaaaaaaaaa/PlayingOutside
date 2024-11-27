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
        // キー入力で移動方向を更新
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

        // ジャンプ処理
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    animController.SetInt(PlayerAnimatorController.ANIM_ID.Jump);
        //    rb.AddForce(transform.up * jump);
        //}
    }

    private void FixedUpdate()
    {
        // カメラの向きと右方向の大きさを取得する
        Vector3 cameraRot = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);

        // 移動量を設定
        Vector3 setMove = (cameraRight * moveX + cameraRot * moveZ).normalized; // 大きさを１にする
        rb.velocity = setMove * speed;

        // 滑らかに回転
        transform.forward = Vector3.Slerp(transform.forward, setMove, Time.deltaTime * 30f);   // 回転速度をかける
    }

    public void InitPlayer(Vector3 startPosition)
    {
        transform.position = startPosition;
    }
}
