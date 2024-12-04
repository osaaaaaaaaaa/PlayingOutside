using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalResultSceneDirector : MonoBehaviour
{
    #region �L�����N�^�[�֌W
    [SerializeField] List<Transform> characterStartPoints;
    [SerializeField] GameObject characterPrefab;
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();  // ���[�U�[�̃L�����N�^�[���
    #endregion

    [SerializeField] FinalResultSceneParticleController particleController;
    [SerializeField] TotalScoreUIController totalScoreUIController;
    [SerializeField] GameObject crownPrefab;
    [SerializeField] GameObject btnLeave;

    const float waitSeconds = 0.1f;

    private void Start()
    {
        // �֐���o�^����
        RoomModel.Instance.OnLeavedUser += this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser += this.NotifyUpdatedPlayerState;
        RoomModel.Instance.OnTransitionFinalResultSceneUser += this.NotifyTransitionFinalResultSceneAllUsers;

        SetupScene();

        // �ŏI���ʔ��\�V�[���ɑJ�ڊ������N�G�X�g
        TransitionFinalResultScene();
    }

    void OnDisable()
    {
        // �V�[���J�ڎ��Ɋ֐��̓o�^������
        RoomModel.Instance.OnLeavedUser -= this.NotifyLeavedUser;
        RoomModel.Instance.OnUpdatePlayerStateUser -= this.NotifyUpdatedPlayerState;
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            UpdatePlayerState();
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    void SetupScene()
    {
        GenerateCharacters();

        // ���[�h��ʂ����
        SceneControler.Instance.StopSceneLoad();
    }

    /// <summary>
    /// �L�����N�^�[��������
    /// </summary>
    void GenerateCharacters()
    {
        var users = RoomModel.Instance.JoinedUsers;

        foreach (var user in users)
        {
            // �L�����N�^�[����,
            GameObject character = Instantiate(characterPrefab);
            characterList[user.Key] = character;

            // �v���C���[�̏���������
            bool isMyCharacter = user.Key == RoomModel.Instance.ConnectionId;
            character.GetComponent<PlayerController>().InitPlayer(characterStartPoints[user.Value.JoinOrder - 1]);

            // ���[�U�[���̏���������
            Color colorText = isMyCharacter ? Color.white : Color.green;
            character.GetComponent<PlayerUIController>().InitUI(user.Value.UserData.Name, colorText);

            // �S�����V�[���J�ڊ�������܂�PlayerController���O��
            character.GetComponent<PlayerController>().enabled = false;

            // ���C���[�^�O��ύX
            character.layer = isMyCharacter ? 3 : 7;
        }
    }

    /// <summary>
    /// �ގ����N�G�X�g
    /// </summary>
    public async void LeaveRoom()
    {
        StopCoroutine(UpdateCoroutine());
        await RoomModel.Instance.LeaveAsync();

        SceneControler.Instance.StartSceneLoad("TopScene");
    }

    /// <summary>
    /// �ގ��ʒm����
    /// </summary>
    void NotifyLeavedUser(Guid connectionId)
    {
        if (connectionId == RoomModel.Instance.ConnectionId)
        {
            // �������ޏo����ꍇ�͑S�č폜
            foreach (var character in characterList.Values)
            {
                Destroy(character);
            }
            characterList.Clear();
        }
        else
        {
            // �Y���̃L�����N�^�[�폜&���X�g����폜
            Destroy(characterList[connectionId]);
            characterList.Remove(connectionId);
        }
    }

    /// <summary>
    /// �v���C���[���X�V���N�G�X�g
    /// </summary>
    public async void UpdatePlayerState()
    {
        if (!characterList.ContainsKey(RoomModel.Instance.ConnectionId)) return;   // �����̑��݃`�F�b�N
        var character = characterList[RoomModel.Instance.ConnectionId];
        PlayerState playerState = new PlayerState()
        {
            position = character.transform.position,
            angle = character.transform.eulerAngles,
            animationId = character.GetComponent<PlayerAnimatorController>().GetAnimId(),
            isActiveSelf = true,
        };
        await RoomModel.Instance.UpdatePlayerStateAsync(playerState);
    }

    /// <summary>
    /// �v���C���[���X�V�ʒm����
    /// </summary>
    /// <param name="user"></param>
    void NotifyUpdatedPlayerState(Guid connectionId, PlayerState playerState)
    {
        if (!characterList.ContainsKey(connectionId)) return;   // �v���C���[�̑��݃`�F�b�N

        // �ړ��E��]�E�A�j���[�V��������
        characterList[connectionId].transform.DOMove(playerState.position, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].transform.DORotate(playerState.angle, waitSeconds).SetEase(Ease.Linear);
        characterList[connectionId].GetComponent<PlayerAnimatorController>().SetInt(playerState.animationId);
    }

    /// <summary>
    /// �ŏI���ʔ��\�V�[���ɑJ�ڂ�������
    /// </summary>
    async void TransitionFinalResultScene()
    {
        await RoomModel.Instance.OnTransitionFinalResultSceneAsynk();
    }

    /// <summary>
    /// �S�����J�ڂł����ʒm
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="playerState"></param>
    void NotifyTransitionFinalResultSceneAllUsers(ResultData[] result)
    {
        StartCoroutine(UpdateCoroutine());

        Guid winnerId = new Guid();
        foreach (ResultData resultData in result)
        {
            if (resultData.rank == 1)
            {
                winnerId = resultData.connectionId;
                break;
            }
        }
        StartCoroutine(ShowResultsCoroutine(result, winnerId));
    }

    /// <summary>
    /// ���ʔ��\�J�n�R���[�`��
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowResultsCoroutine(ResultData[] result,Guid winnerId)
    {
        int[] scores = new int[result.Length];
        for (int i = 0; i < result.Length; i++) 
        {
            scores[i] = result[i].score;
        }
        totalScoreUIController.Init(characterList.Count, scores);
        yield return new WaitForSeconds(1f);

        totalScoreUIController.PlayAnim();
        yield return new WaitForSeconds(3f);

        totalScoreUIController.StopAnim();
        yield return new WaitForSeconds(0.5f);

        // �P�ʂ̃v���C���[�ɉ������Z�b�g����
        Instantiate(crownPrefab, characterList[winnerId].transform);
        yield return new WaitForSeconds(1f);    // �����̃A�j���[�V�������I�����鎞��

        particleController.GenerateParticles(characterList[winnerId].transform);
        yield return new WaitForSeconds(0.5f);

        // �v���C���[�̑�����ł���悤�ɂ���
        characterList[RoomModel.Instance.ConnectionId].GetComponent<PlayerController>().enabled = true;
        // �ގ����ł���悤�ɂ���
        btnLeave.SetActive(true);
    }
}
