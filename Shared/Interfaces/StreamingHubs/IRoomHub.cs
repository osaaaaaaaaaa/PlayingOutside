using MagicOnion;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Shared.Interfaces.StreamingHubs
{
    public interface IRoomHub : IStreamingHub<IRoomHub, IRoomHubReceiver>
    {
        //*********************************************************
        // クライアント側からサーバー側を呼び出す関数を定義する
        //*********************************************************

        /// <summary>
        /// ロビー入室
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns></returns>
        Task<JoinedUser[]> JoinLobbyAsynk(int userId);

        /// <summary>
        /// ユーザー入室
        /// </summary>
        /// <param name="roomName">ルーム名</param>
        /// <param name="userId">ユーザーID</param>
        /// <param name="isMatching">自動マッチング済みかどうか</param>
        /// <returns></returns>
        Task<JoinedUser[]> JoinAsynk(string roomName, int userId, bool isMatching);

        /// <summary>
        /// ユーザー退室
        /// </summary>
        /// <returns></returns>
        Task LeaveAsynk();

        /// <summary>
        /// プレイヤー情報更新
        /// </summary>
        /// <param name="state">プレイヤー情報</param>
        /// <returns></returns>
        Task UpdatePlayerStateAsynk(PlayerState state);

        /// <summary>
        /// マスタークライアントの情報更新処理
        /// </summary>
        /// <param name="masterClient"></param>
        /// <returns></returns>
        Task UpdateMasterClientAsynk(MasterClient masterClient);

        #region ゲーム開始までの処理
        /// <summary>
        /// 競技カントリーリレーの中間エリアを選択する
        /// (マスタークライアントが処理)
        /// </summary>
        /// <param name="selectMidAreaId"></param>
        /// <returns></returns>
        Task SelectMidAreaAsynk(EnumManager.SELECT_MID_AREA_ID selectMidAreaId);

        /// <summary>
        /// 準備完了したかどうか
        /// </summary>
        /// <param name="isReady"></param>
        /// <returns></returns>
        Task ReadyAsynk(bool isReady);

        /// <summary>
        /// ゲーム開始前のカウントダウン終了
        /// </summary>
        /// <returns></returns>
        Task CountdownOverAsynk();
        #endregion

        #region ゲーム共通処理
        /// <summary>
        /// カウントダウン処理
        /// (マスタークライアントが繰り返し呼び出す)
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        Task CountDownAsynk(int currentTime);

        /// <summary>
        /// 各自の画面でゲームが終了
        /// </summary>
        /// <returns></returns>
        Task FinishGameAsynk();

        /// <summary>
        /// ノックダウン時
        /// </summary>
        /// <returns></returns>
        Task KnockDownAsynk(Vector3 startPoint);

        /// <summary>
        /// 場外に出た時
        /// </summary>
        /// <param name="rangePointA">ステージの範囲A</param>
        /// <param name="rangePointB">ステージの範囲B</param>
        /// <returns></returns>
        Task OutOfBoundsAsynk(Vector3 rangePointA, Vector3 rangePointB);

        /// <summary>
        /// アイテムの入手処理
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        Task GetItemAsynk(EnumManager.ITEM_ID itemId, string itemName);

        /// <summary>
        /// アイテムの使用
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        Task UseItemAsynk(EnumManager.ITEM_ID itemId);

        /// <summary>
        /// アイテムの破棄
        /// (マスタークライアントが呼び出す)
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        Task DestroyItemAsynk(string itemName);

        /// <summary>
        /// アイテムの生成
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        Task SpawnItemAsynk(Vector3 spawnPoint, EnumManager.ITEM_ID itemId);

        /// <summary>
        /// 動的なオブジェクトを生成
        /// </summary>
        /// <param name="spawnObject"></param>
        /// <returns></returns>
        Task SpawnObjectAsynk(SpawnObject spawnObject);

        /// <summary>
        /// 動物のギミック発動処理
        /// </summary>
        /// <param name="animalName"></param>
        /// <param name="optionVec"></param>
        /// <returns></returns>
        Task PlayAnimalGimmickAsynk(EnumManager.ANIMAL_GIMMICK_ID animalId , string animalName, Vector3[] optionVec);
        #endregion

        #region 競技『カントリーリレー』の処理
        /// <summary>
        /// 植物のギミックを破棄するリクエスト
        /// (マスタークライアントが呼び出す)
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        Task DestroyPlantsGimmickAsynk(string[] names);

        /// <summary>
        /// 植物のギミックを発動するリクエスト
        /// </summary>
        /// <returns></returns>
        Task TriggeringPlantGimmickAsynk(string name);

        /// <summary>
        /// エリアをクリアした処理
        /// </summary>
        /// <returns></returns>
        Task AreaClearedAsynk();

        /// <summary>
        /// 次のエリアに移動する準備が完了した処理
        /// </summary>
        /// <returns></returns>
        Task ReadyNextAreaAsynk();
        #endregion

        #region ゲーム終了までの処理(最終結果発表シーンの処理)

        /// <summary>
        /// 最終結果発表シーンに遷移した処理
        /// </summary>
        /// <returns></returns>
        Task TransitionFinalResultSceneAsynk();
        #endregion
    }
}
