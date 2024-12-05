using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCameraController : MonoBehaviour
{
    #region 競技『カントリーリレー』の各エリア毎のカメラ設定
    [SerializeField] List<Vector3> rotate;
    [SerializeField] List<Vector3> followOffset;
    #endregion

    [SerializeField] GameDirector gameDirector;
    CinemachineVirtualCamera camera;
    CinemachineTransposer cameraTransposer;

    public Guid currentTargetId { get; private set; }   // 現在追っているターゲット
    public int activeTargetCnt { get; private set; }    // 切り替えることができるターゲットの対象の数

    [SerializeField] int debug_areaId = 0;

    private void Awake()
    {
        camera = GetComponent<CinemachineVirtualCamera>();
        cameraTransposer = camera.GetCinemachineComponent<CinemachineTransposer>(); ;

        if(debug_areaId > 0)
        {
            InitCamera(null, debug_areaId,new Guid());
        }
    }

    public void InitCamera(Transform target,int areaId, Guid targetId)
    {
        if (target != null)
        {
            // 一瞬でターゲットの視点に入れ替える
            camera.Follow = null;
            camera.LookAt = null;
            transform.position = target.position + cameraTransposer.m_FollowOffset;

            // ターゲットの設定
            camera.Follow = target;
            camera.LookAt = target;
            currentTargetId = targetId;
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
        foreach (var character in gameDirector.characterList)
        {
            if (!isSucsess && character.Key != RoomModel.Instance.ConnectionId
                && currentTargetId != character.Key && character.Value.activeSelf)
            {
                // カメラのターゲット切り替え
                InitCamera(character.Value.transform, 0, character.Key);

                isSucsess = true;
            }

            if (character.Value.activeSelf) activeTargetCnt++;
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
