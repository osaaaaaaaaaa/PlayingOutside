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

    private void Start()
    {
        SceneControler.Instance.StopSceneLoad();
        panelRegistUser.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0) && !panelRegistUser.activeSelf)
        {
            if(RoomModel.Instance.MyUserData == null) panelRegistUser.SetActive(true);
            else SceneControler.Instance.StartSceneLoad("TopScene");
        }
    }

    /// <summary>
    /// ���[�U�[�o�^�{�^��
    /// </summary>
    public async void OnRegistUserButton()
    {
        /*        var model = new UserModel();
                var result = await model.RegistUserAsync(inputFieldUserName.text);

                if (!result) inputFieldUserName.text = "";*/

        // ��U���[�U�[ID���i�[���鏈��
        RoomModel.Instance.MyUserData = new User { Id = int.Parse(inputFieldUserName.text) };
        // �V�[���J�ڊJ�n
        SceneControler.Instance.StartSceneLoad("TopScene");
    }
}
