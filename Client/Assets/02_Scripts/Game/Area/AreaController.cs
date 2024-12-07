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

    [SerializeField] List<GameObject> startPoints;    // �e�G���A�̃X�^�[�g�n�_
    [SerializeField] List<GameObject> gimmicks;       // �G���A���̃M�~�b�N

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
    /// ���݂̃G���A���N���A��������
    /// </summary>
    public IEnumerator CurrentAreaClearCoroutine(GameObject player)
    {
        if (isClearedArea) yield break;

        bool isLastArea = (areaId == AREA_ID.AREA_2);
        // �T�[�o�[�Ȃ��̏ꍇ�̂ݎg�p�A�ŏI�I��isDebug���폜
        if (gameDirector.isDebug)
        {
            if (isLastArea)
            {
                Debug.Log("�S�[��!!");
            }
            else
            {
                StartCoroutine(RestarningGameCoroutine(player, 1));
            }

            // ���̃R���[�`�����~
            yield break;
        }

        // �G���A�N���A���������N�G�X�g
        gameDirector.OnAreaCleared();

        // �܂����ɃG���A���N���A���Ă��Ȃ��v���C���[�����邩�`�F�b�N
        if (targetCameraController.IsOtherTarget())
        {
            // �t�F�[�h�C��
            imageBlackObj.SetActive(true);
            imageBlack.DOFade(1f, fadeTime).SetEase(Ease.Linear).OnComplete(() => {
                // �ϐ�p��UI��\������
                spectatingUI.GetComponent<SpectatingUI>().InitUI(true);
            });
            yield return new WaitForSeconds(fadeTime);
        }

        // �ϐ��ʂ�\�����邱�Ƃ��ł����ꍇ
        if (spectatingUI.activeSelf)
        {
            imageBlack.DOFade(0f, fadeTime).SetEase(Ease.Linear).OnComplete(() => { imageBlackObj.SetActive(false); });
        }
        else
        {
            // �������Ō�ɃG���A���N���A�����ꍇ
            StartCoroutine(ReadyNextAreaCoroutine());
        }
    }

    /// <summary>
    /// ���̃G���A�Ɉړ����鏀��
    /// </summary>
    public IEnumerator ReadyNextAreaCoroutine()
    {
        if(isClearedArea) yield break;
        isClearedArea = true;
        Debug.Log("�N���A!");

        DOTween.Kill(imageBlack);
        bool isLastArea = (areaId == AREA_ID.AREA_2);
        float animSec = (imageBlackObj.activeSelf) ? 0f : fadeTime;

        // ����𖳌�������
        gameDirector.characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = false;

        if (isLastArea)
        {
            // ���݂̃G���A���Ō�̃G���A�̏ꍇ�̓Q�[���I������UI��\��
            finishUI.SetActive(true);

            yield return new WaitForSeconds(finishUI.GetComponent<FinishUI>().animSec + 1f);  // �]�C�̎��Ԃ����Z
        }

        // �t�F�[�h�C��
        imageBlackObj.SetActive(true);
        imageBlack.DOFade(1f, animSec).SetEase(Ease.Linear).OnComplete(() => {

            // �ϐ�p��UI���\������
            spectatingUI.GetComponent<SpectatingUI>().InitUI(false);

            // ���݂̃G���A�̃M�~�b�N���\��
            gimmicks[(int)areaId].SetActive(false);

            // ���̃G���A�Ɉړ����鏀���������������N�G�X�g
            gameDirector.OnReadyNextArea(isLastArea);
        });
    }

    /// <summary>
    /// �t�F�[�h�A�E�g��ɃQ�[���ĊJ
    /// </summary>
    public IEnumerator RestarningGameCoroutine(GameObject player ,float restarningWaitSec)
    {
        if (!isClearedArea) yield break;
        isClearedArea = false;
        areaId++;
        Debug.Log("�G���A��ID�F"+ (int)areaId);

        // ���̃G���A�̃M�~�b�N��\��
        gimmicks[(int)areaId].SetActive(true);

        // ���̃G���A�Ɉړ����� && �J�������Z�b�g�A�b�v
        player.transform.position = startPoints[(int)areaId].transform.position;
        targetCameraController.InitCamera(player.transform, (int)areaId,RoomModel.Instance.ConnectionId);

        // �t�F�[�h�A�E�g
        imageBlack.DOFade(0f, fadeTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            imageBlackObj.SetActive(false);
        });

        // �w�肳�ꂽ���ԍ��œ�����悤�ɂ���
        yield return new WaitForSeconds(fadeTime + restarningWaitSec);
        gameDirector.characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        player.SetActive(true);
    }
}
