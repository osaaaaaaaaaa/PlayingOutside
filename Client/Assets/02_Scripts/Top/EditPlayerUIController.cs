using DG.Tweening;
using Server.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EditPlayerUIController : MonoBehaviour
{
    #region TweenアニメーションするUIの親
    [SerializeField] List<GameObject> uiList;
    #endregion

    [SerializeField] List<GameObject> characterProfiles;
    [SerializeField] List<GameObject> scrollViewList;
    [SerializeField] InputField userName;
    [SerializeField] GameObject editUserNameButton;
    [SerializeField] GameObject updateUserNameButton;
    [SerializeField] Text changeButtonText;
    [SerializeField] TopSceneCharacterManager characterManager;
    [SerializeField] TopSceneDirector topSceneDirector;
    TopSceneUIManager topSceneUIManager;

    private void Start()
    {
        foreach (var ui in uiList)
        {
            ui.transform.localScale = Vector3.zero;
        }
        topSceneUIManager = GetComponent<TopSceneUIManager>();
    }

    void ToggleUIVisibility(bool isVisibility)
    {
        Vector3 endScale = isVisibility ? Vector3.one : Vector3.zero;
        Ease setEase = isVisibility ? Ease.OutBack : Ease.InBack;

        foreach (var ui in uiList)
        {
            ui.transform.DOScale(endScale, 0.2f).SetEase(setEase);
        }

        // キャラクターのプロフィールを表示・非表示
        for (int i = 0; i < characterProfiles.Count; i++)
        {
            if (isVisibility)
            {
                int characterId = UserModel.Instance.CharacterId - 1;
                if (i == characterId) characterProfiles[i].SetActive(true);
                else characterProfiles[i].SetActive(false);
            }
        }

        userName.text = UserModel.Instance.UserName;
    }

    /// <summary>
    /// プレイヤー編集UIを表示するボタン
    /// </summary>
    public void OnSelectButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        userName.interactable = false;
        editUserNameButton.SetActive(true);
        editUserNameButton.GetComponent<Button>().interactable = true;
        updateUserNameButton.SetActive(false);
        updateUserNameButton.GetComponent<Button>().interactable = true;

        topSceneUIManager.OnSelectButton();
        ToggleUIVisibility(true);
        characterManager.DOMoveCharacter();
    }

    /// <summary>
    /// プレイヤー編集UIを非表示するボタン
    /// </summary>
    public void OnBackButton()
    {
        if (topSceneUIManager.IsTaskRunning) return;
        topSceneUIManager.IsTaskRunning = true;

        ToggleUIVisibility(false);
        topSceneUIManager.OnBackButton();
        characterManager.DOResetCharacter();
    }

    /// <summary>
    /// スクロールビューを切り替えるボタン
    /// </summary>
    public void OnChangeScrollViewButton()
    {
        foreach(var scrollView in scrollViewList)
        {
            // 表示・非表示切り替え
            scrollView.gameObject.SetActive(!scrollView.gameObject.activeSelf);
        }
        if (scrollViewList[0].gameObject.activeSelf) changeButtonText.text = "エモート";
        if (!scrollViewList[0].gameObject.activeSelf) changeButtonText.text = "キャラクター";
    }

    /// <summary>
    /// ユーザー名を編集するボタン
    /// </summary>
    public void OnEditUserNameButton()
    {
        userName.interactable = true;
        userName.Select();
        editUserNameButton.SetActive(false);
        updateUserNameButton.SetActive(true);
    }

    /// <summary>
    /// ユーザー名を変更するボタン
    /// </summary>
    public async void OnUpdateUserNameButtonAsync()
    {
        if (userName.text.Length <= 0 || userName.text == UserModel.Instance.UserName) return;
        if (NGWordModel.Instance.ContainsNGWord(userName.text))
        {
            ErrorUIController.Instance.ShowErrorUI("使用できないワードが含まれています。");
            return;
        }

        updateUserNameButton.GetComponent<Button>().interactable = false;
        var user = new User()
        {
            Id = UserModel.Instance.UserId,
            Name = userName.text,
            Token = UserModel.Instance.AuthToken,
            Character_Id = UserModel.Instance.CharacterId,
        };

        var result = await UserModel.Instance.UpdateUserAsync(user);
        if (result != null)
        {
            ErrorUIController.Instance.ShowErrorUI(result);
            updateUserNameButton.GetComponent<Button>().interactable = true;
            return;
        }

        topSceneUIManager.SetUserNameText();
        updateUserNameButton.SetActive(false);
        updateUserNameButton.GetComponent<Button>().interactable = true;
        editUserNameButton.SetActive(true);
        userName.interactable = false;
    }

    /// <summary>
    /// キャラクターID変更ボタン
    /// </summary>
    public async void OnUpdateCharacterIDButton(int characterId)
    {
        var user = new User()
        {
            Id = UserModel.Instance.UserId,
            Name = UserModel.Instance.UserName,
            Token = UserModel.Instance.AuthToken,
            Character_Id = characterId
        };

        var result = await UserModel.Instance.UpdateUserAsync(user);
        if (result != null)
        {
            ErrorUIController.Instance.ShowErrorUI(result);
            updateUserNameButton.GetComponent<Button>().interactable = true;
            return;
        }

        // キャラクターのプロフィールを変更する
        for(int i = 0; i < characterProfiles.Count; i++)
        {
            if (i == characterId - 1) characterProfiles[i].SetActive(true);
            else characterProfiles[i].SetActive(false);
        }

        characterManager.ToggleCharacter(characterId);
    }
}
