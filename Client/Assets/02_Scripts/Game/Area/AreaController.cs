using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static AreaController;
using Unity.VisualScripting;
using Shared.Interfaces.Model.Entity;

public class AreaController : MonoBehaviour
{
    #region �R���g���[���[�֌W
    [SerializeField] RelayGameDirector gameDirector;
    [SerializeField] TargetCameraController targetCameraController;
    [SerializeField] List<ItemSpawner> itemSpawnerList;
    public List<ItemSpawner> ItemSpawnerList { get { return itemSpawnerList; } }
    #endregion

    [SerializeField] List<GameObject> startPoints;    // �e�G���A�̃X�^�[�g�n�_
    [SerializeField] List<GameObject> gimmicks;       // �G���A���̃M�~�b�N
    
    #region UI�֌W
    [SerializeField] GameObject finishUI;
    [SerializeField] GameObject spectatingUI;
    [SerializeField] GameObject imageBlackObj;
    Image imageBlack;
    const float fadeTime = 0.5f;
    #endregion

    SEController seController;

    public bool isClearedArea { get; private set; }


    public EnumManager.RELAY_AREA_ID currentAreaId { get; set; } = EnumManager.RELAY_AREA_ID.Area1_Mud;

    private void Awake()
    {
        imageBlack = imageBlackObj.GetComponent<Image>();
        seController = GetComponent<SEController>();
        isClearedArea = false;

        foreach (var item in itemSpawnerList)
        {
            item.enabled = false;
        }
        itemSpawnerList[(int)currentAreaId].enabled = true;

        if (!gameDirector.isDebug)
        {
            ToggleAllGimmicks(false);
            gimmicks[(int)currentAreaId].SetActive(true);
        }
    }

    public void ToggleAllGimmicks(bool isActive)
    {
        foreach(var gimmick in gimmicks)
        {
            gimmick.SetActive(isActive);
        }
    }

    /// <summary>
    /// ���݂̃G���A���N���A��������
    /// </summary>
    public IEnumerator CurrentAreaClearCoroutine(GameObject player)
    {
        seController.PlayAudio();
        isClearedArea = false;
        bool isLastArea = (currentAreaId == EnumManager.LastAreaId);
        // �T�[�o�[�Ȃ��̏ꍇ�̂ݎg�p�A�ŏI�I��isDebug���폜
        if (gameDirector.isDebug)
        {
            if (isLastArea)
            {
                Debug.Log("�S�[��!!");
            }
            else
            {
                isClearedArea = true;
                StartCoroutine(RestarningGameCoroutine(currentAreaId + 1, player, 1));
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
        Debug.Log(currentAreaId + "�G���A�ړ�����");
        isClearedArea = true;

        DOTween.Kill(imageBlack);
        bool isLastArea = (currentAreaId == EnumManager.LastAreaId);
        float animSec = (imageBlackObj.activeSelf) ? 0f : fadeTime;

        // ����𖳌�������
        gameDirector.characterList[RoomModel.Instance.ConnectionId].SetActive(false);

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

            // ���݂̃G���A��P��
            itemSpawnerList[(int)currentAreaId].enabled = false;
            gimmicks[(int)currentAreaId].SetActive(false);

            // ���̃G���A�Ɉړ����鏀���������������N�G�X�g
            gameDirector.OnReadyNextArea(isLastArea);
        });
    }

    /// <summary>
    /// �t�F�[�h�A�E�g��ɃQ�[���ĊJ
    /// </summary>
    public IEnumerator RestarningGameCoroutine(EnumManager.RELAY_AREA_ID nextAreaId,GameObject player ,float restarningWaitSec)
    {
        if (!isClearedArea) yield break;
        currentAreaId = nextAreaId;
        Debug.Log("�G���A��ID�F"+ (int)currentAreaId);

        // ���̃G���A�̏���
        itemSpawnerList[(int)currentAreaId].enabled = true;
        gimmicks[(int)currentAreaId].SetActive(true);

        // ���̃G���A�Ɉړ����� && �J�������Z�b�g�A�b�v
        player.GetComponent<PlayerController>().InitPlayer(startPoints[(int)currentAreaId].transform);
        targetCameraController.InitCamera(player.transform, (int)currentAreaId,RoomModel.Instance.ConnectionId);

        // �t�F�[�h�A�E�g
        imageBlack.DOFade(0f, fadeTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            imageBlackObj.SetActive(false);
        });

        // �w�肳�ꂽ���ԍ��œ�����悤�ɂ���
        yield return new WaitForSeconds(fadeTime + restarningWaitSec);
        isClearedArea = false;
        player.SetActive(true);
    }
}
