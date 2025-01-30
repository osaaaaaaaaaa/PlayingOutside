using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.TextCore.Text;

public class TargetCameraController : MonoBehaviour
{
    #region 競技『カントリーリレー』の各エリア毎のカメラ設定
    [SerializeField] List<Vector3> rotate;
    [SerializeField] List<Vector3> followOffset;
    #endregion

    [SerializeField] RelayGameDirector gameDirector;
    CinemachineVirtualCamera cameraVirtual;
    CinemachineTransposer cameraTransposer;

    int targetIndex;  // 現在追っているターゲットのインデックス番号
    public Guid currentTargetId { get; private set; }   // 現在追っているターゲットのKey
    public int activeTargetCnt { get; private set; }    // 切り替えることができるターゲットの対象の数

    [SerializeField] int debug_areaId = 0;


    private void Awake()
    {
        targetIndex = 0;
        cameraVirtual = GetComponent<CinemachineVirtualCamera>();
        cameraTransposer = cameraVirtual.GetCinemachineComponent<CinemachineTransposer>();

        if(gameDirector != null)
        {
            if (gameDirector.isDebug)
            {
                InitCamera(null, debug_areaId, new Guid());
            }
        }
    }

    IEnumerator ResetDamping(float defDampingX,float defDampingY, float defDampingZ)
    {
        yield return null;
        cameraTransposer.m_XDamping = defDampingX;
        cameraTransposer.m_YDamping = defDampingY;
        cameraTransposer.m_ZDamping = defDampingZ;
    }

    public void InitCamera(Transform target,int areaId, Guid targetId)
    {
        if (target != null)
        {
            // 元のDampingを保持
            float defDampingX = cameraTransposer.m_XDamping;
            float defDampingY = cameraTransposer.m_YDamping;
            float defDampingZ = cameraTransposer.m_ZDamping;

            // Dampingを一時的にリセット
            cameraTransposer.m_XDamping = 0;
            cameraTransposer.m_YDamping = 0;
            cameraTransposer.m_ZDamping = 0;

            // ターゲットを一旦解除して瞬時に移動
            cameraVirtual.Follow = null;
            cameraVirtual.LookAt = null;
            transform.position = target.position + cameraTransposer.m_FollowOffset;

            // ターゲットを再設定
            cameraVirtual.Follow = target;
            cameraVirtual.LookAt = target;
            currentTargetId = targetId;

            // 遅延実行でDampingを元に戻す
            StartCoroutine(ResetDamping(defDampingX, defDampingY, defDampingZ));
        }

        if (rotate.Count == 0 || followOffset.Count == 0 || areaId == 0) return;

        // 競技『カントリーリレー』で使用
        transform.eulerAngles = rotate[areaId];
        cameraTransposer.m_FollowOffset = followOffset[areaId];
    }

    /// <summary>
    /// カメラのターゲットの切り替え先を探す&&切り替える
    /// </summary>
    /// <returns></returns>
    public bool SearchAndChangeTarget()
    {
        bool isSucsess = false;
        activeTargetCnt = 0;

        // キャラクターのKeyを取得
        Guid[] guidCharacters = new Guid[gameDirector.characterList.Count];
        guidCharacters = gameDirector.characterList.Keys.ToArray();

        // ターゲットの検索開始
        int tmpTargetIndex = targetIndex;
        for (int i = 0; i < guidCharacters.Length; i++)
        {
            // keyを取得
            tmpTargetIndex++;
            tmpTargetIndex = tmpTargetIndex < guidCharacters.Length ? tmpTargetIndex : 0;
            Guid key = guidCharacters[tmpTargetIndex];

            if (!isSucsess && key != RoomModel.Instance.ConnectionId
                && currentTargetId != key && gameDirector.characterList[key].activeSelf)
            {
                // カメラのターゲット切り替え
                InitCamera(gameDirector.characterList[key].transform, 0, key);
                isSucsess = true;
                targetIndex = tmpTargetIndex;
            }

            if (gameDirector.characterList[key].activeSelf) activeTargetCnt++;
        }

        return isSucsess;
    }

    /// <summary>
    /// (自分を除く)他にターゲットとなるプレイヤーがいるかチェック
    /// </summary>
    /// <returns></returns>
    public bool IsOtherTarget()
    {
        bool isSucsess = false;
        activeTargetCnt = 0;
        foreach (var character in gameDirector.characterList)
        {
            if (!isSucsess && character.Key != RoomModel.Instance.ConnectionId
                && currentTargetId != character.Key && character.Value.activeSelf)
            {
                isSucsess = true;
            }

            if (character.Value.activeSelf) activeTargetCnt++;
        }

        return isSucsess;
    }
}
