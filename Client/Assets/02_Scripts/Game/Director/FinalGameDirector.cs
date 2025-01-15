using Cinemachine;
using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using static AreaController;
using static UnityEngine.Rendering.DebugUI;

public class FinalGameDirector : MonoBehaviour
{
    #region UI関係
    [SerializeField] GameStartCountDown gameStartCountDown;
    [SerializeField] GameObject countDownUI;
    [SerializeField] GameObject finishUI;
    [SerializeField] CharacterControlUI characterControlUI;
    [SerializeField] UserScoreController userScoreController;
    #endregion

    #region キャラクター情報
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] List<GameObject> characterPrefabList;
    public Dictionary<Guid, GameObject> characterList { get; private set; } = new Dictionary<Guid, GameObject>();  // ユーザーのキャラクター情報
    #endregion

    #region カメラ関係
    [SerializeField] CinemachineTargetGroup targetGroup;
    #endregion

    #region コントローラー関係
    [SerializeField] ItemSpawner itemSpawner;
    #endregion

    [SerializeField] GameObject coinPrefab;

    #region MoveRootスクリプトを適用しているギミック
    [SerializeField] GameObject gimmicksParent;
    [SerializeField] List<GameObject> movingObjects;
    Dictionary<string,MoveSetRoot> movingObjectList = new Dictionary<string, MoveSetRoot>();
    #endregion

    Dictionary<string,GameObject> itemList = new Dictionary<string, GameObject>();

    Coroutine coroutineCountDown;
    int currentTime;
    bool isGameStartCountDownOver;

    const float waitSeconds = 0.1f;

    public bool isDebug = false;

    private void Start()
    {
        if (isDebug) return;
        isGameStartCountDownOver = false;

        // 関数を登録する
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnUpdateMasterClientUser += this.NotifyUpdatedMasterClient;
        RoomModel.Instance.OnCountdownOverUser += this.NotifyStartGame;
        RoomModel.Instance.OnAfterFinalGameUser += this.NotifyAfterFinalGameUser;
        RoomModel.Instance.OnDropCoinsUser += this.NotifyDropCoinsUser;
        RoomModel.Instance.OnDropCoinsAtRandomPositionsUser += this.NotifyDropCoinsAtRandomPositions;
        #region ゲーム共通の通知処理
        RoomModel.Instance.OnUpdateScoreUser += this.NotifyUpdateScore;
        RoomModel.Instance.OnStartCountDownUser += this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser += this.NotifyCountDownUser;
        RoomModel.Instance.OnGetItemUser += this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser += this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser += this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser += this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser += this.NotifySpawnObjectUser;
        #endregion

        SetupGame();
    }

    void OnDisable()
    {
        // シーン遷移時に関数の登録を解除
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnUpdateMasterClientUser -= this.NotifyUpdatedMasterClient;
        RoomModel.Instance.OnCountdownOverUser -= this.NotifyStartGame;
        RoomModel.Instance.OnAfterFinalGameUser -= this.NotifyAfterFinalGameUser;
        RoomModel.Instance.OnDropCoinsUser -= this.NotifyDropCoinsUser;
        RoomModel.Instance.OnDropCoinsAtRandomPositionsUser -= this.NotifyDropCoinsAtRandomPositions;
        #region ゲーム共通の通知処理
        RoomModel.Instance.OnUpdateScoreUser -= this.NotifyUpdateScore;
        RoomModel.Instance.OnStartCountDownUser -= this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser -= this.NotifyCountDownUser;
        RoomModel.Instance.OnGetItemUser -= this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser -= this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser -= this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser -= this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser -= this.NotifySpawnObjectUser;
        #endregion
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient)
            {
                UpdateMasterClientAsynk();
            }
            else
            {
                UpdatePlayerState();
            }
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    IEnumerator CountDownCoroutine()
    {
        if (currentTime == 0) currentTime = 61;
        while (currentTime > 0)
        {
            currentTime--;
            OnCountDown();
            yield return new WaitForSeconds(1f);
        }
    }

    void SetupGame()
    {
        GenerateCharacters();

        // 動くオブジェクトを設定
        foreach(var item in movingObjects)
        {
            movingObjectList.Add(item.name, item.GetComponent<MoveSetRoot>());
        }

        // カメラのターゲットグループを設定する
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[characterList.Count];
        int i = 0;
        foreach (var target in characterList.Values)
        {
            targetGroup.m_Targets[i] = new CinemachineTargetGroup.Target()
            {
                target = target.transform,
                weight = 1,
                radius = 1,
            };
            i++;
        }

        // ロード画面を閉じる
        SceneControler.Instance.StopSceneLoad();

        // 数秒後にカウントダウンが開始
        gameStartCountDown.CallPlayAnim();
    }

    /// <summary>
    /// キャラクター生成処理
    /// </summary>
    void GenerateCharacters()
    {
        var users = RoomModel.Instance.JoinedUsers;

        foreach (var user in users)
        {
            var value = user.Value;

            // キャラクター生成,
            GameObject character = Instantiate(characterPrefabList[value.UserData.Character_Id - 1]);
            characterList[user.Key] = character;
            character.name = value.UserData.Name;

            // プレイヤーの初期化処理
            bool isMyCharacter = user.Key == RoomModel.Instance.ConnectionId;
            Vector3 startPos = characterStartPoints[value.JoinOrder - 1].position;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[value.JoinOrder - 1]);
            character.GetComponent<AudioListener>().enabled = isMyCharacter;

            // ユーザー名の初期化処理
            Color colorText = isMyCharacter ? Color.white : Color.green;
            character.GetComponent<PlayerUIController>().InitUI(value.UserData.Name, colorText);

            // レイヤータグを変更
            character.layer = isMyCharacter ? 3 : 7;
            // ゲームが開始するまではPlayerControllerを外す
            character.GetComponent<PlayerController>().enabled = false;

            if (isMyCharacter)
            {
                characterControlUI.SetupButtonEvent(character);
            }

            userScoreController.InitUserScoreList(value.JoinOrder, value.UserData.Character_Id - 1, value.UserData.Name, value.score);
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

        if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient)
        {
            foreach(var obj in movingObjectList)
            {
                obj.Value.ResumeTween();
            }
        }
    }

    /// <summary>
    /// プレイヤー情報更新リクエスト
    /// </summary>
    public async void UpdatePlayerState()
    {
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // プレイヤーの存在チェック
        var character = characterList[RoomModel.Instance.ConnectionId];
        if (character.GetComponent<PlayerController>().enabled)
        {
            PlayerState playerState = new PlayerState()
            {
                position = character.transform.position,
                angle = character.transform.eulerAngles,
                animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
                isActiveSelf = character.activeSelf,
            };
            await RoomModel.Instance.UpdatePlayerStateAsync(playerState);
        }
    }

    /// <summary>
    /// プレイヤー情報更新通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedPlayerState(Guid connectionId, PlayerState playerState)
    {
        if (!isGameStartCountDownOver) return;

        // プレイヤーの存在チェック
        if (!characterList.ContainsKey(connectionId)) return;

        // 移動・回転・アニメーション処理
        characterList[connectionId].SetActive(playerState.isActiveSelf);
        characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

    /// <summary>
    /// マスタークライアントの情報更新リクエスト
    /// </summary>
    public async void UpdateMasterClientAsynk()
    {
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // プレイヤーの存在チェック

        List<MovingObjectState> movingObjectStates = new List<MovingObjectState>();
        foreach (var obj in movingObjects)
        {
            if (obj.activeSelf && movingObjectList[obj.name].pathTween != null)
            {
                MovingObjectState movingObjectState = new MovingObjectState()
                {
                    name = obj.name,
                    position = obj.transform.position,
                    angle = obj.transform.eulerAngles,
                    elapsedTimeTween = movingObjectList[obj.name].pathTween.Elapsed(),
                    isActiveSelf = obj.activeSelf,
                };
                movingObjectStates.Add(movingObjectState);
            }
        }

        var character = characterList[RoomModel.Instance.ConnectionId];
        PlayerState playerState = null;
        if (character.GetComponent<PlayerController>().enabled)
        {
            playerState = new PlayerState()
            {
                position = character.transform.position,
                angle = character.transform.eulerAngles,
                animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
                isActiveSelf = character.activeSelf,
            };
        }

        MasterClient masterClient = new MasterClient()
        {
            playerState = playerState,
            objectStates = movingObjectStates,
        };
        await RoomModel.Instance.UpdateMasterClientAsynk(masterClient);
    }

    /// <summary>
    /// マスタークライアントの情報更新通知処理
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedMasterClient(Guid connectionId, MasterClient masterClient)
    {
        if (!isGameStartCountDownOver) return;

        // プレイヤーの存在チェック
        if (!characterList.ContainsKey(connectionId)) return;

        if(masterClient.playerState != null)
        {
            PlayerState playerState = masterClient.playerState;

            // 移動・回転・アニメーション処理
            characterList[connectionId].SetActive(playerState.isActiveSelf);
            characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
            characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
            characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
        }

        // オブジェクトの同期
        foreach (var obj in masterClient.objectStates)
        {
            movingObjectList[obj.name].SetPotition(obj, waitSeconds);
        }
    }

    /// <summary>
    /// ゲーム開始前のカウントダウン終了リクエスト
    /// </summary>
    public async void OnCountdownOver()
    {
        isGameStartCountDownOver = true;
        await RoomModel.Instance.CountdownOverAsynk();
    }

    /// <summary>
    /// ゲーム開始通知
    /// </summary>
    void NotifyStartGame()
    {
        // ゲーム開始前のカウントダウンを非表示にする
        gameStartCountDown.PlayCountDownOverAnim();

        // プレイヤーの操作をできるようにする
        characterControlUI.OnSkillButton();
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        StartCoroutine(UpdateCoroutine());

        // ギミックを起動
        gimmicksParent.SetActive(true);
    }

    /// <summary>
    /// ユーザーの所持ポイント更新通知
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="score"></param>
    void NotifyUpdateScore(Guid connectionId, int score)
    {
        userScoreController.UpdateScore(RoomModel.Instance.JoinedUsers[connectionId].JoinOrder, score);
    }

    /// <summary>
    /// カウントダウン開始通知
    /// (マスタークライアントが受信)
    /// </summary>
    void NotifyStartCountDown()
    {
        if (coroutineCountDown == null) coroutineCountDown = StartCoroutine(CountDownCoroutine());
    }

    /// <summary>
    /// カウントダウン処理
    /// (マスタークライアントが処理)
    /// </summary>
    public async void OnCountDown()
    {
        if (currentTime >= 0) await RoomModel.Instance.CountDownAsynk(currentTime);
    }

    /// <summary>
    /// カウントダウン通知
    /// </summary>
    /// <param name="currentTime"></param>
    void NotifyCountDownUser(int currentTime)
    {
        if (coroutineCountDown == null) this.currentTime = currentTime;
        countDownUI.SetActive(true);
        countDownUI.GetComponent<CountDownUI>().UpdateText(currentTime);

        // カウントダウンが0になった場合
        if (currentTime == 0)
        {
            StartCoroutine(FinishGameCoroutine());
        }
    }

    /// <summary>
    /// ゲーム終了が完了したリクエスト
    /// </summary>
    public async void OnFinishGame()
    {
        await RoomModel.Instance.FinishGameAsynk();
    }

    /// <summary>
    /// 最後の競技が終了した通知
    /// </summary>
    void NotifyAfterFinalGameUser()
    {
        // 最終結果発表シーンに遷移
        StopCoroutine(UpdateCoroutine());
        SceneControler.Instance.StartSceneLoad("FinalResultsScene");
    }

    /// <summary>
    /// ゲーム終了準備
    /// </summary>
    public IEnumerator FinishGameCoroutine()
    {
        // 操作を無効化する
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = false;
        characterList[RoomModel.Instance.ConnectionId].layer = 8;   // ギミックなどの当たり判定を無くす

        // ゲーム終了時のUIを表示
        finishUI.SetActive(true);
        yield return new WaitForSeconds(finishUI.GetComponent<FinishUI>().animSec + 1f);  // 余韻の時間を加算

        // ゲーム終了リクエスト
        OnFinishGame();
    }

    /// <summary>
    /// コイン(ポイント)のドロップ通知
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="anglesY"></param>
    void NotifyDropCoinsUser(Vector3 startPoint, int[] anglesY, string[] coinNames, UserScore latestUserScore)
    {
        for (int i = 0; i < coinNames.Length; i++)
        {
            var coin = Instantiate(coinPrefab);
            coin.transform.position = startPoint;
            coin.name = coinNames[i];
            coin.GetComponent<CoinController>().Drop(anglesY[i]);
            if(!itemList.ContainsKey(coin.name)) itemList.Add(coin.name, coin);
        }

        userScoreController.UpdateScore(RoomModel.Instance.JoinedUsers[latestUserScore.ConnectionId].JoinOrder, latestUserScore.LatestScore);
    }

    /// <summary>
    /// 生成場所が異なるコイン(ポイント)のドロップ通知
    /// </summary>
    /// <param name="startPoins"></param>
    /// <param name="coinNames"></param>
    void NotifyDropCoinsAtRandomPositions(Vector3[] startPoins, string[] coinNames, UserScore latestUserScore)
    {
        for (int i = 0; i < coinNames.Length; i++)
        {
            var coin = Instantiate(coinPrefab);
            coin.transform.position = startPoins[i];
            coin.name = coinNames[i];
            if (!itemList.ContainsKey(coin.name)) itemList.Add(coin.name, coin);
        }

        userScoreController.UpdateScore(RoomModel.Instance.JoinedUsers[latestUserScore.ConnectionId].JoinOrder, latestUserScore.LatestScore);
    }

    /// <summary>
    /// アイテム取得通知
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemName"></param>
    /// <param name="option"></param>
    void NotifyGetItemUser(Guid connectionId, string itemName, float option)
    {
        if (!itemList.ContainsKey(itemName)) return;
        var itemController = itemList[itemName].GetComponent<ItemController>();

        if (itemController.ItemId == EnumManager.ITEM_ID.Coin)
        {
            userScoreController.UpdateScore(RoomModel.Instance.JoinedUsers[connectionId].JoinOrder, (int)option);
        }
        else if (connectionId == RoomModel.Instance.ConnectionId)
        {
            characterControlUI.GetComponent<CharacterControlUI>().SetImageItem(itemController.ItemId);
            characterList[connectionId].GetComponent<PlayerItemController>().SetItemSlot(itemController.ItemId);
        }

        Destroy(itemList[itemName]);
    }

    /// <summary>
    /// アイテム使用通知
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="itemId"></param>
    void NotifyUseItemUser(Guid connectionId, EnumManager.ITEM_ID itemId)
    {
        characterList[connectionId].GetComponent<PlayerItemController>().UseItem(itemId);
    }

    /// <summary>
    /// アイテムの破棄通知
    /// </summary>
    /// <param name="itemName"></param>
    void NotifyDestroyItemUser(string itemName)
    {
        if (itemList.ContainsKey(itemName))
        {
            Destroy(itemList[itemName]);
        }
    }

    /// <summary>
    /// アイテムの生成通知
    /// </summary>
    /// <param name="spawnPoint"></param>
    /// <param name="itemId"></param>
    void NotifySpawnItemUser(Vector3 spawnPoint, EnumManager.ITEM_ID itemId, string itemName)
    {
        var item = itemSpawner.Spawn(spawnPoint, itemId, itemName);
        if (!itemList.ContainsKey(item.name)) itemList.Add(itemName, item);
    }

    /// <summary>
    /// 動的なオブジェクトの生成通知
    /// </summary>
    /// <param name="spawnObject"></param>
    void NotifySpawnObjectUser(SpawnObject spawnObject)
    {
        GetComponent<ObjectPrefabController>().Spawn(spawnObject);
    }
}
