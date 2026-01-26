using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SkyTintTrigger : MonoBehaviour
{
    public enum TintMode { Desert, Normal }
    public TintMode mode;

    [Header("Tint Settings")]
    public Color desertColor = new Color(1f, 0.85f, 0.4f); // warm yellow
    public float intensity = 0.5f; // how strong the tint is
    public float transitionSpeed = 2f;

    private static Volume globalVolume;
    private static ColorAdjustments colorAdjust;

    void Start()
    {
        // Find the global volume once
        if (globalVolume == null)
        {
            globalVolume = FindObjectOfType<Volume>();

            if (globalVolume != null)
                globalVolume.profile.TryGet(out colorAdjust);
        }

        // ⭐ Force normal tint at scene start
        if (colorAdjust != null)
        {
            colorAdjust.colorFilter.value = Color.white;
            colorAdjust.postExposure.value = 0f;
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (colorAdjust == null)
            return;

        if (mode == TintMode.Desert)
            StartCoroutine(SetTint(desertColor, intensity));
        else
            StartCoroutine(SetTint(Color.white, 0f));
    }

    System.Collections.IEnumerator SetTint(Color targetColor, float targetIntensity)
    {
        float t = 0f;

        Color startColor = colorAdjust.colorFilter.value;
        float startIntensity = colorAdjust.postExposure.value;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;

            colorAdjust.colorFilter.value = Color.Lerp(startColor, targetColor, t);
            colorAdjust.postExposure.value = Mathf.Lerp(startIntensity, targetIntensity, t);

            yield return null;
        }
    }
}
