using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] GameObject textName;

    // Update is called once per frame
    void Update()
    {
        textName.transform.LookAt(Camera.main.transform);
    }

    public void InitUI(string name,Color color)
    {
        textName.GetComponent<Text>().text = name;
        textName.GetComponent<Text>().color = color;
    }
}
