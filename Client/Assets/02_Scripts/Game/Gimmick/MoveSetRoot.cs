using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveSetRoot : MonoBehaviour
{
    [SerializeField] List<Vector3> root;
    [SerializeField] float animSec;

    // Start is called before the first frame update
    void Start()
    {
        Vector3[] path = new Vector3[root.Count];
        for (int i = 0; i < path.Length; i++) 
        {
            path[i] = root[i];
        }

        this.transform.DOLocalPath(path, animSec).SetLookAt(0.01f).SetEase(Ease.Linear).SetLoops(-1,LoopType.Restart);
    }
}
