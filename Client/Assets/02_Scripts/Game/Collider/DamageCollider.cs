using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField] int damage = 0;
    public int Damage { get {  return damage; } }
    [SerializeField] Vector3 specifiedKnockback = Vector3.zero;
    public Vector3 SpecifiedKnockback { get { return specifiedKnockback; } }

    public GameObject root { get; private set; }

    private void Awake()
    {
        root = transform.root.gameObject;
    }

    public void SetupDamage(int _damage)
    {
        damage = _damage;
    }
}
