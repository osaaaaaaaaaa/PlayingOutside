//*********************************************************
// Ú‘±æ‚ğİ’è‚·‚éƒ‚ƒfƒ‹
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseModel : MonoBehaviour
{
#if UNITY_EDITOR
    protected const string ServerURL = "http://localhost:7000";
#else
    protected const string ServerURL = "http://ikusei.japaneast.cloudapp.azure.com:7000";
#endif
}
