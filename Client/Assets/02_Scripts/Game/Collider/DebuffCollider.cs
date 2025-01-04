using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffCollider : MonoBehaviour
{
    [SerializeField] bool isSpeedDown;
    public bool IsSpeedDown { get { return isSpeedDown; } }
    [SerializeField] float debuffTime;
    public float DebuffTime { get { return debuffTime; } }
}
