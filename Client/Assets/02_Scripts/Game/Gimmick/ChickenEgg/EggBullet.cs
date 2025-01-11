using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EggBullet : MonoBehaviour
{
    [SerializeField] GameObject explosionParticle;
    public void DoExplosion()
    {
        Instantiate(explosionParticle, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
