using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class FadeAudioController : MonoBehaviour
{
    public bool IsFadeing {  get; private set; }
    const float fadeDuration = 0.5f;

    // インスタンス作成
    public static FadeAudioController Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayFadeAudio(GameObject obj, bool isFadeIn)
    {
        var source = obj.GetComponent<AudioSource>();
        if (source == null) return;
        if (!source.isPlaying) source.Play();
        StartCoroutine(FadeCoroutine(source, isFadeIn));
    }

    IEnumerator FadeCoroutine(AudioSource source, bool isFadeIn)
    {
        float maxVolume = source.volume;
        source.volume = isFadeIn ? 0 : maxVolume;

        IsFadeing = true;
        const float waitSec = 0.1f;
        int fadeMaxCnt = (int)(fadeDuration / waitSec);
        for (int i = 0; i < fadeMaxCnt; i++)
        {
            float value = (1f / fadeMaxCnt);
            source.volume += isFadeIn ? value : -value;

            yield return new WaitForSeconds(waitSec);
        }

        source.volume = isFadeIn ? maxVolume : 0;
        IsFadeing = false;
    }
}
