using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ChickenGimmick : MonoBehaviour
{
    [SerializeField] GameObject eggWarningPrefab;
    [SerializeField] Transform minRange;
    [SerializeField] Transform maxRange;
    List<GameObject> targetList = new List<GameObject>();
    Animator animator;
    Coroutine coroutine;
    const float jumpHeight = 1.8f;
    const float rotateNum = 3;
    const float rotateAnimSec = 0.4f;
    const int maxEggNum = 3;

    private void Start()
    {
        animator = GetComponent<Animator>();   
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
        targetList.Clear();
        coroutine = null;
    }

    public void GenerateEggBulletWarning(Vector3[] points)
    {
        foreach (var point in points)
        {
            var eggWarning = Instantiate(eggWarningPrefab);
            eggWarning.transform.position = point;
            eggWarning.GetComponent<EggWarning>().InitParam(transform.position + Vector3.up * jumpHeight);
        }

        animator.Play("LayEgg", 0, 0);
    }

    IEnumerator SetupEggBulletWarningPoints(float waitSec)
    {
        while (targetList.Count > 0)
        {
            var points = GetWarningPoints();
            if (points != null && points.Count > 0)
            {
                if (RoomModel.Instance.userState == RoomModel.USER_STATE.joined)
                {
                    if (RoomModel.Instance.JoinedUsers[RoomModel.Instance.ConnectionId].IsMasterClient) 
                        this.PlayAnimalGimmickAsynk(this.transform.parent.name,points.ToArray());
                }
                else
                {
                    GenerateEggBulletWarning(points.ToArray());
                }
            }

            yield return new WaitForSeconds(waitSec);
        }

        coroutine = null;
    }

    public void Rotate()
    {
        transform.parent.DORotate(Vector3.back * 360 * rotateNum, rotateAnimSec, RotateMode.FastBeyond360).SetEase(Ease.Linear);
    }

    List<Vector3> GetWarningPoints()
    {
        List<Vector3> points = new List<Vector3>();
        List<GameObject> removeTarget = new List<GameObject>();

        const float maxDis = 8f;
        foreach (var target in targetList)
        {
            if(target == null || !target.activeSelf)
            {
                removeTarget.Add(target);
                continue;
            }

            int eggNum = Random.Range(2, maxEggNum);
            for (int i = 0; i < eggNum; i++)
            {
                bool isSucsess = false;
                while (!isSucsess)
                {
                    float pointX = Random.Range(-maxDis, maxDis);
                    float pointZ = Random.Range(-maxDis, maxDis);

                    Vector3 pointVec;
                    Vector3 futurePredictionPoint = target.transform.position + Vector3.Scale(target.transform.forward.normalized, Vector3.forward) * maxDis / 2;
                    pointVec = new Vector3(pointX + futurePredictionPoint.x, transform.position.y, pointZ + futurePredictionPoint.z);

                    if (pointVec.x >= minRange.position.x && pointVec.x <= maxRange.position.x
                        && pointVec.z >= minRange.position.z && pointVec.z <= maxRange.position.z)
                    {
                        isSucsess = true;
                        points.Add(pointVec);
                    }
                }
            }
        }

        foreach (var target in removeTarget)
        {
            targetList.Remove(target);
        }

        return points;
    }

    async void PlayAnimalGimmickAsynk(string name, Vector3[] points)
    {
        await RoomModel.Instance.PlayAnimalGimmickAsynk(EnumManager.ANIMAL_GIMMICK_ID.Chicken, name, points);
    }

    private void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponent<PlayerController>();
        if (controller != null && !targetList.Contains(other.gameObject))
        {
            targetList.Add(other.gameObject);
            if (coroutine == null) coroutine = StartCoroutine(SetupEggBulletWarningPoints(4f));
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
