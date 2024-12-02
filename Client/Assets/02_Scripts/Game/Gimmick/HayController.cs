using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HayController : MonoBehaviour
{
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
        yield return new WaitForSeconds(waitSeconds);

        while (true) 
        {
            yield return new WaitForSeconds(1f);
            if (hayObj == null)
            {
                hayObj = Instantiate(hayPrefab, transform);
                hayObj.GetComponent<Hay>().Init(addForse);
            }
        }
    }
}
