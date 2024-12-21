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
    #region コンポーネント
    PlayerAnimatorController animController;
    PlayerSkillController skillController;
    Rigidbody rb;
    CinemachineImpulseSource impulseSource;
    #endregion

    #region カメラ関係
    const float minShakeVecCamera = 0.5f;
    const float maxShakeVecCamera = 2f;
    #endregion

    #region プレイヤーのステータス
    float moveX;
    float moveZ;

    public int hpMax;
    public int hp;

    public float rangeKick;

    #region ノックバックする力
    public float addKnockBackPower = 5;
    public float correctionKnockBackPowerY = 2f;
    public float defaultKnockBackPower1 = 5;
    public float defaultKnockBackPower2 = 15;
    #endregion

    #region スピード・ジャンプ
    public float speed;
    public float Speed { get { return speed; } set { speed = value; } }
    public float defaultSpeed { get; private set; } = 5;
    float jumpPower;
    #endregion
    #endregion

    // 他のプレイヤーのTransform
    [SerializeField] List<GameObject> objOtherPlayers = new List<GameObject>();

    // 無敵状態かどうか
    bool isInvincible;
    public bool IsInvincible { get { return isInvincible; }set { isInvincible = value; } }

    // 操作が可能かどうか
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

        // キー入力で移動方向を更新
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

            // ジャンプ処理
            if (Input.GetKeyDown(KeyCode.Space))
            {
                transform.position += Vector3.up * GetComponent<PlayerIsGroundController>().rayHeight;
                animController.SetInt(PlayerAnimatorController.ANIM_ID.Jump);
                rb.AddForce(transform.up * jumpPower);
            }

            // キック処理
            if (Input.GetMouseButtonDown(0))
            {
                var target = SerchNearTarget();
                if(target != null) LookAtPlayer(target);
                animController.SetInt(PlayerAnimatorController.ANIM_ID.Kick);
            }

            // スキル発動処理
            if (Input.GetKeyDown(KeyCode.E))
            {
                animController.SetInt(PlayerAnimatorController.ANIM_ID.Skill);
            }
        }
    }

    private void FixedUpdate()
    {
        if (moveX == 0 && moveZ == 0 || !animController.isStandUp || !isControlEnabled) return;

        // カメラの向きと右方向の大きさを取得する
        Vector3 cameraRot = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);

        // 移動量を設定
        Vector3 setMove = (cameraRight * moveX + cameraRot * moveZ).normalized; // 大きさを１にする
        rb.velocity = new Vector3(setMove.x * speed, rb.velocity.y, setMove.z * speed); // 落下速度をいじらないようにする

        // 滑らかに回転
        transform.forward = Vector3.Slerp(transform.forward, setMove, Time.deltaTime * 30f);   // 回転速度をかける
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
    /// ダメージを受ける処理
    /// </summary>
    public void Hit(int damage, Vector3 specifiedKnockback, Transform tf)
    {
        if (hp <= 0 || !animController.isStandUp || isInvincible) return;

        // ダメージに乱数を加える
        damage += Random.Range(0, 10);

        hp -= damage;
        LookAtPlayer(tf);
        Vector3 knockBackVec = (this.transform.position - tf.position).normalized;
         rb.velocity = Vector3.zero;

        if (hp <= 0 || specifiedKnockback != Vector3.zero)
        {
            // ノックバックするベクトルが指定されていない場合
            if(specifiedKnockback == Vector3.zero)
            {
                // ダメージ量に応じてノックバックする力が変化
                float addPower = (float)damage / hpMax * addKnockBackPower;
                float resultPower = defaultKnockBackPower2 + addPower;
                if (resultPower > defaultKnockBackPower2 + addKnockBackPower) resultPower = defaultKnockBackPower2 + addKnockBackPower;
                if (resultPower < defaultKnockBackPower2) resultPower = defaultKnockBackPower2;

                // 大きくノックバックする
                knockBackVec *= resultPower;
                knockBackVec = new Vector3(knockBackVec.x, resultPower / correctionKnockBackPowerY, knockBackVec.z);
            }
            else
            {
                knockBackVec = specifiedKnockback;
            }

            KnockBackAndDown(knockBackVec);

            // カメラをダメージ量に応じて揺らす
            float shakePower = (float)damage / hpMax * (maxShakeVecCamera - minShakeVecCamera);
            shakePower += minShakeVecCamera;
            if(shakePower > maxShakeVecCamera) shakePower = maxShakeVecCamera;
            if(shakePower < minShakeVecCamera) shakePower = minShakeVecCamera;

            impulseSource.m_DefaultVelocity = Vector3.one * shakePower;
            impulseSource.GenerateImpulse();
        }
        else
        {
            // ダメージ量に応じてノックバックする力が変化
            float addPower = (float)damage / hpMax * addKnockBackPower;
            float resultPower = defaultKnockBackPower1 + addPower;
            if (resultPower > defaultKnockBackPower1 + addKnockBackPower) resultPower = defaultKnockBackPower1 + addKnockBackPower;
            if (resultPower < defaultKnockBackPower1) resultPower = defaultKnockBackPower1;

            // 軽くノックバックする
            knockBackVec *= resultPower;
            knockBackVec = new Vector3(knockBackVec.x, resultPower / correctionKnockBackPowerY, knockBackVec.z);
            rb.AddForce(knockBackVec,ForceMode.Impulse);

            animController.SetInt(PlayerAnimatorController.ANIM_ID.Damage);
        }
    }

    /// <summary>
    /// ターゲットの方向を向く 
    /// </summary>
    /// <param name="target"></param>
    void LookAtPlayer(Transform target)
    {
        // ターゲットへの向きベクトル計算
        var dir = target.position - transform.position;

        // ターゲットの方向への回転
        var lookAtRotation = Quaternion.LookRotation(dir, Vector3.up);
        // 回転補正
        var angles = Vector3.Scale(lookAtRotation.eulerAngles, Vector3.up); // プレイヤーのピボットが足元のためY以外は0にする

        // ターゲットの方向を向く
        transform.eulerAngles = angles;
    }

    /// <summary>
    /// ノックバックしつつダウンする処理
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
    /// 一番近いターゲットを取得
    /// </summary>
    Transform SerchNearTarget()
    {
        if(objOtherPlayers.Count == 0)
        {
            // まだ他のプレイヤーを取得していない場合は取得する
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
