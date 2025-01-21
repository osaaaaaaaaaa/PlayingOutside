using DG.Tweening;
using Shared.Interfaces.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ChickenGimmick : MonoBehaviour
{
    #region �e�̒��e�n�_��Prefab�Ɛ����͈�
    [SerializeField] GameObject eggWarningPrefab;
    [SerializeField] Transform minRange;
    [SerializeField] Transform maxRange;
    #endregion

    #region ��Q���̂���͈�
    public List<Vector3> obstaclesMaxRange = new List<Vector3>();
    public List<Vector3> obstaclesMinRange = new List<Vector3>();
    #endregion

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
        if(coroutine != null) StopCoroutine(coroutine);
        targetList.Clear();
        coroutine = null;
    }

    /// <summary>
    /// �e�̒��e�n�_�𐶐�����
    /// </summary>
    /// <param name="points"></param>
    public void GenerateEggBulletWarning(Vector3[] points)
    {
        foreach (var point in points)
        {
            var eggWarning = Instantiate(eggWarningPrefab);
            eggWarning.transform.localRotation = transform.localRotation;
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
        var rotate = transform.parent.eulerAngles;
        transform.parent.DORotate(rotate + Vector3.back * 360 * rotateNum, rotateAnimSec, RotateMode.FastBeyond360).SetEase(Ease.Linear);
    }

    /// <summary>
    /// �e�̒��e�n�_���擾����
    /// </summary>
    /// <returns></returns>
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
                        && pointVec.z >= minRange.position.z && pointVec.z <= maxRange.position.z
                        && !IsOverlappingObstacle(pointVec))
                    {
                        isSucsess = true;
                        points.Add(pointVec);
                    }
                    //else
                    //{
                    //    if(pointVec.x <= minRange.position.x) pointVec.x = minRange.position.x;
                    //    if(pointVec.x >= maxRange.position.x) pointVec.x = maxRange.position.x;
                    //    if(pointVec.z <= minRange.position.z) pointVec.z = minRange.position.z;
                    //    if(pointVec.z >= maxRange.position.z) pointVec.z = maxRange.position.z;
                    //    isSucsess = true;
                    //    points.Add(pointVec);
                    //}
                }
            }
        }

        foreach (var target in removeTarget)
        {
            targetList.Remove(target);
        }

        return points;
    }

    /// <summary>
    /// ��Q���ƍ��W�����Ԃ��Ă��Ȃ����`�F�b�N
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    bool IsOverlappingObstacle(Vector3 point)
    {
        for(int i = 0; i < obstaclesMaxRange.Count; i++)
        {
            if(point.x <= obstaclesMaxRange[i].x && point.z <= obstaclesMaxRange[i].z
                && point.x >= obstaclesMinRange[i].x && point.z >= obstaclesMinRange[i].z)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// ��Q���̒��_���W(0:max,1:min)���擾����
    /// </summary>
    /// <returns></returns>
    Vector3[] GetObstacleRanges(GameObject obstacle)
    {
        // �I�u�W�F�N�g��MeshFilter�������Ă��邱�Ƃ��m�F
        MeshFilter meshFilter = obstacle.GetComponent<MeshFilter>();
        if (meshFilter == null) return null;

        // Mesh�����擾
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3 maxRange = Vector3.zero;
        Vector3 minRange = Vector3.zero;

        // ���[���h���W�ɕϊ����Ē��_�̈ʒu���o��
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPosition = obstacle.transform.TransformPoint(vertices[i]); // ���[���h���W�ɕϊ�

            // ���񎞂̂ݐݒ�
            if(maxRange == Vector3.zero) maxRange = worldPosition;
            if(minRange == Vector3.zero) minRange = worldPosition;

            // �ő�ƍŏ��̒��_���擾����
            if (maxRange.x < worldPosition.x) maxRange.x = worldPosition.x;
            if (maxRange.z < worldPosition.z) maxRange.z = worldPosition.z;
            if (minRange.x > worldPosition.x) minRange.x = worldPosition.x;
            if (minRange.z > worldPosition.z) minRange.z = worldPosition.z;
        }

        vertices = new Vector3[2];
        vertices[0] = maxRange;
        vertices[1] = minRange;
        return vertices;
    }

    /// <summary>
    /// [�}�X�^�[�N���C�A���g�p]
    /// �e�̒��e�n�_�����L����
    /// </summary>
    /// <param name="name"></param>
    /// <param name="points"></param>
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
        else if(other.gameObject.tag == "Obstacle")
        {
            var ranges = GetObstacleRanges(other.gameObject);
            if(ranges != null)
            {
                obstaclesMaxRange.Add(ranges[0]);
                obstaclesMinRange.Add(ranges[1]);
                Debug.Log("max:" + ranges[0]);
                Debug.Log("min:" + ranges[1]);
            }
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
