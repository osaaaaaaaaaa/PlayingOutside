using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCameraController : MonoBehaviour
{
    CinemachineVirtualCamera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<CinemachineVirtualCamera>();
    }

    void InitTarget(Transform target)
    {
        camera.Follow = target;
        camera.LookAt = target;
    }
}
