using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static AreaController;

public class AreaController : MonoBehaviour
{
    [SerializeField] List<GameObject> startPoints;    // エリア１を除く、各エリアのスタート地点
    [SerializeField] TargetCameraController targetCameraController;

    [SerializeField] GameObject imageBlackObj;
    Image imageBlack;

    [SerializeField] List<GameObject> gimmicks; // エリア毎のギミック

    public enum AREA_ID
    {
        AREA_1 = 0,
        AREA_2 = 1,
    }
    public AREA_ID areaId = AREA_ID.AREA_1;

    private void Awake()
    {
        imageBlack = imageBlackObj.GetComponent<Image>();

        foreach(var gimmick in gimmicks)
        {
            gimmick.SetActive(false);
        }
    }

    /// <summary>
    /// エリアのゴール処理
    /// </summary>
    public void AreaGoal(bool isDebug, GameObject player)
    {
        // ゴールしたのが最後のエリアの場合
        if (areaId == AREA_ID.AREA_2)
        {
            Debug.Log("ゴール!!");
        }
        else if(!isDebug)
        {
            HideGameScrean();
        }

        // サーバーなしの場合のみ使用、最終的にisDebugを削除
        else if (isDebug)
        {
            HideGameScrean();
            StartCoroutine(RestarningGame((int)areaId, player, 1));
        }
    }

    /// <summary>
    /// 画面を隠す(フェードイン)
    /// </summary>
    public void HideGameScrean()
    {
        areaId++;

        imageBlackObj.SetActive(true);
        imageBlack.DOFade(1f, 0.5f).SetEase(Ease.Linear).OnComplete(() => {
            gimmicks[(int)areaId - 1].SetActive(false);
        });
    }

    /// <summary>
    /// フェードアウト後にゲーム再開
    /// </summary>
    public IEnumerator RestarningGame(int _areaId, GameObject player ,float restarningWaitSec)
    {
        // 次のエリアに移動する
        player.transform.position = startPoints[_areaId].transform.position;
        targetCameraController.InitCamera(null, _areaId);
        gimmicks[_areaId].SetActive(true);

        imageBlack.DOFade(0f, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            imageBlackObj.SetActive(false);
        });

        // 指定された時間差で動けるようにする
        yield return new WaitForSeconds(0.5f + restarningWaitSec);
        player.SetActive(true);
    }
}
