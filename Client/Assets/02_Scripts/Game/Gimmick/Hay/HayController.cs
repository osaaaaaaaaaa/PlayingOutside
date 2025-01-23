using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HayController : MonoBehaviour
{
    [SerializeField] RelayGameDirector gameDirector;
    [SerializeField] GameObject hayPrefab;
    GameObject hayObj;
    public float waitSeconds;
    public Vector3 addForse;

    private void Start()
    {
        StartCoroutine(GenerateCoroutine());
    }

    IEnumerator GenerateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitSeconds);
            hayObj = Instantiate(hayPrefab, transform);
            hayObj.GetComponent<Hay>().Init(addForse);
        }
    }
}
