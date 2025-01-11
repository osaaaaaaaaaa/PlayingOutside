using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using UnityEngine.Playables;

public class MoveSetRoot : MonoBehaviour
{
    [SerializeField] List<Vector3> root;
    [SerializeField] float animSec;
    [SerializeField] bool isSetLookAt;
    [SerializeField] bool isDebug;
    public Tween pathTween { get; private set; }
    float elapsedTime = 0;


    void Start()
    {
        var path = new Vector3[root.Count];
        for (int i = 0; i < path.Length; i++)
        {
            path[i] = root[i];
        }

        if (isSetLookAt)
        {
            pathTween = this.transform.DOLocalPath(path, animSec).SetLookAt(0.01f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        }
        else
        {
            pathTween = this.transform.DOLocalPath(path, animSec).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        }

        if (!isDebug)
        {
            pathTween.Pause();
            if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
            {
                if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient) pathTween.Play();
            }

        }
    }

    /// <summary>
    /// 再生再開処理
    /// </summary>
    /// <param name="_elapsedTime"></param>
    public void ResumeTween()
    {
        if(!pathTween.IsPlaying()) pathTween.Goto(elapsedTime, true);
    }

    /// <summary>
    /// 現在位置の同期(マスタークライアントから受信)
    /// </summary>
    /// <param name="movingObjectState"></param>
    /// <param name="animSec"></param>
    public void SetPotition(MovingObjectState movingObjectState, float animSec)
    {
        this.gameObject.SetActive(movingObjectState.isActiveSelf);
        this.gameObject.transform.DOMove(movingObjectState.position, animSec).SetEase(Ease.Linear);
        this.gameObject.transform.DORotate(movingObjectState.angle, animSec).SetEase(Ease.Linear);
        elapsedTime = movingObjectState.elapsedTimeTween;
    }
}
