using Server.Model.Entity;
using Shared.Interfaces.Model.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TitleSceneDirector : MonoBehaviour
{
    [SerializeField] GameObject debugUiParent;
    [SerializeField] InputField inputFieldDebugId;
    [SerializeField] bool isDebug;
    bool isLocalData;
    bool isRanning;

    private void Start()
    {
        Application.targetFrameRate = 60;

        isLocalData = false;
        isRanning = false;
        SceneControler.Instance.StopSceneLoad();

        debugUiParent.SetActive(isDebug);
    }

    // Update is called once per frame
    async void Update()
    {
        if (!isDebug && !isRanning && Input.GetMouseButtonDown(0))
        {
            isRanning = true;
            if(!isLocalData) isLocalData = UserModel.Instance.LoadUserData();
            UnityAction errorActoin = OnErrorPanelButton;

            // ���[�J���ɂ��郆�[�U�[�����擾����
            if (isLocalData)
            {
                // ���[�U�[���擾����
                var error = await UserModel.Instance.ShowUserAsync(UserModel.Instance.UserId);

                if (error != null)
                {
                    ErrorUIController.Instance.ShowErrorUI(error, errorActoin);
                    return;
                }

                SceneControler.Instance.StartSceneLoad("TopScene");
            }
            else
            {
                // ���[�U�[�o�^����
                var result = await UserModel.Instance.RegistUserAsync(Guid.NewGuid().ToString("N").Substring(0, 8)); // �n�C�t���Ȃ���8����
                if (result != null)
                {
                    ErrorUIController.Instance.ShowErrorUI(result, errorActoin);
                    return;
                }
                isLocalData = true;

                // ���[�e�B���O�f�[�^��o�^
                await RatingModel.Instance.UpdateRatingAsync(UserModel.Instance.UserId, ConstantManager.DefaultRating);

                // �V�[���J�ڊJ�n
                SceneControler.Instance.StartSceneLoad("TopScene");
            }
        }
    }

    public void OnErrorPanelButton()
    {
        isRanning = false;
    }

    public void OnEnterDebugUI()
    {
        isDebug = true;
    }

    /// <summary>
    /// [�f�o�b�O�p] �w�肵�����[�U�[ID�����Ƀ��O�C��
    /// </summary>
    public async void OnDebugButtonAsync()
    {
        int userId;
        if(int.TryParse(inputFieldDebugId.text, out userId))
        {
            // ���[�U�[���擾����
            var error = await UserModel.Instance.ShowUserAsync(userId);
            if (error != null)
            {
                ErrorUIController.Instance.ShowErrorUI(error);
                return;
            }

            // �V�[���J�ڊJ�n
            SceneControler.Instance.StartSceneLoad("TopScene");
        }
    }
}
