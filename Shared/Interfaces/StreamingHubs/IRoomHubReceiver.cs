using Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    /// <summary>
    /// サーバー側からクライアントを呼び出す関数を定義
    /// </summary>
    public interface IRoomHubReceiver
    {
        //*********************************************************
        // サーバー側からクライアント側を呼び出す関数を定義する
        //*********************************************************

        /// <summary>
        /// マッチング完了通知
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        void OnMatching(string roomName);

        /// <summary>
        /// ユーザーの入室通知
        /// </summary>
        /// <param name="user">入室したユーザー情報</param>
        void OnJoin(JoinedUser user);

        /// <summary>
        /// ユーザーの退室通知
        /// </summary>
        /// <param name="connectionId">退室したユーザーの接続ID</param>
        /// <param name="latestData">最新のユーザーデータ</param>
        void OnLeave(Guid connectionId, JoinedUser latestData);

        /// <summary>
        /// プレイヤーの情報更新通知
        /// </summary>
        /// <param name="connectionId">接続ID</param>
        /// <param name="state">プレイヤー情報</param>
        void OnUpdatePlayerState(Guid connectionId, PlayerState state);

        /// <summary>
        /// マスタークライアントの情報更新通知
        /// </summary>
        /// <param name="masterClient"></param>
        void OnUpdateMasterClient(Guid connectionId, MasterClient masterClient);

        #region ゲーム開始までの処理
        /// <summary>
        /// 準備完了したかどうかの通知
        /// </summary>
        /// <param name="readyCnt">準備完了しているユーザー数</param>
        /// <param name="isTransitionGameScene">全員が準備完了したときにシーン遷移させる</param>
        void OnReady(int readyCnt, bool isTransitionGameScene);

        /// <summary>
        /// ゲーム開始通知
        /// </summary>
        void OnCountdownOver();
        #endregion

        #region ゲーム共通処理
        /// <summary>
        /// ユーザーの所持ポイント更新通知
        /// </summary>
        void OnUpdateScore(UserScore latestUserScore);

        /// <summary>
        /// カウントダウン開始通知
        /// (マスタークライアントが受信)
        /// </summary>
        void OnStartCountDown();

        /// <summary>
        /// カウントダウン通知
        /// </summary>
        /// <param name="currentTime"></param>
        void OnCountDown(int currentTime);

        /// <summary>
        /// 全員がゲーム終了処理を完了した通知
        /// </summary>
        void OnFinishGame(EnumManager.SCENE_ID scene);

        /// <summary>
        /// コイン(ポイント)のドロップ通知
        /// (ユーザーがノックダウンしたときに処理)
        /// </summary>
        /// <param name="startPoint">生成する位置</param>
        /// <param name="anglesY">コインの向き</param>
        /// <param name="coinNames">コインのユニークな名前</param>
        void OnDropCoins(Vector3 startPoint,int[] anglesY, string[] coinNames, UserScore userScore);

        /// <summary>
        /// 生成場所が異なるコイン(ポイント)のドロップ通知
        /// (ユーザーが場外に出たときに処理)
        /// </summary>
        /// <param name="startPoins">生成する位置</param>
        /// <param name="coinNames">コインのユニークな名前</param>
        void OnDropCoinsAtRandomPositions(Vector3[] startPoins, string[] coinNames, UserScore userScore);

        /// <summary>
        /// アイテム入手通知
        /// </summary>
        void OnGetItem(Guid connectionId, string itemName, float option);

        /// <summary>
        /// アイテム使用通知
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="itemId"></param>
        void OnUseItem(Guid connectionId, EnumManager.ITEM_ID itemId);

        /// <summary>
        /// アイテムの破棄通知
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        void OnDestroyItem(string itemName);

        /// <summary>
        /// アイテムの生成通知
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <param name="itemId"></param>
        void OnSpawnItem(Vector3 spawnPoint, EnumManager.ITEM_ID itemId, string itemName);

        /// <summary>
        /// 動的なオブジェクトの生成通知
        /// </summary>
        /// <param name="spawnObject"></param>
        void OnSpawnObject(SpawnObject spawnObject);

        /// <summary>
        /// 動物のギミック発動通知
        /// </summary>
        /// <param name="animalName"></param>
        /// <param name="optionVec"></param>
        void OnPlayAnimalGimmick(EnumManager.ANIMAL_GIMMICK_ID animalId ,string animalName, Vector3[] optionVec);
        #endregion

        #region 競技『カントリーリレー』の処理

        /// <summary>
        /// 植物のギミックを破棄する通知
        /// </summary>
        /// <param name="names"></param>
        void OnDestroyPlantsGimmick(string[] names);

        /// <summary>
        /// 植物のギミックを発動する通知
        /// </summary>
        /// <param name="name"></param>
        void OnTriggeringPlantGimmick(string name);

        /// <summary>
        /// 現在のエリアをクリアした通知
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="userName"></param>
        void OnAreaCleared(Guid connectionId, string userName, bool isClearedAllUsers);

        /// <summary>
        /// 全員が次のエリアに移動する準備が完了した通知(ゲーム再開通知)
        /// </summary>
        /// <param name="restarningWaitSec">ゲーム再開までの待機時間(順位によって変動)</param>
        void OnReadyNextAreaAllUsers(float restarningWaitSec, EnumManager.RELAY_AREA_ID nextAreaId);
        #endregion

        #region ゲーム終了までの処理(最終結果発表シーンの処理)

        /// <summary>
        /// 最後の競技が終了した通知
        /// </summary>
        void OnAfterFinalGame();

        /// <summary>
        /// 全員が遷移できた通知
        /// </summary>
        void OnTransitionFinalResultSceneAllUsers(ResultData[] result, int ratingDelta);
        #endregion
    }
}
