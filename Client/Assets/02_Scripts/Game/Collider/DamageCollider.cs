using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField] int damage = 0;
    public int Damage { get {  return damage; } }
    [SerializeField] Vector3 specifiedKnockback = Vector3.zero;
    public Vector3 SpecifiedKnockback { get { return specifiedKnockback; } }

    public void SetupDamage(int _damage)
    {
        damage = _damage;
    }
}
