using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Playables;

public class Goose : MonoBehaviour
{
    [SerializeField] LayerMask groundRayer;
    [SerializeField] float attackDis;
    [SerializeField] float trackingDis;
    [SerializeField] float startRunDis;
    [SerializeField] float speed;
    [SerializeField] float rayHeight = 2f;
    [SerializeField] float rayWeight = 2.5f;
    [SerializeField] AudioClip walkSE;
    [SerializeField] AudioClip runSE;

    Rigidbody rb;
    AudioSource audioSource;
    Animator animator;
    public List<GameObject> targetList = new List<GameObject>();

    GameObject currentTarget;
    Vector3 startPos;
    float moveX;
    float moveZ;
    bool isControllEnable;
    public bool IsControllEnable { set {  isControllEnable = value; } }
    bool isInitMember;

    public enum ANIM_ID
    {
        idle = 0,
        attack,
        walk,
        run
    }

    private void OnEnable()
    {
        InitMember();
    }

    public void InitMember()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        startPos = transform.position;
        isControllEnable = false;

        var characters = FindObjectsOfType<GameObject>(true);
        foreach (var character in characters)
        {
            if (character.tag == "Character")
            {
                targetList.Add(character);
            }
        }
    }

    private void Update()
    {
        if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
        {
            if (!RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient)
            {
                moveX = 0;
                moveZ = 0;
                return;
            }
        }

        if (currentTarget == null)
        {
            foreach (GameObject target in targetList)
            {
                if (target == null) return;

                // �ǐՔ͈͊O,�^�[�Q�b�g�����G���,�^�[�Q�b�g�Ƃ̊Ԃɒn�ʂ��Ȃ��ꍇ
                float targetDis = Mathf.Abs(Vector3.Distance(transform.position, target.transform.position));
                float disY = Mathf.Abs(transform.position.y - target.transform.position.y);
                if (targetDis > trackingDis || disY > 1 
                    || target.GetComponent<PlayerController>().IsInvincible 
                    || !IsGround(target.transform.position)) continue;

                if (currentTarget == null)
                {
                    currentTarget = target;
                }
                else
                {
                    float currentDis = Mathf.Abs(Vector3.Distance(transform.position, currentTarget.transform.position));
                    if (targetDis < currentDis)
                    {
                        currentTarget = target;
                    }
                }
            }
        }

        if (currentTarget != null)
        {
            // �ǐՔ͈͂��痣�ꂽ || ��ɒn�ʂ��Ȃ� || ���G��Ԃ̏ꍇ
            float distance = Mathf.Abs(Vector3.Distance(transform.position, currentTarget.transform.position));
            if (distance > trackingDis || !IsGround(currentTarget.transform.position) || currentTarget.GetComponent<PlayerController>().IsInvincible)
            {
                currentTarget = null;
                moveX = 0;
                moveZ = 0;
                SetAnimId(ANIM_ID.idle);
                return;
            }

            // �^�[�Q�b�g�Ƃ̋������˒������̏ꍇ
            if (distance <= attackDis)
            {
                Attack();
                moveX = 0;
                moveZ = 0;
            }
            else
            {
                var setMove = GetMoveVec(currentTarget.transform.position);
                moveX = setMove.x;
                moveZ = setMove.z;
            }
        }
        else
        {
            float startPosDis = Mathf.Abs(Vector3.Distance(transform.position, startPos));
            if(startPosDis > 1f)
            {
                var setMove = GetMoveVec(startPos);
                moveX = setMove.x;
                moveZ = setMove.z;
            }
            else
            {
                moveX = 0;
                moveZ = 0;
                SetAnimId(ANIM_ID.idle);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isControllEnable || moveX == 0 && moveZ == 0) return;

        float trackingSpeed = speed;
        ANIM_ID playAnimId = ANIM_ID.walk;

        if (currentTarget != null)
        {
            float distance = Mathf.Abs(Vector3.Distance(transform.position, currentTarget.transform.position));
            if (distance <= startRunDis)
            {
                trackingSpeed = speed * 1.5f;
                playAnimId = ANIM_ID.run;
            }
        }

        DOMove(playAnimId, trackingSpeed);
    }

    void DOMove(ANIM_ID playAnimId, float trackingSpeed)
    {
        SetAnimId(playAnimId);
        transform.forward = Vector3.Slerp(transform.forward, new Vector3(moveX, 0, moveZ), Time.deltaTime * 30f);   // ��]���x��������
        rb.velocity = new Vector3(moveX * trackingSpeed, rb.velocity.y, moveZ * trackingSpeed);
    }

    public void UpdateState(GooseState gooseState, float animSec)
    {
        SetAnimId(GetEnumAnimId(gooseState.animationId));
        transform.DOMove(gooseState.position, animSec).SetEase(Ease.Linear);
        transform.DORotate(gooseState.angle, animSec).SetEase(Ease.Linear);
    }

    ANIM_ID GetEnumAnimId(int animId)
    {
        switch (animId)
        {
            case (int)ANIM_ID.idle:
                return ANIM_ID.idle;
            case (int)ANIM_ID.attack:
                return ANIM_ID.attack;
            case (int)ANIM_ID.walk:
                return ANIM_ID.walk;
            case (int)ANIM_ID.run:
                return ANIM_ID.run;
            default:
                return ANIM_ID.idle;
        }
    }

    public int GetAnimationId()
    {
        return animator.GetInteger("animation_id");
    }

    Vector3 GetMoveVec (Vector3 targetPos)
    {
        Vector3 setMove = (targetPos - this.transform.position).normalized;
        setMove = new Vector3(setMove.x, 0, setMove.z);
        return setMove;
    }

    public void Attack()
    {
        // �^�[�Q�b�g�̕���������
        var dir = currentTarget.transform.position - transform.position;
        var lookAtRotation = Quaternion.LookRotation(dir, Vector3.up);
        var angles = Vector3.Scale(lookAtRotation.eulerAngles, Vector3.up); // �v���C���[�̃s�{�b�g�������̂���Y�ȊO��0�ɂ���
        transform.eulerAngles = angles;

        isControllEnable = true;
        SetAnimId(ANIM_ID.attack);
    }

    public void SetAnimId(ANIM_ID id)
    {
        switch (id) {
            case ANIM_ID.walk:
                if(audioSource.clip != walkSE)
                {
                    audioSource.Stop();
                    audioSource.clip = walkSE;
                    audioSource.Play();
                }
                break;
            case ANIM_ID.run:
                if (audioSource.clip != runSE)
                {
                    audioSource.Stop();
                    audioSource.clip = runSE;
                    audioSource.Play();
                }
                break;
            default:
                audioSource.Stop();
                break;
        }
        animator.SetInteger("animation_id", (int)id);
    }

    public void InitParam()
    {
        isControllEnable = false;
        SetAnimId(ANIM_ID.idle);
    }

    /// <summary>
    /// �����ƃ^�[�Q�b�g�Ƃ̊Ԃɒn�ʂ����邩�`�F�b�N
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    bool IsGround(Vector3 targetPos)
    {
        var dir = targetPos - transform.position;
        Vector3 basePos = transform.position + dir.normalized * rayWeight;
        Vector3 forwardUpPos = basePos + Vector3.up * rayHeight;
        Vector3 forwardDownPos = basePos + Vector3.down * rayHeight;

        Debug.DrawLine(forwardUpPos, forwardDownPos, Color.red);

        return Physics.Linecast(forwardUpPos, forwardDownPos, groundRayer);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<BoundaryAreaCollider>())
        {
            // ��O�ɗ������ꍇ
            transform.position = startPos;
        }
    }
}
