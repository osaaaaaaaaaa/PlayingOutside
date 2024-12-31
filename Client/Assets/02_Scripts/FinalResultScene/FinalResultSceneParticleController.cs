using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalResultSceneParticleController : MonoBehaviour
{
    [SerializeField] GameObject confettiFallingPrefab;
    [SerializeField] GameObject sparksPrefab;

    public void GenerateSparksParticles(Transform character)
    {
        var sparks1 = Instantiate(sparksPrefab, character);
        var sparks2 = Instantiate(sparksPrefab, character);
        sparks1.transform.localPosition = new Vector3(0.4f, 0, -0.5f);
        sparks2.transform.localPosition = new Vector3(-0.4f, 0, -0.5f);
    }

    public void GenarateConfettiParticle()
    {
        var confetti = Instantiate(confettiFallingPrefab);
        confetti.transform.position = Vector3.up * 5;
    }
}
