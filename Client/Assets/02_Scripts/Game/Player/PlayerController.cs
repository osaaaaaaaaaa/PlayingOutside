using Cinemachine;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    #region コンポーネント
    PlayerAnimatorController animController;
    PlayerSkillController skillController;
    PlayerItemController itemController;
    PlayerEffectController effectController;
    CinemachineImpulseSource impulseSource;
    CharacterControlUI characterControlUI;
    FloatingJoystick floatingJoystick;
    Rigidbody rb;
    public Rigidbody Rb { get { return rb; } }
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
    float defaultSpeed = 5f;
    public float DefaultSpeed { get { return defaultSpeed; } set { defaultSpeed = value; } }

    // 加算するスピード
    public float addPepperSpeed { get; private set; } = 3f;
    float addMudSpeed = -2f;

    float jumpPower;
    [SerializeField] float defaultJumpPower;

    #endregion
    #endregion

    Transform modelTf;

    // 復活する座標
    public Vector3 respawnPoint { get; private set; } = Vector3.zero;

    // 他のプレイヤーのTransform
    [SerializeField] List<GameObject> objOtherPlayers = new List<GameObject>();

    // 立ち上がっているかどうか
    public bool isStandUp;
    public bool IsStandUp { get { return isStandUp; } set { isStandUp = value; } }

    // 無敵状態かどうか
    bool isInvincible;
    public bool IsInvincible { get { return isInvincible; }set { isInvincible = value; } }

    // 操作が可能かどうか
    public bool isControlEnabled = true;
    public bool IsControlEnabled { get { return isControlEnabled; }set { isControlEnabled = value; } }

    // マッハキックボタンを押しているかどうか
    bool isPressMachKickBtn;
    public bool IsPressMachKickBtn { get { return isPressMachKickBtn; } set { isPressMachKickBtn = value; } }

    bool isMudCollider;

    public bool isDebug;

    private void Awake()
    {
        skillController = GetComponent<PlayerSkillController>();
        animController = GetComponent<PlayerAnimatorController>();
        itemController = GetComponent<PlayerItemController>();
        effectController = GetComponent<PlayerEffectController>();
        rb = GetComponent<Rigidbody>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        var CharacterControls = GameObject.Find("CharacterControls");
        if(CharacterControls) characterControlUI = CharacterControls.GetComponent<CharacterControlUI>();
        var FloatingJoystick = GameObject.Find("Floating Joystick");
        if(FloatingJoystick) floatingJoystick = FloatingJoystick.GetComponent<FloatingJoystick>();
        modelTf = transform.GetChild(0);


        speed = defaultSpeed;
        jumpPower = defaultJumpPower;
        hp = hpMax;
        isStandUp = true;
        isInvincible = false;
        isMudCollider = false;
    }

    private void OnEnable()
    {
        if (characterControlUI != null && this.gameObject.layer == 3) characterControlUI.ToggleUIVisibiliity(true);
    }

    private void OnDisable()
    {
        if (characterControlUI != null && this.gameObject.layer == 3) characterControlUI.ToggleUIVisibiliity(false);
    }

    void Update()
    {
        if (!isStandUp || !isControlEnabled)
        {
            moveX = 0;
            moveZ = 0;
            characterControlUI.ToggleButtonInteractable(false);
            return;
        }

        // キー入力で移動方向を更新
        if (floatingJoystick && floatingJoystick.Horizontal != 0 && floatingJoystick.Vertical != 0)
        {
            moveX = floatingJoystick.Horizontal;
            moveZ = floatingJoystick.Vertical;
        }
        else
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveZ = Input.GetAxisRaw("Vertical");
        }

        bool isGround = GetComponent<PlayerIsGroundController>().IsGround();
        bool isMachAura = skillController.SkillId == PlayerSkillController.SKILL_ID.Skill3 && skillController.isUsedSkill;
        bool isInteractable = isGround && !skillController.isUsedSkill;
        if (isMachAura)
        {
            characterControlUI.ToggleButtonInteractable(isGround);
        }
        else
        {
            characterControlUI.ToggleButtonInteractable(isInteractable);
        }

        if (isInteractable || isMachAura && isGround)
        {
            if (moveX != 0 || moveZ != 0)
            {
                bool isUsePepper = itemController.itemEffectTimeList.ContainsKey(EnumManager.ITEM_ID.Pepper);
                if (!isUsePepper) animController.SetInt(PlayerAnimatorController.ANIM_ID.Run);
                if (isUsePepper) animController.SetInt(PlayerAnimatorController.ANIM_ID.RunFast);
            }
            else
            {
                // マッハキック中はアイドルアニメを再生しない
                if (animController.GetAnimId() != (int)PlayerAnimatorController.ANIM_ID.MachKick)
                {
                    if (isMachAura) animController.SetInt(PlayerAnimatorController.ANIM_ID.IdleA);
                    if (!isMachAura) animController.SetInt(PlayerAnimatorController.ANIM_ID.IdleB);
                }
            }

            // ↓とりあえず残しておく####################################################

            // ジャンプ処理
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (characterControlUI.IsSetupDone)
                    characterControlUI.ClickEvent(CharacterControlUI.ButtonType.jump);
                else OnJumpButton();
            }

            // スキル発動処理
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (characterControlUI.IsSetupDone)
                    characterControlUI.ClickEvent(CharacterControlUI.ButtonType.skill);
                else OnSkillButton();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (characterControlUI.IsSetupDone)
                    characterControlUI.ClickEvent(CharacterControlUI.ButtonType.item);
            }
        }

        // ↓とりあえず残しておく####################################################
        if (isInteractable || isMachAura)
        {
            // キック処理
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (characterControlUI.IsSetupDone)
                    characterControlUI.ClickEvent(CharacterControlUI.ButtonType.kick);
                else OnKickButton();
            }
        }
    }

    public void OnKickButton()
    {
        bool isMachAura = skillController.SkillId == PlayerSkillController.SKILL_ID.Skill3 && skillController.isUsedSkill;
        var target = SerchNearTarget();
        if (target != null) LookAtPlayer(target);
        if (isMachAura)
        {
            isPressMachKickBtn = true;
            animController.Animator.SetInteger("animation", (int)PlayerAnimatorController.ANIM_ID.MachKick);
        }
        else
        {
            animController.SetInt(PlayerAnimatorController.ANIM_ID.Kick);
        }
    }

    public void OnJumpButton()
    {
        Jump(jumpPower);
    }

    public void OnSkillButton()
    {
        animController.SetInt(animController.GetSkillAnimId());
        characterControlUI.OnSkillButton();
    }

    public void Jump(float _jumpPower)
    {
        transform.position += Vector3.up * GetComponent<PlayerIsGroundController>().rayHeight;
        animController.SetInt(PlayerAnimatorController.ANIM_ID.Jump);
        rb.AddForce(transform.up * _jumpPower);
    }

    private void FixedUpdate()
    {
        if (moveX == 0 && moveZ == 0 || !isStandUp || !isControlEnabled) return;

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
        else if(other.gameObject.tag == "Checkpoint")
        {
            respawnPoint = other.transform.position;
        }
    }

    /// <summary>
    /// ダメージを受ける処理
    /// </summary>
    public void Hit(int damage, Vector3 specifiedKnockback, Transform tf)
    {
        if (hp <= 0 || !isStandUp || isInvincible) return;

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
    public async void KnockBackAndDown(Vector3 knockBackVec)
    {
        if (!isStandUp || isInvincible) return;
        hp = hpMax;
        transform.gameObject.layer = 8; // ノックダウン状態にする

        // ノックバック演出
        transform.position += Vector3.up * GetComponent<PlayerIsGroundController>().rayHeight;
        animController.PlayKnockBackAnim();
        rb.AddForce(knockBackVec, ForceMode.Impulse);

        // コイン(ポイント)のドロップ処理
        if(!isDebug && SceneManager.GetActiveScene().name == "FinalGameScene") await RoomModel.Instance.KnockDownAsynk(transform.position);
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

    public void OnColliderMudEnter()
    {
        if (isMudCollider) return;
        speed += addMudSpeed;
        jumpPower = defaultJumpPower / 1.5f;
        isMudCollider = true;
    }

    public void OnColliderMudExit()
    {
        if (!isMudCollider) return;
        speed -= addMudSpeed;
        jumpPower = defaultJumpPower;
        isMudCollider = false;
    }

    void InitParam()
    {
        // アイテム効果・パーティクルリセット
        itemController.ClearAllItemEffects();
        effectController.ClearAllParticles();

        rb.drag = 0;
        IsStandUp = true;
        IsControlEnabled = true;
        IsInvincible = false;
        isMudCollider = false;
        this.gameObject.layer = 3;
        defaultSpeed = 5;
        speed = defaultSpeed;
        jumpPower = defaultJumpPower;
        GetComponent<PlayerAudioController>().ResetRunningSourse(PlayerAudioController.AudioClipName.running_default);
    }

    public void Respawn()
    {
        transform.position = respawnPoint;
        InitParam();
    }

    /// <summary>
    /// 初期設定などに呼び出し
    /// </summary>
    /// <param name="startPointTf"></param>
    public void InitPlayer(Transform startPointTf)
    {
        respawnPoint = startPointTf.position;
        transform.position = startPointTf.position;
        transform.eulerAngles += startPointTf.eulerAngles;
        modelTf.localPosition = Vector3.zero;

        InitParam();
    }

    /// <summary>
    /// アニメーション終了時などに呼び出し
    /// </summary>
    public void InitPlayer()
    {
        modelTf.localPosition = Vector3.zero;
        this.gameObject.layer = 3;
        isControlEnabled = true;
        rb.drag = 0;

        speed = defaultSpeed;
        jumpPower = defaultJumpPower;
        if (itemController.itemEffectTimeList.ContainsKey(EnumManager.ITEM_ID.Pepper)) speed += addPepperSpeed;
        if (isMudCollider)
        {
            speed += addMudSpeed;
            jumpPower = defaultJumpPower / 1.5f;
        }
    }
}
