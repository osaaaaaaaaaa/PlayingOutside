using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopSceneMaanger : MonoBehaviour
{
    [SerializeField] InputField inputFieldUserName;

    /// <summary>
    /// ���[�U�[�o�^�{�^��
    /// </summary>
    public async void OnRegistUserButton()
    {
        var model = new UserModel();
        var result = await model.RegistUserAsync(inputFieldUserName.text);

        if (!result) inputFieldUserName.text = "";
    }
}
