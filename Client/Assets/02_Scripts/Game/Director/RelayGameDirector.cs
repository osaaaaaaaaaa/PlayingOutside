using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using DG.Tweening;
using Server.Model.Entity;

public class RelayGameDirector : MonoBehaviour
{
    [SerializeField] GameStartCountDown gameStartCountDown;
    [SerializeField] SpectatingUI spectatingUI;
    [SerializeField] GameObject countDownUI;

    #region コントローラー関係
    [SerializeField] AreaController areaController;
    [SerializeField] TargetCameraController targetCameraController;
    [SerializeField] CharacterControlUI characterControlUI;
    [SerializeField] UserScoreController userScoreController;
    #endregion

    #region キャラクター関係
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] List<GameObject> characterPrefabList;
    public Dictionary<Guid,GameObject> characterList { get; private set; }  = new Dictionary<Guid,GameObject>();  // ユーザーのキャラクター情報
    #endregion

    #region MoveRootスクリプトを適用しているギミック
    [SerializeField] GameObject gimmicksParent;
    [SerializeField] List<GameObject> movingObjects;
    Dictionary<string, MoveSetRoot> movingObjectList = new Dictionary<string, MoveSetRoot>();
    #endregion

    #region 動物のギミック
    [SerializeField] List<GameObject> animalGimmicks;
    Dictionary<string,GameObject> animalGimmickList = new Dictionary<string, GameObject>();
    #endregion

    Dictionary<string, GameObject> itemList = new Dictionary<string, GameObject>();

    Coroutine coroutineCountDown;
    int currentTime;
    bool isGameStartCountDownOver;

    const float waitSeconds = 0.1f;

    public bool isDebug = false;

    private void Start()
    {
        if (isDebug) return;
        isGameStartCountDownOver = false;
        currentTime = 0;

        // 関数を登録する
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnUpdateMasterClientUser += this.NotifyUpdatedMasterClient;
        RoomModel.Instance.OnCountdownOverUser += this.NotifyStartGame;
        RoomModel.Instance.OnAreaClearedUser += this.NotifyAreaClearedUser;
        RoomModel.Instance.OnReadyNextAreaUser += this.NotifyRedyNextAreaAllUsers;
        RoomModel.Instance.OnStartCountDownUser += this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser += this.NotifyCountDownUser;
        RoomModel.Instance.OnFinishGameUser += this.NotifyFinishGameUser;
        RoomModel.Instance.OnUpdateScoreUser += this.NotifyUpdateScore;

        RoomModel.Instance.OnGetItemUser += this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser += this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser += this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser += this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser += this.NotifySpawnObjectUser;
        RoomModel.Instance.OnPlayAnimalGimmickUser += this.NotifyPlayAnimalGimmickUser;

        SetupGame();
    }

    void OnDisable()
    {
        // シーン遷移時に関数の登録を解除
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnUpdateMasterClientUser -= this.NotifyUpdatedMasterClient;
        RoomModel.Instance.OnCountdownOverUser -= this.NotifyStartGame;
        RoomModel.Instance.OnAreaClearedUser -= this.NotifyAreaClearedUser;
        RoomModel.Instance.OnReadyNextAreaUser -= this.NotifyRedyNextAreaAllUsers;
        RoomModel.Instance.OnStartCountDownUser -= this.NotifyStartCountDown;
        RoomModel.Instance.OnCountDownUser -= this.NotifyCountDownUser;
        RoomModel.Instance.OnFinishGameUser -= this.NotifyFinishGameUser;
        RoomModel.Instance.OnUpdateScoreUser -= this.NotifyUpdateScore;

        RoomModel.Instance.OnGetItemUser -= this.NotifyGetItemUser;
        RoomModel.Instance.OnUseItemUser -= this.NotifyUseItemUser;
        RoomModel.Instance.OnDestroyItemUser -= this.NotifyDestroyItemUser;
        RoomModel.Instance.OnSpawnItemUser -= this.NotifySpawnItemUser;
        RoomModel.Instance.OnSpawnObjectUser -= this.NotifySpawnObjectUser;
        RoomModel.Instance.OnPlayAnimalGimmickUser -= this.NotifyPlayAnimalGimmickUser;
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
        if (currentTime == 0) currentTime = 11;
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
        foreach (var item in movingObjects)
        {
            movingObjectList.Add(item.name, item.GetComponent<MoveSetRoot>());
        }

        // 動物のギミックを設定
        foreach (var item in animalGimmicks)
        {
            animalGimmickList.Add(item.name, item);
        }

        // 動くオブジェクトを設定
        foreach (var item in movingObjects)
        {
            movingObjectList.Add(item.name, item.GetComponent<MoveSetRoot>());
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
            character.name = user.Value.UserData.Name;

            // プレイヤーの初期化処理
            bool isMyCharacter = user.Key == RoomModel.Instance.ConnectionId;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.Value.JoinOrder - 1]);
            character.GetComponent<AudioListener>().enabled = isMyCharacter;

            // ユーザー名の初期化処理
            Color colorText = isMyCharacter ? Color.white : Color.green;
            character.GetComponent<PlayerUIController>().InitUI(user.Value.UserData.Name, colorText);

            // レイヤータグを変更
            character.layer = isMyCharacter ? 3 : 7;
            // ゲームが開始するまではPlayerControllerを外す
            character.GetComponent<PlayerController>().enabled = false;

            if (isMyCharacter)
            {
                targetCameraController.InitCamera(character.transform, 0, user.Key);    // 自分のモデルにカメラのターゲットを設定
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
            foreach (var obj in movingObjectList)
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
        if(character.GetComponent<PlayerController>().enabled)
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

        if (masterClient.playerState != null)
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
    }

    /// <summary>
    /// 現在のエリアをクリアした処理をリクエスト
    /// </summary>
    public async void OnAreaCleared()
    {
        await RoomModel.Instance.AreaClearedAsynk();
    }

    /// <summary>
    /// 現在のエリアをクリアした通知
    /// </summary>
    void NotifyAreaClearedUser(Guid connectionId,string userName, bool isClearedAllUsers)
    {
        // クリアしたユーザー名を表示する
        Debug.Log(userName + "が突破");

        if (isClearedAllUsers)
        {
            // カウントダウンのコルーチンを停止する
            if(coroutineCountDown != null) StopCoroutine(coroutineCountDown);
            coroutineCountDown = null;

            // 全員が現在のエリアをクリアした場合、次のエリアに移動する準備をする
            StartCoroutine(areaController.ReadyNextAreaCoroutine());
            return;
        }

        // カメラのターゲットが自分の場合は処理を終了
        if (targetCameraController.currentTargetId == RoomModel.Instance.ConnectionId) return;
        characterList[connectionId].SetActive(false);   // 非表示になっていない場合があるため

        // 他にカメラのターゲットの切り替え先が存在するかチェック
        bool isTarget = targetCameraController.IsOtherTarget();
        if (targetCameraController.activeTargetCnt == 1) spectatingUI.SetupButton(false);

        // 現在のカメラのターゲットとクリアした人が同一人物かどうか
        if (isTarget && connectionId == targetCameraController.currentTargetId)
        {
            // カメラのターゲットの切り替え先が存在する場合は切り替える
            spectatingUI.OnChangeTargetBtn();
        }
    }

    /// <summary>
    /// 次のエリアに移動する準備が完了リクエスト
    /// </summary>
    public async void OnReadyNextArea(bool isLastArea)
    {
        // カウントダウンのコルーチンを停止する
        if (coroutineCountDown != null) StopCoroutine(coroutineCountDown);
        coroutineCountDown = null;

        if (isLastArea)
        {
            // ゲーム終了リクエスト
            OnFinishGame();
        }
        else
        {
            // 現在のエリアが最後のエリアではない場合
            await RoomModel.Instance.ReadyNextAreaAsynk();
        }
    }

    /// <summary>
    /// 全員が次のエリアに移動する準備が完了した通知
    /// </summary>
    void NotifyRedyNextAreaAllUsers(float restarningWaitSec)
    {
        countDownUI.SetActive(false);
        coroutineCountDown = null;
        currentTime = 0;

        var myCharacter = characterList[RoomModel.Instance.ConnectionId];
        myCharacter.SetActive(false);

        // ゲーム再開処理
        StartCoroutine(areaController.RestarningGameCoroutine(myCharacter,restarningWaitSec));
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
    /// エリアクリア時のカウントダウン開始通知
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
        if (currentTime >= 0 && !areaController.isClearedArea)
        {
            await RoomModel.Instance.CountDownAsynk(currentTime);
        }
    }

    /// <summary>
    /// カウントダウン通知
    /// </summary>
    /// <param name="currentTime"></param>
    void NotifyCountDownUser(int currentTime)
    {
        if(coroutineCountDown == null) this.currentTime = currentTime;
        countDownUI.SetActive(true);
        countDownUI.GetComponent<CountDownUI>().UpdateText(currentTime);

        // まだクリアしていない && カウントダウンが0以下になったら、次のエリアへ強制移動
        if (!areaController.isClearedArea && currentTime == 0)
        {
            StartCoroutine(areaController.ReadyNextAreaCoroutine());
        }
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
        if (areaController.ItemSpawnerList[(int)areaController.areaId].enabled)
        {
            var item = areaController.ItemSpawnerList[(int)areaController.areaId].Spawn(spawnPoint, itemId, itemName);
            if (!itemList.ContainsKey(item.name)) itemList.Add(itemName, item);
        }
    }

    /// <summary>
    /// 動的なオブジェクトの生成通知
    /// </summary>
    /// <param name="spawnObject"></param>
    void NotifySpawnObjectUser(SpawnObject spawnObject)
    {
        GetComponent<ObjectPrefabController>().Spawn(spawnObject);
    }

    /// <summary>
    /// 動物のギミック発動通知
    /// </summary>
    /// <param name="name"></param>
    /// <param name="option"></param>
    void NotifyPlayAnimalGimmickUser(EnumManager.ANIMAL_GIMMICK_ID animalId, string name, Vector3[] option)
    {
        var animal = animalGimmickList[name];
        if(animal != null && animal.activeSelf)
        {
            switch (animalId)
            {
                case EnumManager.ANIMAL_GIMMICK_ID.Bull:
                    animal.GetComponent<BullGimmick>().PlayEatAnim();
                    break;
                case EnumManager.ANIMAL_GIMMICK_ID.Chicken:
                    animal.transform.GetChild(0).GetComponent<ChickenGimmick>().GenerateEggBulletWarning(option);
                    break;
            }
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
    /// 全員のゲーム終了処理が完了した通知
    /// </summary>
    void NotifyFinishGameUser(string nextSceneName)
    {
        StopCoroutine(UpdateCoroutine());
        SceneControler.Instance.StartSceneLoad(nextSceneName);
    }
}
