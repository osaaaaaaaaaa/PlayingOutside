using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static AreaController;

public class AreaController : MonoBehaviour
{
    [SerializeField] List<GameObject> startPoints;    // �G���A�P�������A�e�G���A�̃X�^�[�g�n�_
    [SerializeField] TargetCameraController targetCameraController;

    [SerializeField] GameObject imageBlackObj;
    Image imageBlack;

    [SerializeField] List<GameObject> gimmicks; // �G���A���̃M�~�b�N

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
    /// �G���A�̃S�[������
    /// </summary>
    public void AreaGoal(bool isDebug, GameObject player)
    {
        // �S�[�������̂��Ō�̃G���A�̏ꍇ
        if (areaId == AREA_ID.AREA_2)
        {
            Debug.Log("�S�[��!!");
        }
        else if(!isDebug)
        {
            HideGameScrean();
        }

        // �T�[�o�[�Ȃ��̏ꍇ�̂ݎg�p�A�ŏI�I��isDebug���폜
        else if (isDebug)
        {
            HideGameScrean();
            StartCoroutine(RestarningGame((int)areaId, player, 1));
        }
    }

    /// <summary>
    /// ��ʂ��B��(�t�F�[�h�C��)
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
    /// �t�F�[�h�A�E�g��ɃQ�[���ĊJ
    /// </summary>
    public IEnumerator RestarningGame(int _areaId, GameObject player ,float restarningWaitSec)
    {
        // ���̃G���A�Ɉړ�����
        player.transform.position = startPoints[_areaId].transform.position;
        targetCameraController.InitCamera(null, _areaId);
        gimmicks[_areaId].SetActive(true);

        imageBlack.DOFade(0f, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            imageBlackObj.SetActive(false);
        });

        // �w�肳�ꂽ���ԍ��œ�����悤�ɂ���
        yield return new WaitForSeconds(0.5f + restarningWaitSec);
        player.SetActive(true);
    }
}
