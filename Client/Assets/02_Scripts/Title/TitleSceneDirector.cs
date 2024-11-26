using Server.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneDirector : MonoBehaviour
{
    [SerializeField] GameObject panelRegistUser;
    [SerializeField] InputField inputFieldUserName;

    private async void Start()
    {
        panelRegistUser.SetActive(false);

        // サーバー接続処理
        await RoomModel.Instance.ConnectAsync();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if(RoomModel.Instance.MyUserData == null) panelRegistUser.SetActive(true);
            //else SceneControler.Instance.StartSceneLoad("TopScene");
        }
    }

    /// <summary>
    /// ユーザー登録ボタン
    /// </summary>
    public async void OnRegistUserButton()
    {
        /*        var model = new UserModel();
                var result = await model.RegistUserAsync(inputFieldUserName.text);

                if (!result) inputFieldUserName.text = "";*/

        // 一旦ユーザーIDを格納する処理
        RoomModel.Instance.MyUserData = new User { Id = int.Parse(inputFieldUserName.text) };

        SceneControler.Instance.StartSceneLoad("TopScene");
    }
}
