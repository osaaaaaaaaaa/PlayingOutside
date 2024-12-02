using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCameraController : MonoBehaviour
{
    CinemachineVirtualCamera camera;

    private void Awake()
    {
        camera = GetComponent<CinemachineVirtualCamera>();
    }

    public void InitTarget(Transform target)
    {
        camera.Follow = target;
        camera.LookAt = target;
    }
}
