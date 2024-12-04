using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalResultSceneDirector : MonoBehaviour
{
    #region キャラクター関係
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();  // ユーザーのキャラクター情報
    #endregion

    [SerializeField] FinalResultSceneParticleController particleController;
    [SerializeField] TotalScoreUIController totalScoreUIController;
    [SerializeField] GameObject crownPrefab;
    [SerializeField] GameObject btnLeave;

    const float waitSeconds = 0.1f;

    private void Start()
    {
        // 関数を登録する
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnTransitionFinalResultSceneUser += this.NotifyTransitionFinalResultSceneAllUsers;

        SetupScene();

        // 最終結果発表シーンに遷移完了リクエスト
        TransitionFinalResultScene();
    }

    void OnDisable()
    {
        // シーン遷移時に関数の登録を解除
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            UpdatePlayerState();
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    void SetupScene()
    {
        GenerateCharacters();

        // ロード画面を閉じる
        SceneControler.Instance.StopSceneLoad();
    }

    /// <summary>
    /// キャラクター生成処理
    /// </summary>
    void GenerateCharacters()
    {
        var users = RoomModel.Instance.JoinedUsers;

        foreach (var user in users)
        {
            // キャラクター生成,
            GameObject character = Instantiate(characterPrefab);
            characterList[user.Key] = character;

            // プレイヤーの初期化処理
            bool isMyCharacter = user.Key == RoomModel.Instance.ConnectionId;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.Value.JoinOrder - 1]);

            // ユーザー名の初期化処理
            Color colorText = isMyCharacter ? Color.white : Color.green;
            character.GetComponent<PlayerUIController>().InitUI(user.Value.UserData.Name, colorText);

            // 全員がシーン遷移完了するまでPlayerControllerを外す
            character.GetComponent<PlayerController>().enabled = false;

            // レイヤータグを変更
            character.layer = isMyCharacter ? 3 : 7;
        }
    }

    /// <summary>
    /// 退室リクエスト
    /// </summary>
    public async void LeaveRoom()
    {
        StopCoroutine(UpdateCoroutine());
        await RoomModel.Instance.LeaveAsync();

        SceneControler.Instance.StartSceneLoad("TopScene");
    }

    /// <summary>
    /// 退室通知処理
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            // 自分が退出する場合は全て削除
            foreach (var character in characterList.Values)
            {
                Destroy(character);
            }
            characterList.Clear();
        }
        else
        {
            // 該当のキャラクター削除&リストから削除
            Destroy(characterList[connectionId]);
            characterList.Remove(connectionId);
        }
    }

    /// <summary>
    /// プレイヤー情報更新リクエスト
    /// </summary>
    public async void UpdatePlayerState()
    {
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // 自分の存在チェック
        var character = characterList[RoomModel.Instance.ConnectionId];
        PlayerState playerState = new PlayerState()
        {
            position = character.transform.position,
            angle = character.transform.eulerAngles,
            animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
            isActiveSelf = true,
        };
        await RoomModel.Instance.UpdatePlayerStateAsync(playerState);
    }

    /// <summary>
    /// プレイヤー情報更新通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedPlayerState(Guid connectionId, PlayerState playerState)
    {
        if (!characterList.ContainsKey(connectionId)) return;   // プレイヤーの存在チェック

        // 移動・回転・アニメーション処理
        characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

    /// <summary>
    /// 最終結果発表シーンに遷移した処理
    /// </summary>
    async void TransitionFinalResultScene()
    {
        await RoomModel.Instance.OnTransitionFinalResultSceneAsynk();
    }

    /// <summary>
    /// 全員が遷移できた通知
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="playerState"></param>
    void NotifyTransitionFinalResultSceneAllUsers(ResultData[] result)
    {
        StartCoroutine(UpdateCoroutine());

        Guid winnerId = new Guid();
        foreach (ResultData resultData in result)
        {
            if (resultData.rank == 1)
            {
                winnerId = resultData.connectionId;
                break;
            }
        }
        StartCoroutine(ShowResultsCoroutine(result, winnerId));
    }

    /// <summary>
    /// 結果発表開始コルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowResultsCoroutine(ResultData[] result,Guid winnerId)
    {
        int[] scores = new int[result.Length];
        for (int i = 0; i < result.Length; i++) 
        {
            scores[i] = result[i].score;
        }
        totalScoreUIController.Init(characterList.Count, scores);
        yield return new WaitForSeconds(1f);

        totalScoreUIController.PlayAnim();
        yield return new WaitForSeconds(3f);

        totalScoreUIController.StopAnim();
        yield return new WaitForSeconds(0.5f);

        // １位のプレイヤーに王冠をセットする
        Instantiate(crownPrefab, characterList[winnerId].transform);
        yield return new WaitForSeconds(1f);    // 王冠のアニメーションが終了する時間

        particleController.GenerateParticles(characterList[winnerId].transform);
        yield return new WaitForSeconds(0.5f);

        // プレイヤーの操作をできるようにする
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        // 退室ができるようにする
        btnLeave.SetActive(true);
    }
}
