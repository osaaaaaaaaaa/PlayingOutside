using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TopSceneUIManager : MonoBehaviour
{
    #region オーディオ関係
    [SerializeField] AudioClip selectSE;
    [SerializeField] AudioClip closeSE;
    SEController seController;
    #endregion

    [SerializeField] Text userNameText;

    [SerializeField] List<GameObject> selectButtonList;
    [SerializeField] GameObject textUserName;

    [SerializeField] List<Sprite> spriteIcons;
    public List<Sprite> SpriteIcons { get { return spriteIcons; } }

    // Tweenアニメーション中かどうか
    bool isTaskRunning  = false;
    public bool IsTaskRunning { get { return isTaskRunning; } set { isTaskRunning = value; } }

    private void Start()
    {
        seController = GetComponent<SEController>();
        SetUserNameText();
    }

    public void SetUserNameText()
    {
        userNameText.text = UserModel.Instance.UserName;
    }

    void ToggleUIVisibility(bool isVisibility)
    {
        Vector3 endScale = isVisibility ? Vector3.one : Vector3.zero;
        Ease setEase = isVisibility ? Ease.OutBack : Ease.InBack;

        var sequence = DOTween.Sequence();

        foreach (var button in selectButtonList)
        {
            sequence.Join(button.transform.DOScale(endScale, 0.2f).SetEase(setEase));
        }
        sequence.Join(textUserName.transform.DOScale(endScale, 0.2f).SetEase(setEase));

        sequence.Play().OnComplete(() => { isTaskRunning = false; });
    }

    public virtual void OnSelectButton()
    {
        seController.PlayAudio(selectSE);
        ToggleUIVisibility(false);
    }

    public virtual void OnBackButton()
    {
        seController.PlayAudio(closeSE);
        ToggleUIVisibility(true);
    }
}
