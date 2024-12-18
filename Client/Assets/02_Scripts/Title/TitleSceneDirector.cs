using Server.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneDirector : MonoBehaviour
{
    [SerializeField] GameObject panelRegistUser;
    [SerializeField] Button btnRegist;
    [SerializeField] InputField inputFieldUserName;
    [SerializeField] InputField inputFieldDebugId;
    [SerializeField] Text textError;
    bool isDebug;
    bool isLocalData;

    private void Start()
    {
        Application.targetFrameRate = 60;

        isDebug = false;
        isLocalData = false;
        SceneControler.Instance.StopSceneLoad();
        panelRegistUser.SetActive(false);
    }

    // Update is called once per frame
    async void Update()
    {
        //if (!isDebug && !isLocalData && Input.GetMouseButtonUp(0) && !panelRegistUser.activeSelf)
        //{
        //    isLocalData = UserModel.Instance.LoadUserData();

        //    // ローカルにあるユーザー情報を取得する
        //    if (isLocalData)
        //    {
        //        // ユーザー情報取得処理
        //        var error = await UserModel.Instance.ShowUserAsync(UserModel.Instance.UserId);

        //        if (error != null) 
        //        {
        //            ErrorUIController.Instance.ShowErrorUI(error);
        //            return;
        //        }
        //        SceneControler.Instance.StartSceneLoad("TopScene");
        //    }
        //    else
        //    {
        //        // ユーザー登録用のUIを表示する
        //        panelRegistUser.SetActive(true);
        //    }
        //}
    }

    public void OnEnterDebugUI()
    {
        isDebug = true;
    }

    /// <summary>
    /// [デバッグ用] 指定したユーザーIDを元にログイン
    /// </summary>
    public async void OnDebugButtonAsync()
    {
        int userId;
        if(int.TryParse(inputFieldDebugId.text, out userId))
        {
            // ユーザー情報取得処理
            var error = await UserModel.Instance.ShowUserAsync(userId);
            if (error != null)
            {
                ErrorUIController.Instance.ShowErrorUI(error);
                return;
            }

            // シーン遷移開始
            SceneControler.Instance.StartSceneLoad("TopScene");
        }
    }

    /// <summary>
    /// ユーザー登録ボタン
    /// </summary>
    public async void OnRegistUserButton()
    {
        btnRegist.interactable = false;
        var result = await UserModel.Instance.RegistUserAsync(inputFieldUserName.text);

        if (result != null)
        {
            textError.text = result;
            btnRegist.interactable= true;
            return;
        }

        // シーン遷移開始
        SceneControler.Instance.StartSceneLoad("TopScene");
    }
}
