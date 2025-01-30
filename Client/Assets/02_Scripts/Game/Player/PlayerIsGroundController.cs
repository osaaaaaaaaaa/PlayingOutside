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

            // 着地時のエフェクト生成
            GetComponent<PlayerEffectController>().SetEffect(PlayerEffectController.EFFECT_ID.Landing);
        }
        else if (triggerIsGround && !IsGround()) 
        {
            triggerIsGround = false;
        }
    }

    /// <summary>
    /// 地面判定用
    /// </summary>
    /// <returns></returns>
    public bool IsGround()
    {
        Vector3 basePos = transform.position;    // ピボットが中心にあるため調整する
        Vector3 leftStartPos = basePos - Vector3.right * rayWeight;      // 左側の始点
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
}
