using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EggWarning : MonoBehaviour
{
    [SerializeField] GameObject eggBulletPrefab;
    GameObject eggBullet;
    Vector3 eggGeneratePoint;
    const float jumpPower = 15;
    bool isGeneratedEgg;

    public void InitParam(Vector3 _eggGeneratePoint)
    {
        eggGeneratePoint = _eggGeneratePoint;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 6 && !isGeneratedEgg)
        {
            isGeneratedEgg = true;
            transform.GetChild(0).gameObject.SetActive(true);   // WarningパーティクルON

            eggBullet = Instantiate(eggBulletPrefab, eggGeneratePoint, Quaternion.identity);
            var defaultScale = eggBullet.transform.localScale;
            eggBullet.transform.localScale = Vector3.zero;

            var sequence = DOTween.Sequence();
            sequence.Append(eggBullet.transform.DOJump(this.transform.position, jumpPower, 1, 2f).SetDelay(0.5f).SetEase(Ease.InBack)
                .OnComplete(() => { Invoke("Destroys", 0.05f); }))   // 早すぎてダメージ判定が取れないため
                .Join(eggBullet.transform.DOScale(defaultScale, 0.5f).SetDelay(1.8f));
            sequence.Play();
        }
    }

    void Destroys()
    {
        eggBullet.GetComponent<EggBullet>().DoExplosion(); 
        Destroy(this.gameObject);
    }
}
