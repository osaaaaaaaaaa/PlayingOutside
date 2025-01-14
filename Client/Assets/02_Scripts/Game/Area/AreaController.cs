using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static AreaController;
using Unity.VisualScripting;

public class AreaController : MonoBehaviour
{
    #region コントローラー関係
    [SerializeField] RelayGameDirector gameDirector;
    [SerializeField] TargetCameraController targetCameraController;
    [SerializeField] List<ItemSpawner> itemSpawnerList;
    public List<ItemSpawner> ItemSpawnerList { get { return itemSpawnerList; } }
    #endregion

    [SerializeField] List<GameObject> startPoints;    // 各エリアのスタート地点
    [SerializeField] List<GameObject> gimmicks;       // エリア毎のギミック
    
    #region UI関係
    [SerializeField] GameObject finishUI;
    [SerializeField] GameObject spectatingUI;
    [SerializeField] GameObject imageBlackObj;
    Image imageBlack;
    const float fadeTime = 0.5f;
    #endregion

    #region オーディオ関係
    [SerializeField] AudioClip clearedAreSE;
    AudioSource audioSource;
    #endregion

    public bool isClearedArea { get; private set; }

    public enum AREA_ID
    {
        AREA_1 = 0,
        AREA_2,
        AREA_3,
        AREA_4,
    }
    public AREA_ID areaId { get; set; } = AREA_ID.AREA_1;
    AREA_ID lastAreaId = AREA_ID.AREA_4;

    private void Awake()
    {
        imageBlack = imageBlackObj.GetComponent<Image>();
        audioSource = GetComponent<AudioSource>();
        isClearedArea = false;

        foreach (var item in itemSpawnerList)
        {
            item.enabled = false;
        }
        itemSpawnerList[(int)areaId].enabled = true;
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
        audioSource.PlayOneShot(clearedAreSE);
        isClearedArea = false;
        bool isLastArea = (areaId == lastAreaId);
        // サーバーなしの場合のみ使用、最終的にisDebugを削除
        if (gameDirector.isDebug)
        {
            if (isLastArea)
            {
                Debug.Log("ゴール!!");
            }
            else
            {
                isClearedArea = true;
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
        Debug.Log(areaId + "エリア移動準備");
        isClearedArea = true;

        DOTween.Kill(imageBlack);
        bool isLastArea = (areaId == lastAreaId);
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

            // 現在のエリアを撤去
            itemSpawnerList[(int)areaId].enabled = false;
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
        player.SetActive(false);
        areaId++;
        Debug.Log("エリアのID："+ (int)areaId);

        // 次のエリアの準備
        itemSpawnerList[(int)areaId].enabled = true;
        gimmicks[(int)areaId].SetActive(true);

        // 次のエリアに移動する && カメラをセットアップ
        player.GetComponent<PlayerController>().InitPlayer(startPoints[(int)areaId].transform);
        targetCameraController.InitCamera(player.transform, (int)areaId,RoomModel.Instance.ConnectionId);

        // フェードアウト
        imageBlack.DOFade(0f, fadeTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            imageBlackObj.SetActive(false);
        });

        // 指定された時間差で動けるようにする
        yield return new WaitForSeconds(fadeTime + restarningWaitSec);
        isClearedArea = false;
        player.GetComponent<PlayerController>().enabled = true;
        player.SetActive(true);
        player.GetComponent<PlayerController>().InitPlayer();
    }
}
