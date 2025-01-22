using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class Goose : MonoBehaviour
{
    [SerializeField] LayerMask groundRayer;
    [SerializeField] float attackDis;
    [SerializeField] float trackingDis;
    [SerializeField] float startRunDis;
    [SerializeField] float speed;
    public float rayHeight = 0.2f;
    public float rayWeight = 0.4f;

    Rigidbody rb;
    Animator animator;
    public List<GameObject> targetList = new List<GameObject>();

    GameObject currentTarget;
    [SerializeField] GameObject startPoint;
    float moveX;
    float moveZ;
    bool isControllEnable;

    public enum ANIM_ID
    {
        idle = 0,
        attack,
        walk,
        run
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator =transform.GetChild(0).GetComponent<Animator>();
        isControllEnable = false;

        var characters = GameObject.FindGameObjectsWithTag("Character");
        if(characters != null)
        {
            targetList = new List<GameObject>(characters);
        }
    }

    private void Update()
    {
        // �i�s�����ɒn�ʂ��Ȃ���ΊJ�n�ʒu�ɖ߂�
        if (!IsGround())
        {
            var moveVec = GetMoveVec(startPoint.transform.position);
            currentTarget = startPoint;
            moveX = moveVec.x;
            moveZ = moveVec.z;
            return;
        }

        if (currentTarget == null)
        {
            foreach (GameObject target in targetList)
            {
                if (target.gameObject.layer == 8) continue;

                if (currentTarget == null)
                {
                    currentTarget = target;
                }
                else
                {
                    float targetDis = Mathf.Abs(Vector3.Distance(transform.position, currentTarget.transform.position));
                    float dis = Mathf.Abs(Vector3.Distance(transform.position, target.transform.position));
                    if (dis < targetDis)
                    {
                        currentTarget = target;
                    }
                }
            }
        }

        if (currentTarget != null)
        {
            // �ǐՔ͈͂��痣�ꂽ || �m�b�N�_�E����Ԃɂ���ꍇ
            float distance = Mathf.Abs(Vector3.Distance(transform.position, currentTarget.transform.position));
            if (distance > trackingDis || currentTarget.gameObject.layer == 8)
            {
                currentTarget = null;
                SetAnimId(ANIM_ID.idle);
                return;
            }

            // �^�[�Q�b�g�Ƃ̋������˒������̏ꍇ
            if (distance <= attackDis)
            {
                // �X�^�[�g�n�_�ւ��ǂ�r���Ȃ�
                if (currentTarget == startPoint)
                {
                    currentTarget = null;
                }
                else
                {
                    Attack();
                }
                
                moveX = 0;
                moveZ = 0;
            }
        }
        else
        {
            SetAnimId(ANIM_ID.idle);
        }
    }

    private void FixedUpdate()
    {
        if (isControllEnable || moveX == 0 && moveZ == 0) return;

        float distance = Mathf.Abs(Vector3.Distance(transform.position, currentTarget.transform.position));

        float trackingSpeed = speed;
        if (distance <= startRunDis && currentTarget != startPoint)
        {
            trackingSpeed = speed * 1.5f;
            SetAnimId(ANIM_ID.run);
        }
        else
        {
            SetAnimId(ANIM_ID.walk);
        }

        rb.velocity = new Vector3(moveX * trackingSpeed, rb.velocity.y, moveZ * trackingSpeed);
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
        animator.SetInteger("animation_id", (int)id);
    }

    public void InitParam()
    {
        isControllEnable = false;
        SetAnimId(ANIM_ID.idle);
    }

    bool IsGround()
    {
        Vector3 basePos = transform.position + transform.forward.normalized * rayWeight;    // �s�{�b�g�����S�ɂ��邽�ߒ�������

        Vector3 forwardUpPos = basePos + Vector3.up * rayHeight;
        Vector3 forwardDownPos = basePos + Vector3.down * rayHeight;

        Debug.DrawLine(forwardUpPos, forwardDownPos, Color.red);

        return Physics.Linecast(forwardUpPos, forwardDownPos, groundRayer);
    }
}
