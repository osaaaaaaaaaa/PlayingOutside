//*********************************************************
// セーブデータの型
// Author:Rui Enomoto
//*********************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData
{
    public string Name { get; set; }
    public int UserID { get; set; }
    public string AuthToken { get; set; }
    public bool IsReadTutorial { get; set; }
    public float BGMVolume { get; set; }
    public float SEVolume { get; set; }
}
