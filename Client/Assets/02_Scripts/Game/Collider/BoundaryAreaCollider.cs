using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoundaryAreaCollider : MonoBehaviour
{
    #region ��O�ɏo���ۂɁA�����R�C�����h���b�v����͈�
    [SerializeField] Transform rangePointA; // position�̃p�����[�^��S��rangePointB��菬��������
    [SerializeField] Transform rangePointB; // position�̃p�����[�^��S��rangePointA���傫������
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        OnBoundaryAreaCollider(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnBoundaryAreaCollider(collision.gameObject);
    }

    void OnBoundaryAreaCollider(GameObject obj)
    {
        var compornent = obj.gameObject.GetComponent<PlayerController>();
        if (compornent != null)
        {
            if (!compornent.enabled) return;
            PlayerController playerController = obj.gameObject.GetComponent<PlayerController>();
            PlayerAnimatorController playerAnimatorController = obj.gameObject.GetComponent<PlayerAnimatorController>();

            playerController.Respawn();
            playerController.IsControlEnabled = false;
            playerController.IsStandUp = false;
            playerAnimatorController.SetInt(PlayerAnimatorController.ANIM_ID.Respawn);

            obj.gameObject.layer = 8;
            obj.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

            if (rangePointA != null && rangePointB != null) OutOfBoundsAsynk();
        }
    }

    async void OutOfBoundsAsynk()
    {
        if (rangePointA != null && rangePointB != null)
            await RoomModel.Instance.OutOfBoundsAsynk(rangePointA.position, rangePointB.position);
    }
}
