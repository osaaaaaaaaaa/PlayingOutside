//*********************************************************
// 滑らかにシーン遷移させるスクリプト
// Author:Rui Enomoto
//*********************************************************
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
    public float FadeSecTime { get; private set; } = 0.5f;
    public bool isLoading { get; private set; }
    public static SceneControler Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            isLoading = false;

            // トップ画面の状態を保持する
            Instance = this;

            // シーン遷移しても破棄しないようにする
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // シーン遷移して新しく生成される自身を破棄
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
        // 一定時間に卵のUIを表示する
        foreach (var item in eggAnimSets)
        {
            item.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        // シーン遷移開始
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        // シーンを非同期でロードする
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);

        // ロードが完了するまで待機する
        while (true)
        {
            yield return null;

            if (!async.isDone)
            {
                if(FadeAudioController.Instance != null && !FadeAudioController.Instance.IsFadeing)
                {
                    break;
                }
                else if(FadeAudioController.Instance == null)
                {
                    break;
                }
            }
        }
    }

    public void StartSceneLoad(string sceneName)
    {
        if (isLoading) return;

        // BGMをフェードアウト
        var bgmController = Camera.main.gameObject.GetComponent<BGMController>();
        if( bgmController != null ) bgmController.StopAudio();

        isLoading = true;
        InitUI(true);

        this.sceneName = sceneName;

        // 徐々にαを上げるアニメーション
        var sequence = DOTween.Sequence();
        foreach (var item in images)
        {
            sequence.Join(item.GetComponent<Image>().DOFade(1, FadeSecTime).SetEase(Ease.Linear));
        }
        sequence.Play().OnComplete(() => 
        { 
            StartCoroutine(ShowEggAnimCortine()); 
        });
    }

    public void StopSceneLoad()
    {
        if(!isLoading) return;
        isLoading = false;

        // 徐々にαを下げるアニメーション
        var sequence = DOTween.Sequence();
        foreach (var item in images)
        {
            sequence.Join(item.GetComponent<Image>().DOFade(0, FadeSecTime).SetEase(Ease.Linear));
        }
        sequence.Play().OnComplete(() => { InitUI(false); });
    }
}
