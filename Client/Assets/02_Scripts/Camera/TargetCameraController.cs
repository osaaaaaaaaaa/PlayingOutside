using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCameraController : MonoBehaviour
{
    #region ���Z�w�J���g���[�����[�x�̊e�G���A���̃J�����ݒ�
    [SerializeField] List<Vector3> rotate;
    [SerializeField] List<Vector3> followOffset;
    #endregion

    [SerializeField] GameDirector gameDirector;
    CinemachineVirtualCamera camera;
    CinemachineTransposer cameraTransposer;

    public Guid currentTargetId { get; private set; }   // ���ݒǂ��Ă���^�[�Q�b�g
    public int activeTargetCnt { get; private set; }    // �؂�ւ��邱�Ƃ��ł���^�[�Q�b�g�̑Ώۂ̐�

    [SerializeField] int debug_areaId = 0;

    private void Awake()
    {
        camera = GetComponent<CinemachineVirtualCamera>();
        cameraTransposer = camera.GetCinemachineComponent<CinemachineTransposer>(); ;

        if(debug_areaId > 0)
        {
            InitCamera(null, debug_areaId,new Guid());
        }
    }

    public void InitCamera(Transform target,int areaId, Guid targetId)
    {
        if (target != null)
        {
            // ��u�Ń^�[�Q�b�g�̎��_�ɓ���ւ���
            camera.Follow = null;
            camera.LookAt = null;
            transform.position = target.position + cameraTransposer.m_FollowOffset;

            // �^�[�Q�b�g�̐ݒ�
            camera.Follow = target;
            camera.LookAt = target;
            currentTargetId = targetId;
        }

        if (rotate.Count == 0 || followOffset.Count == 0 || areaId == 0) return;

        // ���Z�w�J���g���[�����[�x�Ŏg�p
        transform.eulerAngles = rotate[areaId];
        cameraTransposer.m_FollowOffset = followOffset[areaId];
    }

    /// <summary>
    /// �J�����̃^�[�Q�b�g�̐؂�ւ����T��&&�؂�ւ���
    /// </summary>
    /// <returns></returns>
    public bool SearchAndChangeTarget()
    {
        bool isSucsess = false;
        activeTargetCnt = 0;
        foreach (var character in gameDirector.characterList)
        {
            if (!isSucsess && character.Key != RoomModel.Instance.ConnectionId
                && currentTargetId != character.Key && character.Value.activeSelf)
            {
                // �J�����̃^�[�Q�b�g�؂�ւ�
                InitCamera(character.Value.transform, 0, character.Key);

                isSucsess = true;
            }

            if (character.Value.activeSelf) activeTargetCnt++;
        }

        return isSucsess;
    }

    /// <summary>
    /// (����������)���Ƀ^�[�Q�b�g�ƂȂ�v���C���[�����邩�`�F�b�N
    /// </summary>
    /// <returns></returns>
    public bool IsOtherTarget()
    {
        bool isSucsess = false;
        activeTargetCnt = 0;
        foreach (var character in gameDirector.characterList)
        {
            if (!isSucsess && character.Key != RoomModel.Instance.ConnectionId
                && currentTargetId != character.Key && character.Value.activeSelf)
            {
                isSucsess = true;
            }

            if (character.Value.activeSelf) activeTargetCnt++;
        }

        return isSucsess;
    }
}
