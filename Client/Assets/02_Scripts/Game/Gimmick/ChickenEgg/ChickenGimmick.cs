using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ChickenGimmick : MonoBehaviour
{
    [SerializeField] GameObject eggWarningPrefab;
    [SerializeField] Transform minRange;
    [SerializeField] Transform maxRange;
    [SerializeField] int maxEggNum;
    List<GameObject> targetList = new List<GameObject>();
    Animator animator;
    const float jumpHeight = 1.8f;
    const float rotateNum = 3;
    const float rotateAnimSec = 0.4f;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.V))
        {
            animator.Play("LayEgg");
        }
    }

    public void GenerateEggBulletWarning()
    {
        var points = GetWarningPoints();
        if (points != null)
        {
            foreach (var point in points)
            {
                var eggWarning = Instantiate(eggWarningPrefab);
                eggWarning.transform.position = point;
                eggWarning.GetComponent<EggWarning>().InitParam(transform.position + Vector3.up * jumpHeight);
            }
        }
    }

    public void Rotate()
    {
        transform.parent.DORotate(Vector3.back * 360 * rotateNum, rotateAnimSec, RotateMode.FastBeyond360).SetEase(Ease.Linear);
    }

    List<Vector3> GetWarningPoints()
    {
        List<Vector3> points = new List<Vector3>();

        const float maxDis = 8f;
        foreach (var target in targetList)
        {
            int eggNum = Random.Range(0, maxEggNum);
            for (int i = 0; i < eggNum; i++)
            {
                bool isSucsess = false;
                while (!isSucsess)
                {
                    float pointX = Random.Range(-maxDis, maxDis);
                    float pointZ = Random.Range(-maxDis, maxDis);
                    Vector3 pointVec = target.transform.position + new Vector3(pointX, transform.position.y, pointZ);
                    if (pointVec.x >= minRange.position.x && pointVec.x <= maxRange.position.x
                        && pointVec.z >= minRange.position.z && pointVec.z <= maxRange.position.z)
                    {
                        isSucsess = true;
                        points.Add(pointVec);
                    }
                }
            }
        }

        return points;
    }

    private void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponent<PlayerController>();
        if (controller != null && !targetList.Contains(other.gameObject))
        {
            targetList.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var controller = other.GetComponent<PlayerController>();
        if (controller != null && targetList.Contains(other.gameObject))
        {
            targetList.Remove(other.gameObject);
        }
    }
}
