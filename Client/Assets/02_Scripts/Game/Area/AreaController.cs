using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static AreaController;
using Unity.VisualScripting;

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

    #region �I�[�f�B�I�֌W
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
    /// ���݂̃G���A���N���A��������
    /// </summary>
    public IEnumerator CurrentAreaClearCoroutine(GameObject player)
    {
        audioSource.PlayOneShot(clearedAreSE);
        isClearedArea = false;
        bool isLastArea = (areaId == lastAreaId);
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
        Debug.Log(areaId + "�G���A�ړ�����");
        isClearedArea = true;

        DOTween.Kill(imageBlack);
        bool isLastArea = (areaId == lastAreaId);
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

            // ���݂̃G���A��P��
            itemSpawnerList[(int)areaId].enabled = false;
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
        player.SetActive(false);
        areaId++;
        Debug.Log("�G���A��ID�F"+ (int)areaId);

        // ���̃G���A�̏���
        itemSpawnerList[(int)areaId].enabled = true;
        gimmicks[(int)areaId].SetActive(true);

        // ���̃G���A�Ɉړ����� && �J�������Z�b�g�A�b�v
        player.GetComponent<PlayerController>().InitPlayer(startPoints[(int)areaId].transform);
        targetCameraController.InitCamera(player.transform, (int)areaId,RoomModel.Instance.ConnectionId);

        // �t�F�[�h�A�E�g
        imageBlack.DOFade(0f, fadeTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            imageBlackObj.SetActive(false);
        });

        // �w�肳�ꂽ���ԍ��œ�����悤�ɂ���
        yield return new WaitForSeconds(fadeTime + restarningWaitSec);
        isClearedArea = false;
        player.GetComponent<PlayerController>().enabled = true;
        player.SetActive(true);
        player.GetComponent<PlayerController>().InitPlayer();
    }
}
