using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;

    public Image fadeImage;
    public float fadeTime = 1f;

    void Awake()
    {
        instance = this;
    }

    public IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0, 1, t / fadeTime);
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1, 0, t / fadeTime);
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }
    }
}
