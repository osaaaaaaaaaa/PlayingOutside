using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class ErrorUIController : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] Image bg;
    [SerializeField] GameObject errorContainer;
    [SerializeField] Text textError;
    [SerializeField] Button btnClose;

    public static ErrorUIController Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            // �g�b�v��ʂ̏�Ԃ�ێ�����
            Instance = this;

            // �V�[���J�ڂ��Ă��j�����Ȃ��悤�ɂ���
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // �V�[���J�ڂ��ĐV������������鎩�g��j��
            Destroy(gameObject);
        }
    }

    public void ShowErrorUI(string error)
    {
        textError.text = error;

        const float animSec = 0.25f;
        var sequence = DOTween.Sequence();
        sequence.Append(bg.DOFade(0.5f, animSec).SetEase(Ease.InOutCirc))
            .Join(errorContainer.transform.DOScale(Vector3.one, animSec).SetEase(Ease.OutBack).SetDelay(animSec - 0.05f));
        sequence.Play().OnComplete(() => { btnClose.interactable = true; });

        canvas.SetActive(true);
    }

    public void ShowErrorUI(string error,UnityAction action)
    {
        textError.text = error;
        btnClose.onClick.AddListener(action);

        const float animSec = 0.25f;
        var sequence = DOTween.Sequence();
        sequence.Append(bg.DOFade(0.5f, animSec).SetEase(Ease.InOutCirc))
            .Join(errorContainer.transform.DOScale(Vector3.one, animSec).SetEase(Ease.OutBack).SetDelay(animSec - 0.05f));
        sequence.Play().OnComplete(() => { btnClose.interactable = true; });

        canvas.SetActive(true);
    }

    public void HideErrorUI()
    {
        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(HideErrorUI);
        btnClose.interactable = false;
        const float animSec = 0.25f;
        var sequence = DOTween.Sequence();
        sequence.Append(errorContainer.transform.DOScale(Vector3.up, animSec).SetEase(Ease.InBack))
            .Join(bg.DOFade(0, animSec).SetEase(Ease.InOutCirc).SetDelay(animSec - 0.05f));
        sequence.Play().OnComplete(() => { canvas.SetActive(false); });
    }
}
