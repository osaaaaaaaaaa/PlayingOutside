using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using TedLab;

public class SceneControler : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] List<GameObject> images;
    [SerializeField] List<GameObject> eggAnimSets;
    string sceneName;
    public static SceneControler Instance;

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

    void InitUI(bool isActive)
    {
        canvas.SetActive(isActive);
        foreach (var item in images) 
        { 
            item.gameObject.SetActive(isActive);
        }

        foreach (var item in eggAnimSets)
        {
            item.gameObject.SetActive(false);
        }
    }

    IEnumerator ShowEggAnimCortine()
    {
        // ��莞�Ԃɗ���UI��\������
        foreach (var item in eggAnimSets)
        {
            item.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        // �V�[���J�ڊJ�n
        StartCoroutine(Load());
        //SceneManager.LoadScene(sceneName);
    }

    IEnumerator Load()
    {
        // �V�[����񓯊��Ń��[�h����
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);

        // ���[�h����������܂őҋ@����
        while (!async.isDone)
        {
            yield return null;
        }

        // ���Ŕz�u
        StopSceneLoad();
    }

    public void StartSceneLoad(string sceneName)
    {
        InitUI(true);

        this.sceneName = sceneName;

        // ���X�Ƀ����グ��A�j���[�V����
        var sequence = DOTween.Sequence();
        foreach (var item in images)
        {
            sequence.Join(item.GetComponent<Image>().DOFade(1, 0.5f).SetEase(Ease.Linear));
        }
        sequence.Play().OnComplete(() => 
        { 
            StartCoroutine(ShowEggAnimCortine()); 
        });
    }

    public void StopSceneLoad()
    {
        // ���X�Ƀ���������A�j���[�V����
        var sequence = DOTween.Sequence();
        foreach (var item in images)
        {
            sequence.Join(item.GetComponent<Image>().DOFade(0, 0.5f).SetEase(Ease.Linear));
        }
        sequence.Play().OnComplete(() => { InitUI(false); });
    }
}
