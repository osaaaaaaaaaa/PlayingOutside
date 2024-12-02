using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCameraController : MonoBehaviour
{
    [SerializeField] List<Vector3> rotate;
    [SerializeField] List<Vector3> followOffset;

    CinemachineVirtualCamera camera;
    CinemachineTransposer cameraTransposer;

    [SerializeField] int debug_areaId = 0;

    private void Awake()
    {
        camera = GetComponent<CinemachineVirtualCamera>();
        cameraTransposer = camera.GetCinemachineComponent<CinemachineTransposer>(); ;

        if(debug_areaId > 0)
        {
            InitCamera(null, debug_areaId);
        }
    }

    public void InitCamera(Transform target,int areaId)
    {
        if (target != null)
        {
            camera.Follow = target;
            camera.LookAt = target;
        }

        transform.eulerAngles = rotate[areaId];
        cameraTransposer.m_FollowOffset = followOffset[areaId];
    }
}
