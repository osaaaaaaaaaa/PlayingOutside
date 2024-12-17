using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    public int damage;
    public void SetupDamage(int _damage)
    {
        damage = _damage;
    }
}
