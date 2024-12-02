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
    #region 地面判定用
    [SerializeField] LayerMask groundRayer;
    const float rayHeight = 0.2f;
    const float rayWeight = 0.4f;
    #endregion

    #region コンポーネント
    PlayerAnimatorController animController;
    Rigidbody rb;
    #endregion

    #region プレイヤーのステータス
    float moveX;
    float moveZ;
    bool isInvincible;  // 無敵状態かどうか


    #region スピード・ジャンプ
    float speed;
    public float Speed { get { return speed; } set { speed = value; } }
    public float defaultSpeed { get; private set; } = 5;
    float jumpPower;
    public float JumpPower { get { return speed; } set { speed = value; } }
    public float defaultJumpPower { get; private set; } = 500;
    #endregion
    #endregion

    #region メッシュ関係
    [SerializeField] List<SkinnedMeshRenderer> skinnedMeshs;
    [SerializeField] MeshRenderer meshMain;
    #endregion

    private void Start()
    {
        isInvincible = false;
        speed = defaultSpeed;
        jumpPower = defaultJumpPower;
        animController = GetComponent<PlayerAnimatorController>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!animController.isStandUp) return;

        // キー入力で移動方向を更新
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

            // ジャンプ処理
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
        if (!animController.isStandUp) return;

        // カメラの向きと右方向の大きさを取得する
        Vector3 cameraRot = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);

        // 移動量を設定
        Vector3 setMove = (cameraRight * moveX + cameraRot * moveZ).normalized; // 大きさを１にする
        rb.velocity = new Vector3(setMove.x * speed, rb.velocity.y, setMove.z * speed); // 落下速度をいじらないようにする

        // 滑らかに回転
        transform.forward = Vector3.Slerp(transform.forward, setMove, Time.deltaTime * 30f);   // 回転速度をかける
    }

    public bool IsGround()
    {
        Vector3 basePos = transform.position;    // モンスターのピボットが中心にあるため調整する
        Vector3 leftStartPos= basePos - Vector3.right * rayWeight;      // 左側の始点
        Vector3 rightStartPos = basePos + Vector3.right * rayWeight;    // 右側の始点
        Vector3 forwardStartPos = basePos - Vector3.back * rayWeight;   // 前の始点
        Vector3 backStartPos = basePos + Vector3.back * rayWeight;      // 後ろの始点
        Vector3 endPosition = basePos - Vector3.up * rayHeight;     // 終点(下)

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
    /// ノックバックしつつダウンする処理
    /// </summary>
    /// <param name="knockBackVec"></param>
    public void KnockBackAndDown(Vector3 knockBackVec)
    {
        if (isInvincible) return;

        if (animController.isStandUp)
        {
            transform.position += Vector3.up * rayHeight;
            animController.PlayKnockBackAnim();
            rb.AddForce(knockBackVec, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 点滅処理
    /// </summary>
    /// <returns></returns>
    public IEnumerator FlashCoroutine()
    {
        isInvincible = true;

        float waitSec = 0.125f;
        for(float i = 0; i < 1; i += waitSec)
        {
            yield return new WaitForSeconds(waitSec);

            foreach(var meshs in skinnedMeshs)
            {
                meshs.enabled = !meshs.enabled;
            }
            meshMain.enabled = !meshMain.enabled;
        }

        isInvincible = false;
    }

    public void InitPlayer(Vector3 startPosition)
    {
        transform.position = startPosition;
    }
}
