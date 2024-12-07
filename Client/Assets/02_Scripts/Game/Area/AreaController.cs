using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static AreaController;
using Unity.VisualScripting;

public class AreaController : MonoBehaviour
{
    [SerializeField] GameDirector gameDirector;
    [SerializeField] TargetCameraController targetCameraController;

    [SerializeField] List<GameObject> startPoints;    // 各エリアのスタート地点
    [SerializeField] List<GameObject> gimmicks;       // エリア毎のギミック

    [SerializeField] GameObject finishUI;
    [SerializeField] GameObject spectatingUI;
    [SerializeField] GameObject imageBlackObj;
    Image imageBlack;

    const float fadeTime = 0.5f;
    public bool isClearedArea { get; private set; }

    public enum AREA_ID
    {
        AREA_1 = 0,
        AREA_2 = 1,
    }
    public AREA_ID areaId = AREA_ID.AREA_1;

    private void Awake()
    {
        imageBlack = imageBlackObj.GetComponent<Image>();
        isClearedArea = false;

        foreach (var gimmick in gimmicks)
        {
            gimmick.SetActive(false);
        }
        gimmicks[(int)areaId].SetActive(true);
    }

    /// <summary>
    /// 現在のエリアをクリアした処理
    /// </summary>
    public IEnumerator CurrentAreaClearCoroutine(GameObject player)
    {
        if (isClearedArea) yield break;

        bool isLastArea = (areaId == AREA_ID.AREA_2);
        // サーバーなしの場合のみ使用、最終的にisDebugを削除
        if (gameDirector.isDebug)
        {
            if (isLastArea)
            {
                Debug.Log("ゴール!!");
            }
            else
            {
                StartCoroutine(RestarningGameCoroutine(player, 1));
            }

            // このコルーチンを停止
            yield break;
        }

        // エリアクリア処理をリクエスト
        gameDirector.OnAreaCleared();

        // まだ他にエリアをクリアしていないプレイヤーがいるかチェック
        if (targetCameraController.IsOtherTarget())
        {
            // フェードイン
            imageBlackObj.SetActive(true);
            imageBlack.DOFade(1f, fadeTime).SetEase(Ease.Linear).OnComplete(() => {
                // 観戦用のUIを表示する
                spectatingUI.GetComponent<SpectatingUI>().InitUI(true);
            });
            yield return new WaitForSeconds(fadeTime);
        }

        // 観戦画面を表示することができた場合
        if (spectatingUI.activeSelf)
        {
            imageBlack.DOFade(0f, fadeTime).SetEase(Ease.Linear).OnComplete(() => { imageBlackObj.SetActive(false); });
        }
        else
        {
            // 自分が最後にエリアをクリアした場合
            StartCoroutine(ReadyNextAreaCoroutine());
        }
    }

    /// <summary>
    /// 次のエリアに移動する準備
    /// </summary>
    public IEnumerator ReadyNextAreaCoroutine()
    {
        if(isClearedArea) yield break;
        isClearedArea = true;
        Debug.Log("クリア!");

        DOTween.Kill(imageBlack);
        bool isLastArea = (areaId == AREA_ID.AREA_2);
        float animSec = (imageBlackObj.activeSelf) ? 0f : fadeTime;

        // 操作を無効化する
        gameDirector.characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = false;

        if (isLastArea)
        {
            // 現在のエリアが最後のエリアの場合はゲーム終了時のUIを表示
            finishUI.SetActive(true);

            yield return new WaitForSeconds(finishUI.GetComponent<FinishUI>().animSec + 1f);  // 余韻の時間を加算
        }

        // フェードイン
        imageBlackObj.SetActive(true);
        imageBlack.DOFade(1f, animSec).SetEase(Ease.Linear).OnComplete(() => {

            // 観戦用のUIを非表示する
            spectatingUI.GetComponent<SpectatingUI>().InitUI(false);

            // 現在のエリアのギミックを非表示
            gimmicks[(int)areaId].SetActive(false);

            // 次のエリアに移動する準備が完了したリクエスト
            gameDirector.OnReadyNextArea(isLastArea);
        });
    }

    /// <summary>
    /// フェードアウト後にゲーム再開
    /// </summary>
    public IEnumerator RestarningGameCoroutine(GameObject player ,float restarningWaitSec)
    {
        if (!isClearedArea) yield break;
        isClearedArea = false;
        areaId++;
        Debug.Log("エリアのID："+ (int)areaId);

        // 次のエリアのギミックを表示
        gimmicks[(int)areaId].SetActive(true);

        // 次のエリアに移動する && カメラをセットアップ
        player.transform.position = startPoints[(int)areaId].transform.position;
        targetCameraController.InitCamera(player.transform, (int)areaId,RoomModel.Instance.ConnectionId);

        // フェードアウト
        imageBlack.DOFade(0f, fadeTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            imageBlackObj.SetActive(false);
        });

        // 指定された時間差で動けるようにする
        yield return new WaitForSeconds(fadeTime + restarningWaitSec);
        gameDirector.characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        player.SetActive(true);
    }
}
