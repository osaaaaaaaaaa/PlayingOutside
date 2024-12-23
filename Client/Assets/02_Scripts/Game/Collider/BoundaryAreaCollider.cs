using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoundaryAreaCollider : MonoBehaviour
{
    [SerializeField] Transform rangePointA; // positionのパラメータを全てrangePointBより小さくする
    [SerializeField] Transform rangePointB; // positionのパラメータを全てrangePointAより大きくする

    private async void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>().enabled)
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            PlayerAnimatorController playerAnimatorController = collision.gameObject.GetComponent<PlayerAnimatorController>();

            playerController.InitPlayer();
            playerController.IsControlEnabled = false;
            playerController.IsStandUp = false;
            playerAnimatorController.SetInt(PlayerAnimatorController.ANIM_ID.Respawn);

            collision.gameObject.layer = 8;
            collision.gameObject.transform.position = playerController.respawnPoint;
            collision.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

            await RoomModel.Instance.OnOutOfBoundsAsynk(rangePointA.position, rangePointB.position);
        }
    }
}
