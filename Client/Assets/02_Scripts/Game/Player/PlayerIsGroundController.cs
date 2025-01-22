using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIsGroundController : MonoBehaviour
{
    [SerializeField] LayerMask groundRayer;
    public float rayHeight { get; private set; } = 0.2f;
    float rayWeight = 0.4f;
    bool triggerIsGround;

    private void Update()
    {
        if (!triggerIsGround && IsGround())
        {
            triggerIsGround = true;

            // ���n���̃G�t�F�N�g����
            GetComponent<PlayerEffectController>().SetEffect(PlayerEffectController.EFFECT_ID.Landing);
        }
        else if (triggerIsGround && !IsGround()) 
        {
            triggerIsGround = false;
        }
    }

    /// <summary>
    /// �n�ʔ���p
    /// </summary>
    /// <returns></returns>
    public bool IsGround()
    {
        Vector3 basePos = transform.position;    // �s�{�b�g�����S�ɂ��邽�ߒ�������
        Vector3 leftStartPos = basePos - Vector3.right * rayWeight;      // �����̎n�_
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
}
