using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.TextCore.Text;

public class TargetCameraController : MonoBehaviour
{
    #region ���Z�w�J���g���[�����[�x�̊e�G���A���̃J�����ݒ�
    [SerializeField] List<Vector3> rotate;
    [SerializeField] List<Vector3> followOffset;
    #endregion

    [SerializeField] GameDirector gameDirector;
    CinemachineVirtualCamera cameraVirtual;
    CinemachineTransposer cameraTransposer;

    int targetIndex;  // ���ݒǂ��Ă���^�[�Q�b�g�̃C���f�b�N�X�ԍ�
    public Guid currentTargetId { get; private set; }   // ���ݒǂ��Ă���^�[�Q�b�g��Key
    public int activeTargetCnt { get; private set; }    // �؂�ւ��邱�Ƃ��ł���^�[�Q�b�g�̑Ώۂ̐�

    [SerializeField] int debug_areaId = 0;


    private void Awake()
    {
        targetIndex = 0;
        cameraVirtual = GetComponent<CinemachineVirtualCamera>();
        cameraTransposer = cameraVirtual.GetCinemachineComponent<CinemachineTransposer>();

        if(debug_areaId > 0)
        {
            InitCamera(null, debug_areaId,new Guid());
        }
    }

    IEnumerator ResetDamping(float defDampingX,float defDampingY, float defDampingZ)
    {
        yield return null;
        cameraTransposer.m_XDamping = defDampingX;
        cameraTransposer.m_YDamping = defDampingY;
        cameraTransposer.m_ZDamping = defDampingZ;
    }

    public void InitCamera(Transform target,int areaId, Guid targetId)
    {
        if (target != null)
        {
            // ����Damping��ێ�
            float defDampingX = cameraTransposer.m_XDamping;
            float defDampingY = cameraTransposer.m_YDamping;
            float defDampingZ = cameraTransposer.m_ZDamping;

            // Damping���ꎞ�I�Ƀ��Z�b�g
            cameraTransposer.m_XDamping = 0;
            cameraTransposer.m_YDamping = 0;
            cameraTransposer.m_ZDamping = 0;

            // �^�[�Q�b�g����U�������ďu���Ɉړ�
            cameraVirtual.Follow = null;
            cameraVirtual.LookAt = null;
            transform.position = target.position + cameraTransposer.m_FollowOffset;

            // �^�[�Q�b�g���Đݒ�
            cameraVirtual.Follow = target;
            cameraVirtual.LookAt = target;
            currentTargetId = targetId;

            // �x�����s��Damping�����ɖ߂�
            StartCoroutine(ResetDamping(defDampingX, defDampingY, defDampingZ));
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

        // �L�����N�^�[��Key���擾
        Guid[] guidCharacters = new Guid[gameDirector.characterList.Count];
        guidCharacters = gameDirector.characterList.Keys.ToArray();

        // �^�[�Q�b�g�̌����J�n
        int tmpTargetIndex = targetIndex;
        for (int i = 0; i < guidCharacters.Length; i++)
        {
            // key���擾
            tmpTargetIndex++;
            tmpTargetIndex = tmpTargetIndex < guidCharacters.Length ? tmpTargetIndex : 0;
            Guid key = guidCharacters[tmpTargetIndex];

            if (!isSucsess && key != RoomModel.Instance.ConnectionId
                && currentTargetId != key && gameDirector.characterList[key].activeSelf)
            {
                // �J�����̃^�[�Q�b�g�؂�ւ�
                InitCamera(gameDirector.characterList[key].transform, 0, key);
                isSucsess = true;
                targetIndex = tmpTargetIndex;
            }

            if (gameDirector.characterList[key].activeSelf) activeTargetCnt++;
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
